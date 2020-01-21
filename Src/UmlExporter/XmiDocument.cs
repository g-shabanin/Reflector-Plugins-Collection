namespace Reflector.UmlExporter
{
	using System;
	using System.Xml.Serialization;

	[XmlRoot("XMI")]
	public class XmiDocument
	{
		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces xmlns;

		[XmlElement("XMI.header")]
		public XmiHeader Header = new XmiHeader();

		[XmlElement("XMI.content")]
		public XmiContent Content = new XmiContent();

		[XmlAttribute("timestamp")]
		public DateTime TimeStamp = DateTime.Now;

		[XmlAttribute("xmi.version")]
		public string Version = "1.2";

		public XmiDocument()
		{
			xmlns = new XmlSerializerNamespaces();
			xmlns.Add("UML", "org.omg.xmi.namespace.UML");
			xmlns.Add("UML2", "org.omg.xmi.namespace.UML2");
		}
	}

	public class XmiHeader
	{
		[XmlElement("XMI.documentation")]
		public XmiDocumentation documentation = new XmiDocumentation();

		public class XmiDocumentation
		{
			[XmlElement("XMI.exporter")]
			public string Exporter = "Reflector XMI Exporter";

			[XmlElement("XMI.exporterVersion")]
			public string ExporterVersion = "1.0";

			[XmlElement("XMI.metaModelVersion")]
			public string MetaModelVersion = "1.4.4";
		}
	}

	public class XmiContent
	{
		[XmlElement("Model", Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "org.omg.xmi.namespace.UML")]
		public UmlModel Model;
	}
}
