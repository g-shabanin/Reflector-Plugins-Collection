namespace Reflector.SilverlightLoader
{
	using System.Collections;
	using System.Globalization;
	using System.Text;

	internal abstract class HtmlNode
	{
		private HtmlElement parent = null;

		protected HtmlNode()
		{
		}

		public HtmlElement Parent
		{
			get
			{
				return this.parent;
			}
		}

		internal void SetParent(HtmlElement value)
		{
			this.parent = value;
		}
	}

	internal sealed class HtmlNodeCollection : IEnumerable
	{
		private HtmlElement parent;
		private ArrayList list;

		internal HtmlNodeCollection(HtmlElement parent)
		{
			this.parent = parent;
			this.list = new ArrayList();
		}

		public IEnumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		public int Count
		{
			get
			{
				return this.list.Count;
			}
		}

		public void Add(HtmlNode value)
		{
			if (this.parent != null)
			{
				if (value.Parent != null)
				{
					value.Parent.Nodes.Remove(value);
				}

				value.SetParent(this.parent);
			}

			this.list.Add(value);
		}

		public void Insert(int index, HtmlNode value)
		{
			if (this.parent != null)
			{
				if (value.Parent != null)
				{
					value.Parent.Nodes.Remove(value);
				}

				value.SetParent(this.parent);
			}

			this.list.Insert(index, value);
		}

		public void Clear()
		{
			if (this.parent != null)
			{
				for (int i = 0; i < this.list.Count; i++)
				{
					HtmlNode value = (HtmlNode)this.list[i];
					value.SetParent(null);
				}
			}

			this.list.Clear();
		}

		public void Remove(HtmlNode value)
		{
			if (this.parent != null)
			{
				value.SetParent(null);
			}

			this.list.Remove(value);
		}

		public void RemoveAt(int index)
		{
			if (this.parent != null)
			{
				HtmlNode value = (HtmlNode)this.list[index];
				value.SetParent(null);
			}

			this.list.RemoveAt(index);
		}

		public HtmlNode this[int index]
		{
			get
			{
				return (HtmlNode)this.list[index];
			}
		}

		public HtmlNode this[string name]
		{
			get
			{
				name = name.Trim().ToLower();
				for (int i = 0; i < this.list.Count; i++)
				{
					HtmlElement element = this.list[i] as HtmlElement;
					if ((element != null) && (element.Name == name))
					{
						return element;
					}
				}

				return null;
			}
		}

		public int IndexOf(HtmlNode node)
		{
			return this.list.IndexOf(node);
		}
	}

	internal sealed class HtmlAttribute
	{
		private string name;
		private string value;

		public HtmlAttribute(string name)
		{
			this.name = name.Trim().ToLower();
			this.value = string.Empty;
		}

		public string Name
		{
			get { return this.name; }
		}

		public string Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		public override string ToString()
		{
			return string.Format("{0}='{1}'", this.name, HtmlDocument.TranslateTextToHtml(this.value, false));
		}
	}

	internal sealed class HtmlAttributeCollection : IEnumerable
	{
		private ArrayList list = new ArrayList();

		public IEnumerator GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		public int Count
		{
			get
			{
				return this.list.Count;
			}
		}

		public void Add(HtmlAttribute value)
		{
			this.list.Add(value);
		}

		public void Insert(int index, HtmlAttribute value)
		{
			this.list.Insert(index, value);
		}

		public void Clear()
		{
			this.list.Clear();
		}

		public void Remove(HtmlAttribute value)
		{
			this.list.Remove(value);
		}

		public void RemoveAt(int index)
		{
			this.list.RemoveAt(index);
		}

		public HtmlAttribute this[int index]
		{
			get
			{
				return (HtmlAttribute)this.list[index];
			}
		}

		public HtmlAttribute this[string name]
		{
			get
			{
				for (int i = 0; i < this.list.Count; i++)
				{
					HtmlAttribute attribute = (HtmlAttribute)this.list[i];
					if (attribute.Name == name)
					{
						return attribute;
					}
				}

				return null;
			}
		}
	}

	internal sealed class HtmlElement : HtmlNode
	{
		private string name;
		private HtmlAttributeCollection attributes;
		private HtmlNodeCollection nodes;
		private bool terminated;

		public HtmlElement(string name)
		{
			this.name = name.Trim().ToLower();
			this.terminated = true;
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public HtmlAttributeCollection Attributes
		{
			get
			{
				if (this.attributes == null)
				{
					this.attributes = new HtmlAttributeCollection();
				}

				return this.attributes;
			}
		}

		public HtmlNodeCollection Nodes
		{
			get
			{
				if (this.nodes == null)
				{
					this.nodes = new HtmlNodeCollection(this);
				}

				return this.nodes;
			}
		}

		public string InnerText
		{
			get
			{
				StringBuilder builder = new StringBuilder();

				for (int i = 0; i < this.Nodes.Count; i++)
				{
					HtmlText text = this.Nodes[i] as HtmlText;
					if (text != null)
					{
						builder.Append(text.Text);
					}
				}

				return builder.ToString();
			}
		}

		public bool Terminated
		{
			get
			{
				return this.terminated;
			}

			set
			{
				this.terminated = value;
			}
		}

		public override string ToString()
		{
			StringBuilder writer = new StringBuilder();
			writer.Append("<" + this.name);

			if (this.attributes != null)
			{
				for (int i = 0; i < this.attributes.Count; i++)
				{
					writer.Append(" " + this.attributes[i].ToString());
				}
			}

			if ((this.nodes != null) && (this.nodes.Count > 0))
			{
				writer.Append(">");
				for (int i = 0; i < this.nodes.Count; i++)
				{
					writer.Append(this.nodes[i].ToString());
				}
				writer.Append("</" + this.name + ">");
			}
			else if (this.terminated)
			{
				writer.Append("/>");
			}
			else
			{
				writer.Append(">");
			}

			return writer.ToString();
		}
	}

	internal sealed class HtmlComment : HtmlNode
	{
		private string comment = string.Empty;

		public HtmlComment()
		{
		}

		public string Comment
		{
			get
			{
				return this.comment;
			}

			set
			{
				this.comment = value;
			}
		}

		public override string ToString()
		{
			return "<!" + this.comment + ">";
		}
	}

	internal sealed class HtmlProcessingInstruction : HtmlNode
	{
		private string value = string.Empty;

		public string Value
		{
			get
			{
				return this.value;
			}

			set
			{
				this.value = value;
			}
		}

		public override string ToString()
		{
			return "<?" + this.value + "?>";
		}
	}

	internal class HtmlCharacterData : HtmlNode
	{
		private string text = string.Empty;

		public string Text
		{
			get
			{
				return this.text;
			}

			set
			{
				this.text = value;
			}
		}

		public override string ToString()
		{
			if ((this.Parent != null) && ((this.Parent.Name == "script") || (this.Parent.Name == "style")))
			{
				return this.text;
			}

			return "<![CDATA[" + this.text + "]]>";
		}
	}

	internal sealed class HtmlText : HtmlCharacterData
	{
		public override string ToString()
		{
			return HtmlDocument.TranslateTextToHtml(this.Text, true);
		}
	}

	internal sealed class HtmlDocument
	{
		private HtmlNodeCollection nodes = null;
		
		// Tokenizer
		private string html;
		private ArrayList tokens = new ArrayList(1024);
		
		// Parser
		private int tokenIndex = 0;
		private HtmlNodeCollection currentScope = null;
		private IDictionary openElementStack = new Hashtable();
		private IDictionary openElementTable = new Hashtable();
		private int openElementCounter = 0;

		private static IDictionary entityToCharTable;
		private static IDictionary charToEntityTable;

		public HtmlNodeCollection Nodes
		{
			get
			{
				if (this.nodes == null)
				{
					this.nodes = new HtmlNodeCollection(null);
				}

				return this.nodes;
			}
		}

		public void LoadHtml(string value)
		{
			this.html = value;
			this.Nodes.Clear();

			InitializeEntityTable();

			this.Tokenize();
			this.Parse();

			this.tokens.Clear();
			this.currentScope = null;
			this.openElementStack.Clear();
			this.openElementTable.Clear();
			this.openElementCounter = 0;
			this.html = null;
		}

		private Token ReadToken()
		{
			return (Token)this.tokens[this.tokenIndex++];
		}

		private Token PeekToken()
		{
			return (Token)this.tokens[this.tokenIndex];
		}

		private void Parse()
		{
			this.currentScope = this.nodes;
			this.openElementCounter = 0;
			this.tokenIndex = 0;
			while (this.tokenIndex < this.tokens.Count)
			{
				Token token = this.ReadToken();
				if (token.Type == TokenType.StartElement)
				{
					string tagName = token.Value;
					if (tagName == "p")
					{
						this.ParseOptionalEndTag(tagName, new string[] { "p" });
						this.ParseElement(tagName);
					}
					else if (tagName == "li")
					{
						this.ParseOptionalEndTag(tagName, new string[] { "ol", "ul", "menu", "dir" });
						this.ParseElement(tagName);
					}
					else if (tagName == "option")
					{
						this.ParseOptionalEndTag(tagName, new string[] { "select", "optgroup" });
						this.ParseElement(tagName);
					}
					else if (tagName == "tr")
					{
						this.ParseOptionalEndTag("td", new string[] { "tr", "tbody", "thead", "tfoot", "table" });
						this.ParseOptionalEndTag("th", new string[] { "tr", "tbody", "thead", "tfoot", "table" });
						this.ParseOptionalEndTag(tagName, new string[] { "tbody", "thead", "tfoot", "table" });
						this.ParseElement(tagName);
					}
					else if ((tagName == "td") || (tagName == "th"))
					{
						this.ParseOptionalEndTag(tagName, new string[] { "tr", "tbody", "thead", "tfoot", "table" });
						this.ParseElement(tagName);
					}
					else if ((tagName == "tbody") || (tagName == "tfoot") || (tagName == "colgroup"))
					{
						this.ParseOptionalEndTag("td", new string[] { "table" });
						this.ParseOptionalEndTag("th", new string[] { "table" });
						this.ParseOptionalEndTag("tr", new string[] { "table" });
						this.ParseOptionalEndTag(tagName, new string[] { "table" });
						this.ParseElement(tagName);
					}
					else if ((tagName == "dt") || (tagName == "dd"))
					{
						this.ParseOptionalEndTag(tagName, new string[] { "dl" });
						this.ParseElement(tagName);
					}
					else
					{
						this.ParseElement(tagName);
					}
				}
				else if (token.Type == TokenType.EndElement)
				{
					HtmlElement openElement = (HtmlElement) this.FindOpenElement(token.Value);
					if (openElement != null)
					{
						openElement.Terminated = true;
						this.CloseElement(openElement, false);
						this.currentScope = (openElement.Parent != null) ? openElement.Parent.Nodes : this.nodes;
					}
				}
				else if (token.Type == TokenType.Comment)
				{
					HtmlComment node = new HtmlComment();
					node.Comment = token.Value;
					this.currentScope.Add(node);
				}
				else if (token.Type == TokenType.ProcessingInstruction)
				{
					HtmlProcessingInstruction node = new HtmlProcessingInstruction();
					node.Value = token.Value;
					this.currentScope.Add(node);
				}
				else if ((token.Type == TokenType.Text) || (token.Type == TokenType.CharacterData))
				{
					StringBuilder builder = new StringBuilder();
					builder.Append(token.Value);

					Token nextToken = this.PeekToken();
					while ((this.tokenIndex < this.tokens.Count) && (token.Type == nextToken.Type))
					{
						builder.Append(token.Value);
						this.ReadToken();
						nextToken = this.PeekToken();
					}

					if (token.Type == TokenType.CharacterData)
					{
						HtmlCharacterData node = new HtmlCharacterData();
						node.Text = builder.ToString();
						this.currentScope.Add(node);
					}
					else
					{
						HtmlText node = new HtmlText();
						node.Text = TranslateHtmlToText(builder.ToString());
						this.currentScope.Add(node);
					}
				}
				else if (token.Type == TokenType.End)
				{
					for (int i = 0; i < this.nodes.Count; i++)
					{
						HtmlElement childElement = this.nodes[i] as HtmlElement;
						if ((childElement != null) && (this.openElementTable.Contains(childElement)))
						{
							this.CloseElement(childElement, true);
						}
					}

					this.tokens.Clear();
					break;
				}
			}
		}

		private void ParseElement(string tagName)
		{
			HtmlElement element = new HtmlElement(tagName);

			for (Token token = this.PeekToken(); token.Type == TokenType.StartAttribute; token = this.PeekToken())
			{
				HtmlAttribute attribute = new HtmlAttribute(token.Value);
				element.Attributes.Add(attribute);

				this.ReadToken();
				token = this.PeekToken();
				if (token.Type == TokenType.EndAttribute)
				{
					attribute.Value = TranslateHtmlToText(token.Value);
					this.ReadToken();
				}
			}

			Token endToken = this.PeekToken();
			if (endToken.Type != TokenType.EndStartElement)
			{
				this.currentScope.Add(element);
			}
			else
			{
				if ((tagName == "area") || (tagName == "bgsound") || (tagName == "base") || (tagName == "br") || (tagName == "basefont") || (tagName == "col") || (tagName == "embed") || (tagName == "frame") || (tagName == "hr") || (tagName == "img") || (tagName == "isindex") || (tagName == "input") || (tagName == "keygen") || (tagName == "link") || (tagName == "meta") || (tagName == "nextid") || (tagName == "option") || (tagName == "param") || (tagName == "sound") || (tagName == "spacer") || (tagName == "wbr"))
				{
					this.currentScope.Add(element);
					element.Terminated = false;
				}
				else
				{
					if (!this.openElementStack.Contains(element.Name))
					{
						this.openElementStack.Add(element.Name, new Stack());
					}

					Stack stack = (Stack) this.openElementStack[element.Name];
					stack.Push(element);

					this.openElementTable.Add(element, this.openElementCounter++);

					this.currentScope.Add(element);
					this.currentScope = element.Nodes;
				}
			}

			this.ReadToken();
		}

		private void ParseOptionalEndTag(string tagName, string[] boundElements)
		{
			HtmlElement openElement = (HtmlElement) this.FindOpenElement(tagName);
			if (openElement != null)
			{
				bool fixChildren = true;

				for (int i = 0; i < boundElements.Length; i++)
				{
					string boundName = boundElements[i];
					HtmlElement boundElement = (HtmlElement) this.FindOpenElement(boundName);
					if ((boundElement != null) && (this.openElementTable.Contains(boundElement)))
					{
						int boundIndex = (int)this.openElementTable[boundElement];
						int openIndex = (int)this.openElementTable[openElement];

						if (boundIndex > openIndex)
						{
							fixChildren = false;
							break;
						}
					}
				}

				if (fixChildren)
				{
					this.CloseElement(openElement, fixChildren);
					this.currentScope = (openElement.Parent != null) ? openElement.Parent.Nodes : this.nodes;
				}
			}
		}

		private HtmlElement FindOpenElement(string tagName)
		{
			if (this.openElementStack.Contains(tagName))
			{
				Stack stack = (Stack)this.openElementStack[tagName];
				return (HtmlElement) stack.Peek();
			}

			return null;
		}

		private void CloseElement(HtmlElement element, bool fixChildren)
		{
			if (fixChildren)
			{
				for (int i = element.Nodes.Count - 1; i >= 0; i--)
				{
					HtmlElement childElement = element.Nodes[i] as HtmlElement;
					if ((childElement != null) && (this.openElementTable.Contains(childElement)))
					{
						this.CloseElement(childElement, true);
					}
				}

				HtmlElement parent = element.Parent;
				if (parent != null)
				{
					int index = parent.Nodes.IndexOf(element);
					for (int i = element.Nodes.Count - 1; i >= 0; i--)
					{
						parent.Nodes.Insert(index + 1, element.Nodes[i]);
					}
				}
			}

			this.openElementTable.Remove(element);

			Stack stack = (Stack)this.openElementStack[element.Name];
			stack.Pop();
			if (stack.Count == 0)
			{
				this.openElementStack.Remove(element.Name);
			}
		}

		private void Tokenize()
		{
			ArrayList tokens = new ArrayList(512);

			int index = 0;
			int nextIndex = 0;
			while (index < this.html.Length)
			{
				tokens.Clear();

				if (this.Match(index, "</"))
				{
					nextIndex = this.TokenizeEndTag(index, tokens);
				}
				else if (this.Match(index, "<!"))
				{
					nextIndex = this.TokenizeComment(index, tokens);
				}
				else if (this.Match(index, "<?"))
				{
					nextIndex = this.TokenizeProcessingInstruction(index, tokens);
				}
				else if (this.Match(index, "<"))
				{
					nextIndex = this.TokenizeStartTag(index, tokens);
				}
				else
				{
					nextIndex = this.TokenizeText(index, tokens);
				}

				if (nextIndex == -1)
				{
					tokens.Clear();
					nextIndex = this.TokenizeText(index, tokens);
				}

				index = nextIndex;
				this.tokens.AddRange(tokens);
			}

			this.tokens.Add(new Token(TokenType.End, null));
		}

		private int TokenizeStartTag(int index, ArrayList tokens)
		{
			index++;
			if (index < this.html.Length)
			{
				if (this.html[index].Equals('>'))
				{
					return index + 1;
				}

				if (!this.MatchChar(index, " \t\r\n\'\"=/<?"))
				{
					int startIndex = index++;
					index = this.SkipTo(index, " \t\r\n/>");
					if (index < this.html.Length)
					{
						string tagName = this.html.Substring(startIndex, index - startIndex).ToLower();

						index = this.SkipWhitespace(index);
						if (index < this.html.Length)
						{
							if (this.html[index] == '>')
							{
								// <Foo>
								tokens.Add(new Token(TokenType.StartElement, tagName));
								tokens.Add(new Token(TokenType.EndStartElement, null));
								index++;
							}
							else if (this.Match(index, "/>"))
							{
								// <Foo/>
								tokens.Add(new Token(TokenType.StartElement, tagName));
								tokens.Add(new Token(TokenType.EndElement, null));
								index = index + 2;
							}
							else
							{
								tokens.Add(new Token(TokenType.StartElement, tagName));
								int nextIndex = this.TokenizeAttributeList(index, tokens);
								if (nextIndex == index)
								{
									return -1;
								}
								index = nextIndex;
							}

							if ((tagName == "script") || (tagName == "style"))
							{
								Token token = (Token)tokens[tokens.Count - 1];
								if (token.Type == TokenType.EndStartElement)
								{
									if (index < this.html.Length)
									{
										int endIndex = this.html.IndexOf("</", index);
										while (endIndex != -1)
										{
											startIndex = endIndex + 2;

											int tagNameEnd = this.SkipTo(startIndex, " \t\r\n/>");
											if (tagNameEnd >= this.html.Length)
											{
												endIndex = -1;
												break;
											}

											if (tagNameEnd > startIndex)
											{
												if (tagName == this.html.Substring(startIndex, tagNameEnd - startIndex).ToLower())
												{
													break;
												}
											}

											endIndex = this.html.IndexOf("</", endIndex + 2);
										}

										if (endIndex == -1)
										{
											tokens.Add(new Token(TokenType.CharacterData, this.html.Substring(index)));
											index = this.html.Length;
											tokens.Add(new Token(TokenType.EndElement, tagName));
										}
										else
										{
											tokens.Add(new Token(TokenType.CharacterData, this.html.Substring(index, endIndex - index)));
											index = endIndex;
											int nextIndex = this.TokenizeEndTag(index, tokens);
											if (nextIndex == index)
											{
												return -1;
											}

											index = nextIndex;
										}
									}
								}
							}

							return index;
						}
					}
				}
			}

			return -1;
		}

		private int TokenizeEndTag(int index, ArrayList tokens)
		{
			if (this.Match(index, "</"))
			{
				index = index + 2;
				if (index < this.html.Length)
				{
					if (this.html[index].Equals('>'))
					{
						return index + 1;
					}

					if (!this.MatchChar(index, " \t\r\n\'\"=/<?"))
					{
						int startIndex = index++;
						index = this.SkipTo(index, " \t\r\n/>");
						if (index < this.html.Length)
						{
							string tagName = this.html.Substring(startIndex, index - startIndex).ToLower();
							index = this.SkipTo(index, ">");
							if (index < this.html.Length)
							{
								tokens.Add(new Token(TokenType.EndElement, tagName));
								return index + 1;
							}
						}
					}
				}
			}

			return -1;
		}

		private int TokenizeText(int index, ArrayList tokens)
		{
			int endIndex = (this.html[index].Equals('<')) ? ((index + 1 < this.html.Length) ? this.html.IndexOf("<", index + 1) : -1) : (this.html.IndexOf("<", index));

			string text = string.Empty;
			if (endIndex != -1)
			{
				text = this.html.Substring(index, endIndex - index);
				index = endIndex;
			}
			else
			{
				text = this.html.Substring(index);
				index = this.html.Length;
			}

			if (text.Length > 0)
			{
				tokens.Add(new Token(TokenType.Text, text));
			}

			return index;
		}

		private int TokenizeComment(int index, ArrayList tokens)
		{
			index = index + 2;
			if (index < this.html.Length)
			{
				int startIndex = index;

				if (this.html.Substring(index, 7) == "[CDATA[")
				{
					index = this.html.IndexOf("]]>");
					if (index == -1)
					{
						tokens.Add(new Token(TokenType.CharacterData, this.html.Substring(startIndex)));
						index = this.html.Length;
					}
					else
					{
						tokens.Add(new Token(TokenType.CharacterData, this.html.Substring(startIndex, index - startIndex + 2)));
						index = index + 3;
					}
				}
				else if (this.html.Substring(index, 2) == "--")
				{
					index = this.html.IndexOf("-->", startIndex);
					if (index == -1)
					{
						tokens.Add(new Token(TokenType.Comment, this.html.Substring(startIndex)));
						index = this.html.Length;
					}
					else
					{
						tokens.Add(new Token(TokenType.Comment, this.html.Substring(startIndex, index - startIndex + 2)));
						index = index + 3;
					}
				}
				else
				{
					index = this.html.IndexOf(">", startIndex);
					if (index == -1)
					{
						tokens.Add(new Token(TokenType.Comment, this.html.Substring(startIndex)));
						index = this.html.Length;
					}
					else
					{
						tokens.Add(new Token(TokenType.Comment, this.html.Substring(startIndex, index - startIndex)));
						index++;
					}
				}
			}

			return index;
		}

		private int TokenizeProcessingInstruction(int index, ArrayList tokens)
		{
			index = index + 2;
			if (index < this.html.Length)
			{
				int startIndex = index;
				index = this.html.IndexOf("?>", startIndex);
				if (index == -1)
				{
					index = this.html.IndexOf(">", startIndex);
				}

				if (index == -1)
				{
					string text = this.html.Substring(startIndex);
					tokens.Add(new Token(TokenType.ProcessingInstruction, text));
					index = this.html.Length;
				}
				else
				{
					string text = this.html.Substring(startIndex, index - startIndex);
					tokens.Add(new Token(TokenType.ProcessingInstruction, text));
					index = index + 2;
				}
			}

			return index;
		}

		private int TokenizeAttributeList(int index, ArrayList tokens)
		{
			while (true)
			{
				index = this.SkipWhitespace(index);
				if (index < this.html.Length)
				{
					if (this.html[index].Equals('>'))
					{
						tokens.Add(new Token(TokenType.EndStartElement, null));
						return index + 1;
					}

					if (this.Match(index, "/>"))
					{
						tokens.Add(new Token(TokenType.EndElement, null));
						return index + 2;
					}

					if (!IsAttributeNameChar(this.html[index]))
					{
						index++;
						continue;
					}

					int startIndex = index++;

					while ((index < this.html.Length) && (IsAttributeNameChar(this.html[index])))
					{
						index++;
					}

					if (index < this.html.Length)
					{
						tokens.Add(new Token(TokenType.StartAttribute, this.html.Substring(startIndex, index - startIndex)));

						index = this.SkipWhitespace(index);
						if (index < this.html.Length)
						{
							if (this.html[index].Equals('='))
							{
								index++;
								index = this.SkipWhitespace(index);
								if (index < this.html.Length)
								{
									if (this.html[index].Equals('\"') || this.html[index].Equals('\''))
									{
										char quote = this.html[index++];
										if (index < this.html.Length)
										{
											startIndex = index;
											index = this.html.IndexOf(quote, index);
											if (index != -1)
											{
												tokens.Add(new Token(TokenType.EndAttribute, this.html.Substring(startIndex, index - startIndex)));
												index++;
												continue;
											}
										}
									}
									else
									{
										startIndex = index;
										while ((index < this.html.Length) && (IsAttributeValueChar(this.html[index])))
										{
											index++;
										}

										if (index < this.html.Length)
										{
											if (index > startIndex)
											{
												tokens.Add(new Token(TokenType.EndAttribute, this.html.Substring(startIndex, index - startIndex)));
											}
											continue;
										}
									}
								}
							}
						}
					}
				}

				return -1;
			}
		}

		private int SkipWhitespace(int index)
		{
			while ((index < this.html.Length) && (((this.html[index] == ' ') || (this.html[index] == '\t') || (this.html[index] == '\r') || (this.html[index] == '\n'))))
			{
				index++;
			}

			return index;
		}

		private int SkipTo(int index, string chars)
		{
			while ((index < this.html.Length) && (chars.IndexOf(this.html[index]) == -1))
			{
				index++;
			}

			return index;
		}

		private bool MatchChar(int index, string value)
		{
			return (value.IndexOf(this.html[index]) != -1);
		}

		private bool Match(int index, string value)
		{
			return (((index + value.Length) <= this.html.Length) && (this.html.IndexOf(value, index, value.Length) != -1));
		}

		private static bool IsAttributeNameChar(char value)
		{
			return ((('a' <= value) && (value <= 'z')) || (('A' <= value) && (value <= 'Z')) || (('0' <= value) && (value <= '9')));
		}

		private static bool IsAttributeValueChar(char value)
		{
			return ((IsAttributeNameChar(value)) || (value == '-') || (value == '.') || (value == ':') || (value == '_') || (value == '/') || (value == '#'));
		}

		private static string TranslateHtmlToText(string value)
		{
			StringBuilder builder = new StringBuilder();

			int index = 0;
			while (index < value.Length)
			{
				int nextIndex = value.IndexOf('&', index);
				if (nextIndex == -1)
				{
					return builder.ToString() + value.Substring(index);
				}

				builder.Append(value.Substring(index, nextIndex - index));
				index = nextIndex;
				nextIndex = value.IndexOf(';', index);
				if (nextIndex != -1)
				{
					string text = value.Substring(index, nextIndex - index + 1);
					if ((text[1] == '#') && (text.Length > 2))
					{
						text = text.Substring(2, text.Length - 3);
						char ch = (char) ((text[0] != 'x') ? int.Parse(text) : int.Parse(text.Substring(1), NumberStyles.HexNumber));
						builder.Append(ch);
					}
					else
					{
						char ch = TranslateEntityToChar(text);
						if (ch != '\0')
						{
							builder.Append(ch);
						}
					}

					index = nextIndex;
				}
				else
				{
					builder.Append(value.Substring(index));
					break;
				}

				index++;
			}

			return builder.ToString();
		}

		internal static string TranslateTextToHtml(string value, bool lineFeed)
		{
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < value.Length; i++)
			{
				char ch = value[i];
				if (((ch == ' ') || (ch == '\t')) || ((lineFeed) && ((ch == '\r') || (ch == '\n'))))
				{
					builder.Append(ch);
				}
				else
				{
					builder.Append(TranslateCharToEntity(ch));
				}
			}

			return builder.ToString();
		}

		private static void InitializeEntityTable()
		{
			if ((charToEntityTable == null) && (entityToCharTable == null))
			{
				charToEntityTable = new Hashtable();
				entityToCharTable = new Hashtable();

				int[] chars = new int[] { 
					'<', '>', '&', '\"', 193, 225, 194, 226, 180, 198, 230, 192, 224, 8501, 913, 945, 8743, 8736, 197, 229, 8776, 
					195, 227, 196, 228, 8222, 914, 946, 166, 8226, 8745, 199, 231, 184, 162, 935, 967, 710, 9827, 8773, 169, 8629,
					8746, 164, 8224, 8225, 8595, 8659, 176, 916, 948, 9830, 247, 201, 233, 202, 234, 200, 232, 8709, 8195, 8194, 
					917, 949, 8801, 919, 951, 208, 240, 203, 235, 128, 8707, 402, 8704, 189, 188, 190, 8260, 915, 947, 8805, 8596, 
					8660, 9829, 8230, 205, 237, 206, 238, 161, 204, 236, 8465, 8734, 8747, 921, 953, 191, 8712, 207, 239, 922, 954, 
					923, 955, 9001, 171, 8592, 8656, 8968, 8220, 8804, 8970, 8727, 9674, 8206, 8249, 8216, 175, 8212, 181, 183, 8722, 
					924, 956, 8711, 160, 8211, 8800, 8715, 172, 8713, 8836, 209, 241, 925, 957, 211, 243, 212, 244, 338, 339, 210, 242, 
					8254, 937, 969, 927, 959, 8853, 8744, 170, 186, 216, 248, 213, 245, 8855, 214, 246, 182, 8706, 8240, 8869, 934, 966, 
					928, 960, 982, 177, 163, 8242, 8243, 8719, 8733, 936, 968, 8730, 9002, 187, 8594, 8658, 8969, 8221, 8476, 174, 8971, 
					929, 961, 8207, 8250, 8217, 8218, 352, 353, 8901, 167, 173, 931, 963, 962, 8764, 9824, 8834, 8838, 8721, 8835, 185, 
					178, 179, 8839, 223, 932, 964, 8756, 920, 952, 977, 8201, 222, 254, 732, 215, 8482, 218, 250, 8593, 8657, 219, 251, 
					217, 249, 168, 978, 933, 965, 220, 252, 8472, 926, 958, 221, 253, 165, 376, 255, 918, 950, 8205, 8204 };

				string[] entities = new string[] {
					"&lt;", "&gt;", "&amp;", "&quot;", "&Aacute;", "&aacute;", "&Acirc;", "&acirc;", "&acute;", "&AElig;", "&aelig;", "&Agrave;", "&agrave;", "&alefsym;", "&Alpha;", "&alpha;", "&and;", 
					"&ang;", "&Aring;", "&aring;", "&asymp;", "&Atilde;", "&atilde;", "&Auml;", "&auml;", "&bdquo;", "&Beta;", "&beta;", "&brvbar;", "&bull;", "&cap;", "&Ccedil;", "&ccedil;", "&cedil;", 
					"&cent;", "&Chi;", "&chi;", "&circ;", "&clubs;", "&cong;", "&copy;", "&crarr;", "&cup;", "&curren;", "&dagger;", "&Dagger;", "&darr;", "&dArr;", "&deg;", "&Delta;", "&delta;", "&diams;", 
					"&divide;", "&Eacute;", "&eacute;", "&Ecirc;", "&ecirc;", "&Egrave;", "&egrave;", "&empty;", "&emsp;", "&ensp;", "&Epsilon;", "&epsilon;", "&equiv;", "&Eta;", "&eta;", "&ETH;", "&eth;", 
					"&Euml;", "&euml;", "&euro;", "&exist;", "&fnof;", "&forall;", "&frac12;", "&frac14;", "&frac34;", "&fras1;", "&Gamma;", "&gamma;", "&ge;", "&harr;", "&hArr;", "&hearts;", "&hellip;", 
					"&Iacute;", "&iacute;", "&Icirc;", "&icirc;", "&iexcl;", "&Igrave;", "&igrave;", "&image;", "&infin;", "&int;", "&Iota;", "&iota;", "&iquest;", "&isin;", "&Iuml;", "&iuml;", "&Kappa;", 
					"&kappa;", "&Lambda;", "&lambda;", "&lang;", "&laquo;", "&larr;", "&lArr;", "&lceil;", "&ldquo;", "&le;", "&lfloor;", "&lowast;", "&loz;", "&lrm;", "&lsaquo;", "&lsquo;", "&macr;", 
					"&mdash;", "&micro;", "&middot;", "&minus;", "&Mu;", "&mu;", "&nabla;", "&nbsp;", "&ndash;", "&ne;", "&ni;", "&not;", "&notin;", "&nsub;", "&Ntilde;", "&ntilde;", "&Nu;", "&nu;", 
					"&Oacute;", "&oacute;", "&Ocirc;", "&ocirc;", "&OElig;", "&oelig;", "&Ograve;", "&ograve;", "&oline;", "&Omega;", "&omega;", "&Omicron;", "&omicron;", "&oplus;", "&or;",
					"&ordf;", "&ordm;", "&Oslash;", "&oslash;", "&Otilde;", "&otilde;", "&otimes;", "&Ouml;", "&ouml;", "&para;", "&part;", "&permil;", "&perp;", "&Phi;", "&phi;", "&Pi;", "&pi;", 
					"&piv;", "&plusmn;", "&pound;", "&prime;", "&Prime;", "&prod;", "&prop;", "&Psi;", "&psi;", "&radic;", "&rang;", "&raquo;", "&rarr;", "&rArr;", "&rceil;", "&rdquo;", "&real;", 
					"&reg;", "&rfloor;", "&Rho;", "&rho;", "&rlm;", "&rsaquo;", "&rsquo;", "&sbquo;", "&Scaron;", "&scaron;", "&sdot;", "&sect;", "&shy;", "&Sigma;", "&sigma;", "&sigmaf;", "&sim;", 
					"&spades;", "&sub;", "&sube;", "&sum;", "&sup;", "&sup1;", "&sup2;", "&sup3;", "&supe;", "&szlig;", "&Tau;", "&tau;", "&there4;", "&Theta;", "&theta;", "&thetasym;", "&thinsp;", 
					"&THORN;", "&thorn;", "&tilde;", "&times;", "&trade;", "&Uacute;", "&uacute;", "&uarr;", "&uArr;", "&Ucirc;", "&ucirc;", "&Ugrave;", "&ugrave;", "&uml;", "&upsih;", "&Upsilon;", 
					"&upsilon;", "&Uuml;", "&uuml;", "&weierp;", "&Xi;", "&xi;", "&Yacute;", "&yacute;", "&yen;", "&Yuml;", "&yuml;", "&Zeta;", "&zeta;", "&zwj;", "&zwnj;"};

				for (int i = 0; i < chars.Length; i++)
				{
					charToEntityTable.Add(chars[i], entities[i]);
					entityToCharTable.Add(entities[i], chars[i]);
				}
			}
		}

		private static char TranslateEntityToChar(string value)
		{
			return (entityToCharTable.Contains(value)) ? (char)(int)entityToCharTable[value] : '\0';
		}

		internal static string TranslateCharToEntity(char value)
		{
			return (charToEntityTable.Contains((int)value)) ? (string)charToEntityTable[(int)value] : value.ToString();
		}

		private enum TokenType
		{
			StartElement, EndStartElement, EndElement, StartAttribute, EndAttribute, Comment, ProcessingInstruction, CharacterData, Text, End
		}

		private class Token
		{
			private TokenType type;
			private string value;

			public Token(TokenType type, string value)
			{
				this.type = type;
				this.value = value;
			}

			public TokenType Type
			{
				get 
				{ 
					return this.type; 
				}
			}

			public string Value
			{
				get 
				{ 
					return this.value; 
				}
			}

			public override string ToString()
			{
				return this.Type.ToString() + ((this.value != null) ? (",\"" + this.value.ToString() + "\"") : string.Empty);
			}
		}
	}
}
