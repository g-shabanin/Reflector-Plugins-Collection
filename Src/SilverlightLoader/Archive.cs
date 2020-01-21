namespace Reflector.SilverlightLoader
{
	using System;
	using System.IO;
	using System.Text;

	internal class Archive
	{
		private Stream stream;

		public Archive(Stream stream)
		{
			this.stream = stream;
		}

		public ArchiveItem Read()
		{
			int signature = this.ReadInt32();
			if ((signature == 0x02014B50) || (signature == 0x06054B50) || (signature == 0x05054B50) || (signature == 0x06064B50))
			{
				return null;
			}

			if (signature == 0x08074B50 || signature == 0x30304B50)
			{
				signature = this.ReadInt32();
			}

			if (signature != 0x04034B50) 
			{
				throw new InvalidOperationException("ZIP decompression: Invalid signature.");
			}

			ArchiveItem item = new ArchiveItem();
			this.ReadInt16(); // Version
			int flags = this.ReadInt16(); // Flags
			int compressionMethod = this.ReadInt16(); // CompressionMethod
			this.ReadInt32(); // DosTime
			this.ReadInt32();
			int compressedSize = this.ReadInt32(); // CompressedSize
			int size = this.ReadInt32(); // Size
			int nameLength = this.ReadInt16();
			int extraDataLength = this.ReadInt16();
			byte[] nameBuffer = this.ReadBytes(nameLength); // Name
			item.Name = Encoding.ASCII.GetString(nameBuffer, 0, nameBuffer.Length);
			this.ReadBytes(extraDataLength); // ExtraData

			if ((flags & 1) == 1) // Crypted
			{
				throw new NotSupportedException("ZIP decompression: File is crypted.");
			}

			if ((flags & 8) != 0)
			{
				throw new NotSupportedException("ZIP decompression: File type is not supported.");
			}

			switch (compressionMethod)
			{
				case 0: // Stored
					if (compressedSize != size)
					{
						throw new InvalidOperationException("ZIP decompression: Invalid compression size.");
					}

					item.Value = this.ReadBytes(compressedSize);
					break;

				case 8: // Deflated
					Inflater inflater = new Inflater(this.ReadBytes(compressedSize));
					item.Value = inflater.Inflate(size);
					if (item.Value == null)
					{
						throw new NotSupportedException("ZIP decompression: Invalid state.");
					}
					break;

				default:
					throw new InvalidOperationException("ZIP decompression: Invalid compression method.");
			}

			return item;
		}

		private byte[] ReadBytes(int count)
		{
			byte[] buffer = new byte[count];
			if (this.stream.Read(buffer, 0, count) != count)
			{
				throw new InvalidOperationException("ZIP decompression: No data available.");	
			}
			return buffer;
		}

		private int ReadInt16()
		{
			byte[] buffer = this.ReadBytes(2);
			return buffer[0] | (buffer[1] << 8);
		}
		
		private int ReadInt32()
		{
			return this.ReadInt16() | (this.ReadInt16() << 16);
		}

		private class Inflater
		{
			private static int[] literalCopyLengths = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31, 35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258 };
			private static int[] literalExtraBits = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0 };
			private static int[] distanceCopyOffsets = { 1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193, 257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577 };
			private static int[] distanceExtraBits = { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 13, 13 };

			private DecodeMode mode;
			private bool lastBlock;
			private int neededBits;
			private int repeatLength;
			private int repeatDistance;
			private int uncompressedLength;
		
			private BitReader reader;
			private DynamicHeader dynamicHeader;
			private HuffmanTree literalLengthTree;
			private HuffmanTree distanceTree;

			private static int size = 0x8000;
			private static int mask = 0x7fff;
			private byte[] buffer = new byte[size];
			private int position  = 0;
			private int available = 0;
			
			public Inflater(byte[] buffer)
			{
				this.reader = new BitReader();
				this.reader.SetInput(buffer, 0, buffer.Length);
				this.mode = DecodeMode.Blocks;
			}

			public byte[] Inflate(int size)
			{
				byte[] buffer = new byte[size];
				int count = 0;

				int offset = 0;
				int length = size;
				do
				{
					int more = this.CopyOutput(buffer, offset, length);
					offset += more;
					count += more;
					length -= more;
					if (length < 0)
					{
						break;
					}
				}
				while (this.Decode() || (this.available > 0));

				if ((this.mode != DecodeMode.Finished) || (this.available > 0) || (count != size) || (this.reader.AvailableBytes > 0))
				{
					return null;
				}

				return buffer;
			}

			private bool Decode()
			{
				switch (this.mode) 
				{
					case DecodeMode.Header:
						int header = this.reader.ReadBits(16);
						if (header >= 0) 
						{
							this.reader.SkipBits(16);
							header = ((header << 8) | (header >> 8)) & 0xffff;

							if ((header % 31 != 0) || ((header & 0x0f00) != (8 << 8)))
							{
								throw new InvalidOperationException();
							}
							
							if ((header & 0x0020) == 0) 
							{
								this.mode = DecodeMode.Blocks;
							} 
							else 
							{
								this.mode = DecodeMode.Dictionary;
								this.neededBits = 32;
							}
							return true;
						}
						return false;

					case DecodeMode.Dictionary:
						while (this.neededBits > 0) 
						{
							int value = this.reader.ReadBits(8);
							if (value < 0) 
							{
								break;
							}

							this.reader.SkipBits(8);
							this.neededBits -= 8;
						}
						return false;

					case DecodeMode.Blocks:
						if (lastBlock) 
						{
							this.mode = DecodeMode.Finished;
							return false;
						}

						int type = reader.ReadBits(3);
						if (type < 0) 
						{
							return false;
						}

						this.reader.SkipBits(3);
						if ((type & 1) != 0) 
						{
							lastBlock = true;
						}
	
						switch (type >> 1)
						{
							case 0:
								this.reader.Align();
								this.mode = DecodeMode.StoredLength1;
								break;

							case 1:
								byte[] codeLengths = new byte[288];
								int i = 0;
								while (i < 144) 
								{
									codeLengths[i++] = 8;
								}
								while (i < 256) 
								{
									codeLengths[i++] = 9;
								}
								while (i < 280) 
								{
									codeLengths[i++] = 7;
								}
								while (i < 288) 
								{
									codeLengths[i++] = 8;
								}
								this.literalLengthTree = new HuffmanTree(codeLengths);
								
								codeLengths = new byte[32];
								i = 0;
								while (i < 32) 
								{
									codeLengths[i++] = 5;
								}
								this.distanceTree = new HuffmanTree(codeLengths);

								this.mode = DecodeMode.Huffman;
								break;

							case 2:
								this.dynamicHeader = new DynamicHeader();
								this.mode = DecodeMode.DynamicHeader;
								break;

							default:
								throw new NotSupportedException("ZIP decompression: Invalid inflater block type.");
						}

						return true;

					case DecodeMode.StoredLength1: 
						{
							if ((this.uncompressedLength = this.reader.ReadBits(16)) < 0) 
							{
								return false;
							}
							this.reader.SkipBits(16);
							this.mode = DecodeMode.StoredLength2;
						}
						goto case DecodeMode.StoredLength2;

					case DecodeMode.StoredLength2:
						{
							int nlen = this.reader.ReadBits(16);
							if (nlen < 0) 
							{
								return false;
							}
							this.reader.SkipBits(16);
							if (nlen != (this.uncompressedLength ^ 0xffff)) 
							{
								throw new FormatException();
							}
							this.mode = DecodeMode.Stored;
						}
						goto case DecodeMode.Stored;

					case DecodeMode.Stored: 
						{
							int more = this.Copy(this.reader, this.uncompressedLength);
							this.uncompressedLength -= more;
							if (this.uncompressedLength == 0) 
							{
								this.mode = DecodeMode.Blocks;
								return true;
							}
							return !this.reader.NeedInput;
						}

					case DecodeMode.DynamicHeader:
						if (!this.dynamicHeader.Decode(this.reader)) 
						{
							return false;
						}

						this.literalLengthTree = this.dynamicHeader.CreateLiteralLengthTree();
						this.distanceTree = this.dynamicHeader.CreateDistanceTree();
						this.mode = DecodeMode.Huffman;
						goto case DecodeMode.Huffman;

					case DecodeMode.Huffman:
					case DecodeMode.HuffmanLengthBits:
					case DecodeMode.HuffmanDist:
					case DecodeMode.HuffmanDistBits:
						return this.DecodeHuffman();

					case DecodeMode.Finished:
						return false;

					default:
						throw new FormatException();
				}
			}
			
			private bool DecodeHuffman()
			{
				int free = (size - this.available);
				while (free >= 258) 
				{
					int symbol;
					switch (this.mode) 
					{
						case DecodeMode.Huffman:
							while (((symbol = this.literalLengthTree.ReadSymbol(this.reader)) & ~0xff) == 0) 
							{
								this.available++;
								if (this.available == size) 
								{
									throw new InvalidOperationException();
								}
								this.buffer[this.position++] = (byte) symbol;
								this.position &= mask;

								if (--free < 258) 
								{
									return true;
								}
							}
							if (symbol < 257) 
							{
								if (symbol < 0) 
								{
									return false;
								} 
								else 
								{
									this.distanceTree = null;
									this.literalLengthTree = null;
									this.mode = DecodeMode.Blocks;
									return true;
								}
							}
							
							this.repeatLength = literalCopyLengths[symbol - 257];
							neededBits = literalExtraBits[symbol - 257];
							goto case DecodeMode.HuffmanLengthBits;

						case DecodeMode.HuffmanLengthBits:
							if (neededBits > 0) 
							{
								this.mode = DecodeMode.HuffmanLengthBits;
								int i = this.reader.ReadBits(neededBits);
								if (i < 0) 
								{
									return false;
								}
								this.reader.SkipBits(neededBits);
								this.repeatLength += i;
							}
							this.mode = DecodeMode.HuffmanDist;
							goto case DecodeMode.HuffmanDist;

						case DecodeMode.HuffmanDist:
							symbol = this.distanceTree.ReadSymbol(this.reader);
							if (symbol < 0) 
							{
								return false;
							}
	
							this.repeatDistance = distanceCopyOffsets[symbol];
							neededBits = distanceExtraBits[symbol];
							goto case DecodeMode.HuffmanDistBits;

						case DecodeMode.HuffmanDistBits:
							if (neededBits > 0) 
							{
								this.mode = DecodeMode.HuffmanDistBits;
								int i = this.reader.ReadBits(neededBits);
								if (i < 0) 
								{
									return false;
								}
								this.reader.SkipBits(neededBits);
								this.repeatDistance += i;
							}
							this.Repeat(this.repeatLength, this.repeatDistance);
							free -= this.repeatLength;
							this.mode = DecodeMode.Huffman;
							break;

						default:
							throw new InvalidOperationException();
					}
				}
				return true;
			}

			private void Repeat(int length, int dist)
			{
				this.available += length;
				if ((this.available) > size) 
				{
					throw new InvalidOperationException();
				}
				
				int repeatStart = (this.position - dist) & mask;
				int border = size - length;
				if ((repeatStart <= border) && (this.position < border))
				{
					if (length <= dist) 
					{
						Array.Copy(this.buffer, repeatStart, this.buffer, this.position, length);
						this.position += length;
					} 
					else 
					{
						while (length-- > 0) 
						{
							this.buffer[this.position++] = this.buffer[repeatStart++];
						}
					}
				} 
				else 
				{
					while (length-- > 0) 
					{
						this.buffer[this.position++] = this.buffer[repeatStart++];
						this.position &= mask;
						repeatStart &= mask;
					}
				}
			}

			private int Copy(BitReader input, int length)
			{
				length = Math.Min(Math.Min(length, size - this.available), input.AvailableBytes);
				int copied;

				int tailLen = size - this.position;
				if (length > tailLen) 
				{
					copied = input.CopyBytes(this.buffer, this.position, tailLen);
					if (copied == tailLen) 
					{
						copied += input.CopyBytes(this.buffer, 0, length - tailLen);
					}
				} 
				else 
				{
					copied = input.CopyBytes(this.buffer, this.position, length);
				}
				
				this.position = (this.position + copied) & mask;
				this.available += copied;
				return copied;
			}
						
			private int CopyOutput(byte[] output, int offset, int length)
			{
				int end = this.position;
				if (length > this.available) 
				{
					length = this.available;
				} 
				else 
				{
					end = (this.position - this.available + length) & mask;
				}
				
				int copied = length;
				int tailLength = length - end;
				if (tailLength > 0) 
				{
					Array.Copy(this.buffer, size - tailLength, output, offset, tailLength);
					offset += tailLength;
					length = end;
				}

				Array.Copy(this.buffer, end - length, output, offset, length);
				this.available -= copied;
				if (this.available < 0) 
				{
					throw new InvalidOperationException();
				}

				return copied;
			}

			private enum DecodeMode
			{
				Header = 0,
				Dictionary = 1,
				Blocks = 2,
				StoredLength1 = 3,
				StoredLength2 = 4,
				Stored = 5,
				DynamicHeader = 6,
				Huffman = 7,
				HuffmanLengthBits = 8,
				HuffmanDist = 9,
				HuffmanDistBits = 10,
				Finished = 11
			}

			private class DynamicHeader
			{
				private static readonly int[] repMin = { 3, 3, 11 };
				private static readonly int[] repBits = { 2, 3, 7 };
				private static readonly int[] blOrder = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

				private byte[] blLens;
				private byte[] litdistLens;

				private HuffmanTree blTree;
				private int mode;
				private int lnum;
				private int dnum;
				private int blnum;
				private int repSymbol;
				private byte lastLen;
				private int position;
				private int num;

				public bool Decode(BitReader reader)
				{
					while (true)
					{
						if (this.mode == 0) // LNUM
						{
							this.lnum = reader.ReadBits(5);
							if (this.lnum < 0)
							{
								return false;
							}
							this.lnum += 257;
							reader.SkipBits(5);
							this.mode = 1; // DNUM
						}

						if (this.mode == 1) // DNUM
						{
							this.dnum = reader.ReadBits(5);
							if (this.dnum < 0)
							{
								return false;
							}
							this.dnum++;
							reader.SkipBits(5);
							this.num = this.lnum + this.dnum;
							this.litdistLens = new byte[this.num];
							this.mode = 2; // BLNUM
						}

						if (this.mode == 2) // BLNUM
						{
							this.blnum = reader.ReadBits(4);
							if (this.blnum < 0)
							{
								return false;
							}

							this.blnum += 4;
							reader.SkipBits(4);
							this.blLens = new byte[19];
							this.position = 0;
							this.mode = 3; // BLLENS
						}

						if (this.mode == 3) // BLLENS
						{
							while (this.position < this.blnum)
							{
								int length = reader.ReadBits(3);
								if (length < 0)
								{
									return false;
								}

								reader.SkipBits(3);
								this.blLens[blOrder[position]] = (byte)length;
								this.position++;
							}

							this.blTree = new HuffmanTree(blLens);
							this.blLens = null;
							this.position = 0;
							this.mode = 4; // LENS
						}

						if (this.mode == 4) // LENS
						{
							int symbol;
							while (((symbol = blTree.ReadSymbol(reader)) & ~15) == 0)
							{
								this.litdistLens[position++] = lastLen = (byte)symbol;
								if (position == num)
								{
									return true;
								}
							}

							if (symbol < 0)
							{
								return false;
							}

							if (symbol >= 17)
							{
								lastLen = 0;
							}
							else
							{
								if (position == 0)
								{
									throw new InvalidOperationException();
								}
							}

							this.repSymbol = symbol - 16;
							this.mode = 5; // REPS
						}

						if (this.mode == 5) // REPS
						{
							int bits = repBits[repSymbol];
							int count = reader.ReadBits(bits);
							if (count < 0)
							{
								return false;
							}

							reader.SkipBits(bits);
							count += repMin[repSymbol];

							if ((position + count) > num)
							{
								throw new InvalidOperationException();
							}

							while (count-- > 0)
							{
								this.litdistLens[position++] = lastLen;
							}

							if (position == num)
							{
								return true;
							}

							this.mode = 4; // LENS
						}
					}
				}

				public HuffmanTree CreateLiteralLengthTree()
				{
					byte[] litlenLens = new byte[this.lnum];
					Array.Copy(this.litdistLens, 0, litlenLens, 0, this.lnum);
					return new HuffmanTree(litlenLens);
				}

				public HuffmanTree CreateDistanceTree()
				{
					byte[] distLens = new byte[this.dnum];
					Array.Copy(this.litdistLens, this.lnum, distLens, 0, this.dnum);
					return new HuffmanTree(distLens);
				}
			}

			private class HuffmanTree
			{
				private short[] buffer;

				public HuffmanTree(byte[] codeLengths)
				{
					byte[] bitTable = { 0, 8, 4, 12, 2, 10, 6, 14, 1, 9, 5, 13, 3, 11, 7, 15 };
					int[] blCount = new int[16];
					int[] nextCode = new int[16];

					for (int i = 0; i < codeLengths.Length; i++)
					{
						int bits = codeLengths[i];
						if (bits > 0)
						{
							blCount[bits]++;
						}
					}

					int code = 0;
					int treeSize = 512;
					for (int bits = 1; bits <= 15; bits++)
					{
						nextCode[bits] = code;
						code += blCount[bits] << (16 - bits);
						if (bits >= 10)
						{
							int start = nextCode[bits] & 0x1ff80;
							int end = code & 0x1ff80;
							treeSize += (end - start) >> (16 - bits);
						}
					}

					this.buffer = new short[treeSize];
					int treePtr = 512;
					for (int bits = 15; bits >= 10; bits--)
					{
						int end = code & 0x1ff80;
						code -= blCount[bits] << (16 - bits);
						int start = code & 0x1ff80;
						for (int i = start; i < end; i += 1 << 7)
						{
							this.buffer[BitReverse(i, bitTable)] = (short)((-treePtr << 4) | bits);
							treePtr += 1 << (bits - 9);
						}
					}

					for (int i = 0; i < codeLengths.Length; i++)
					{
						int bits = codeLengths[i];
						if (bits == 0)
						{
							continue;
						}
						code = nextCode[bits];
						int revcode = BitReverse(code, bitTable);
						if (bits <= 9)
						{
							do
							{
								this.buffer[revcode] = (short)((i << 4) | bits);
								revcode += 1 << bits;
							}
							while (revcode < 512);
						}
						else
						{
							int subTree = this.buffer[revcode & 511];
							int treeLen = 1 << (subTree & 15);
							subTree = -(subTree >> 4);
							do
							{
								this.buffer[subTree | (revcode >> 9)] = (short)((i << 4) | bits);
								revcode += 1 << bits;
							}
							while (revcode < treeLen);
						}

						nextCode[bits] = code + (1 << (16 - bits));
					}
				}

				public int ReadSymbol(BitReader reader)
				{
					int peek = reader.ReadBits(9);
					int symbol;
					if (peek >= 0)
					{
						if ((symbol = this.buffer[peek]) >= 0)
						{
							reader.SkipBits(symbol & 15);
							return symbol >> 4;
						}

						int subtree = -(symbol >> 4);
						int bitLength = symbol & 15;
						if ((peek = reader.ReadBits(bitLength)) >= 0)
						{
							symbol = this.buffer[subtree | (peek >> 9)];
							reader.SkipBits(symbol & 15);
							return symbol >> 4;
						}
						else
						{
							int bits = reader.AvailableBits;
							peek = reader.ReadBits(bits);
							symbol = this.buffer[subtree | (peek >> 9)];
							if ((symbol & 15) <= bits)
							{
								reader.SkipBits(symbol & 15);
								return symbol >> 4;
							}

							return -1;
						}
					}
					else
					{
						int bits = reader.AvailableBits;
						peek = reader.ReadBits(bits);
						symbol = this.buffer[peek];
						if (symbol >= 0 && (symbol & 15) <= bits)
						{
							reader.SkipBits(symbol & 15);
							return symbol >> 4;
						}

						return -1;
					}
				}

				private static short BitReverse(int value, byte[] table)
				{
					return (short)(table[value & 0x0f] << 12 | table[(value >> 4) & 0x0f] << 8 | table[(value >> 8) & 0x0f] << 4 | table[value >> 12]);
				}
			}

			private class BitReader
			{
				private byte[] buffer;
				private int position;
				private int length;
				private uint value;
				private int bits;

				public int ReadBits(int count)
				{
					if (this.bits < count)
					{
						if (this.position == this.length)
						{
							return -1;
						}

						this.value |= (uint)((this.buffer[this.position++] & 0xff | (this.buffer[this.position++] & 0xff) << 8) << this.bits);
						this.bits += 16;
					}

					return (int)(this.value & ((1 << count) - 1));
				}

				public void SkipBits(int count)
				{
					this.value >>= count;
					this.bits -= count;
				}

				public void Align()
				{
					this.value >>= (this.bits & 7);
					this.bits &= ~7;
				}

				public int CopyBytes(byte[] buffer, int offset, int length)
				{
					if (length < 0)
					{
						throw new ArgumentOutOfRangeException("length");
					}

					if ((this.bits & 7) != 0)
					{
						throw new InvalidOperationException();
					}

					int count = 0;
					while ((this.bits > 0) && (length > 0))
					{
						buffer[offset++] = (byte)this.value;
						this.value >>= 8;
						this.bits -= 8;
						length--;
						count++;
					}

					if (length != 0)
					{
						int available = this.length - this.position;
						if (length > available)
						{
							length = available;
						}

						Array.Copy(this.buffer, this.position, buffer, offset, length);
						this.position += length;

						if (((this.position - this.length) & 1) != 0)
						{
							this.value = (uint)(this.buffer[this.position++] & 0xff);
							this.bits = 8;
						}
					}

					return (count + length);
				}

				public void SetInput(byte[] buffer, int offset, int length)
				{
					if (this.position < this.length)
					{
						throw new InvalidOperationException();
					}

					int end = offset + length;
					if ((offset < 0) || (offset > end))
					{
						throw new ArgumentOutOfRangeException("offset");
					}

					if (end > buffer.Length)
					{
						throw new ArgumentOutOfRangeException("length");
					}

					if ((length & 1) != 0)
					{
						this.value |= (uint)((buffer[offset++] & 0xff) << this.bits);
						this.bits += 8;
					}

					this.buffer = buffer;
					this.position = offset;
					this.length = end;
				}

				public int AvailableBits
				{
					get
					{
						return this.bits;
					}
				}

				public int AvailableBytes
				{
					get
					{
						return this.length - this.position + (this.bits >> 3);
					}
				}

				public bool NeedInput
				{
					get
					{
						return this.position == this.length;
					}
				}
			}
		}
	}
}
