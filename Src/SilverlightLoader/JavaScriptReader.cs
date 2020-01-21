namespace Reflector.SilverlightLoader
{
	using System;
	using System.Collections;
	using System.Text.RegularExpressions;

	internal class JavaScriptReader
	{
		private string content;

		public JavaScriptReader(string content)
		{
			this.content = content;
		}

		public ICollection XamlLinks
		{
			get
			{
				ArrayList list = new ArrayList();
				list.AddRange(this.ParseCreateObject());
				list.AddRange(this.ParseCreateObjectEx());
				return list;
			}
		}

		private ICollection ParseCreateObject()
		{
			ArrayList list = new ArrayList();
			Regex expression = new Regex(@"Silverlight.createObject\(");
			foreach (Match match in expression.Matches(this.content))
			{
				if (match.Success)
				{
					int index = match.Index + match.Length;
					int startIndex = index;

					while ((index < this.content.Length) && (this.content[index] != ',') && (this.content[index] != ')'))
					{
						index++;
					}

					string xaml = this.content.Substring(startIndex, index - startIndex).Trim();
					if ((xaml.Length > 2) && ((xaml[0] == '\"') || (xaml[0] == '\'')) && (xaml[0] == xaml[xaml.Length - 1]))
					{
						xaml = xaml.Substring(1, xaml.Length - 2);
						list.Add(xaml);
					}
				}
			}

			return list;
		}

		private ICollection ParseCreateObjectEx()
		{
			ArrayList list = new ArrayList();
			Regex expression = new Regex(@"Silverlight.createObjectEx\(");
			foreach (Match match in expression.Matches(this.content))
			{
				if (match.Success)
				{
					int startIndex = match.Index + match.Length;

					while ((startIndex < this.content.Length) && (this.content[startIndex] != '{'))
					{
						startIndex++;
					}

					startIndex++;
					if (startIndex < this.content.Length)
					{
						int endIndex = startIndex;
						while ((endIndex < this.content.Length) && (this.content[endIndex] != '{') && (this.content[endIndex] != '}'))
						{
							endIndex++;
						}

						if (endIndex < this.content.Length)
						{
							string innerContent = this.content.Substring(startIndex, endIndex - startIndex);
							Regex innerExpression = new Regex(@"source:\s*" + "[\"|']");
							Match innerMatch = innerExpression.Match(innerContent);
							if (innerMatch.Success)
							{
								int index = innerMatch.Index + innerMatch.Length;
								char ch = innerContent[index];

								string text = string.Empty;
								while (ch != '\"' && ch != '\'')
								{
									text = text + ch;
									index++;
									ch = innerContent[index];
								}

								list.Add(text);
							}
						}
					}
				}
			}

			return list;
		}
	}
}