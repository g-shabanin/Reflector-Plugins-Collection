namespace Reflector.SilverlightLoader
{
	using System;
	using System.Collections;
	using System.Xml;

	internal class XamlReader
	{
		private XmlDocument document;

		public XamlReader(string content)
		{
			this.document = new XmlDocument();
			this.document.LoadXml(content);
		}

		public ICollection AssemblyLinks
		{
			get
			{
				ArrayList list = new ArrayList();
				this.ParseElement(this.document.DocumentElement, list);
				return list;
			}
		}

		public ICollection SourceLinks
		{
			get
			{
				ArrayList list = new ArrayList();
				this.ParseElementForSource(this.document.DocumentElement, list);
				return list;
			}
		}

		private void ParseElement(XmlElement element, IList list)
		{
			foreach (XmlAttribute attribute in element.Attributes)
			{
				if ((attribute.NamespaceURI == "http://schemas.microsoft.com/winfx/2006/xaml") && (attribute.LocalName == "Class"))
				{
					this.ParseAttributeValue(attribute.Value, list);
				}

				if ((attribute.Prefix == "xmlns") && (attribute.Value.Trim().StartsWith("clr-namespace:")))
				{
					this.ParseAttributeValue(attribute.Value, list);
				}
			}

			foreach (XmlNode child in element.ChildNodes)
			{
				XmlElement childElement = child as XmlElement;
				if (childElement != null)
				{
					this.ParseElement(childElement, list);
				}
			}
		}

		private void ParseElementForSource(XmlElement element, IList list)
		{
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(element.OwnerDocument.NameTable);
			nsmgr.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
			XmlNodeList codeElements = element.SelectNodes("x:Code", nsmgr);
			if (codeElements.Count != 0)
			{
				foreach (XmlElement codeElement in codeElements)
				{
					XmlAttribute sourceAttribute = codeElement.Attributes["Source"];
					if (sourceAttribute != null)
					{
						list.Add(sourceAttribute.Value);
					}
				}
			}
		}

		private void ParseAttributeValue(string attributeValue, IList list)
		{
			string[] parts = attributeValue.Split(new char[] { ';' });
			foreach (string part in parts)
			{
				string property = part.Trim();
				if (property.StartsWith("assembly="))
				{
					string assemblyName = property.Substring(9).Trim();
					list.Add(assemblyName);
				}
			}
		}
	}
}
