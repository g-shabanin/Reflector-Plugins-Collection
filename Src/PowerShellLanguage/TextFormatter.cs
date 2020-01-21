using Reflector.CodeModel;
using System;
using System.Globalization;
using System.IO;

namespace Reflector.PowerShellLanguage
{
	internal class TextFormatter : IFormatter
	{
		private StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
		private bool newLine;
		private int indent = 0;

		public override string ToString()
		{
			return this.writer.ToString();
		}

		public void Write(string text)
		{
			this.ApplyIndent();
			this.writer.Write(text);
		}

		public void WriteDeclaration(string text)
		{
			this.WriteBold(text);
		}

		public void WriteDeclaration(string text, object target)
		{
			this.WriteBold(text);
		}

		public void WriteComment(string text)
		{
			this.WriteColor(text, (int)0x808080);
		}

		public void WriteLiteral(string text)
		{
			this.WriteColor(text, (int)0x800000);
		}

		public void WriteKeyword(string text)
		{
			this.WriteColor(text, (int)0x000080);
		}

		public void WriteIndent()
		{
			this.indent++;
		}

		public void WriteLine()
		{
			this.writer.WriteLine();
			this.newLine = true;
		}

		public void WriteOutdent()
		{
			this.indent--;
		}

		public void WriteReference(string text, string toolTip, Object reference)
		{
			this.ApplyIndent();
			this.writer.Write(text);
		}

		public void WriteProperty(string propertyName, string propertyValue)
		{
		}

		private void WriteBold(string text)
		{
			this.ApplyIndent();
			this.writer.Write(text);
		}

		private void WriteColor(string text, int color)
		{
			this.ApplyIndent();
			this.writer.Write(text);
		}

		private void ApplyIndent()
		{
			if (this.newLine)
			{
				for (int i = 0; i < this.indent; i++)
				{
					this.writer.Write("    ");
				}

				this.newLine = false;
			}
		}
	}
}

