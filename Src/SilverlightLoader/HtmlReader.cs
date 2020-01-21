namespace Reflector.SilverlightLoader
{
	using System;
	using System.Collections;

	internal class HtmlReader
	{
		private HtmlDocument document;

		public HtmlReader(string content)
		{
			this.document = new HtmlDocument();
			this.document.LoadHtml(content);
		}

		public ICollection HtmlLinks
		{
			get
			{
				ArrayList list = new ArrayList();

				foreach (HtmlElement link in this.GetElementsByTagName(this.document.Nodes, "a"))
				{
					HtmlAttribute attribute = link.Attributes["href"];
					if (attribute != null)
					{
						list.Add(attribute.Value);
					}
				}

				return list;
			}
		}

		public ICollection JavaScriptLinks
		{
			get
			{
				ArrayList list = new ArrayList();

				foreach (HtmlElement script in this.GetElementsByTagName(this.document.Nodes, "script"))
				{
					HtmlAttribute attribute = script.Attributes["src"];
					if (attribute != null)
					{
						list.Add(attribute.Value);
					}
				}

				return list;
			}
		}

		public ICollection XamlLinks
		{
			get
			{
				ArrayList list = new ArrayList();

				foreach (HtmlElement element in GetElementsByTagName(this.document.Nodes, "object"))
				{
					HtmlAttribute type = element.Attributes["type"];
					if (type != null) 
					{
						switch (type.Value)
						{
							case "application/x-silverlight":
							case "application/ag-plugin":
								foreach (HtmlElement param in this.GetElementsByTagName(element.Nodes, "param"))
								{
									HtmlAttribute name = param.Attributes["name"];
									HtmlAttribute value = param.Attributes["value"];
									if ((name != null) && (value != null) && (name.Value == "source") && (value.Value != null) && (value.Value.Length > 0))
									{
										list.Add(value.Value);
									}
								}
								break;
						}
					}
				}

				foreach (HtmlElement script in this.GetElementsByTagName(this.document.Nodes, "script"))
				{
					HtmlAttribute attribute = script.Attributes["src"];
					if ((attribute == null) && (script.Nodes.Count == 1) && (script.Nodes[0] is HtmlCharacterData))
					{
						HtmlCharacterData text = (HtmlCharacterData)script.Nodes[0];
						JavaScriptReader reader = new JavaScriptReader(text.Text);
						list.AddRange(reader.XamlLinks);
					}
				}

				return list;
			}
		}

		private ICollection GetElementsByTagName(HtmlNodeCollection value, string name)
		{
			ArrayList list = new ArrayList();

			for (int i = 0; i < value.Count; i++)
			{
				HtmlElement element = value[i] as HtmlElement;
				if (element != null)
				{
					if (element.Name == name)
					{
						list.Add(element);
					}

					list.AddRange(this.GetElementsByTagName(element.Nodes, name));
				}
			}

			return list;
		}
	}
}
