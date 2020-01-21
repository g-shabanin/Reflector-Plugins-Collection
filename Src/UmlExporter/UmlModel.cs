namespace Reflector.UmlExporter
{
	using System;
	using System.Collections;
	using System.Xml.Serialization;

	public class UmlModel : UmlOwner
	{
		//[XmlElement("Namespace.ownedElement", Namespace = "org.omg.xmi.namespace.UML")]
		//public UmlOwner OwnedElements = new UmlOwner();
	}

	[XmlInclude(typeof(UmlModel)), XmlInclude(typeof(UmlPackage))]
	public abstract class UmlModelElement
	{
		[XmlAttribute("xmi.id")]
		public Guid Id = Guid.NewGuid();

		[XmlAttribute("isSpecification")]
		public bool IsSpecification = false;

		[XmlAttribute("name")]
		public string Name;
	}

	public abstract class UmlStandardModelElement : UmlModelElement
	{
		[XmlAttribute("isRoot")]
		public bool IsRoot = false;

		[XmlAttribute("isLeaf")]
		public bool IsLeaf = false;

		[XmlAttribute("isAbstract")]
		public bool IsAbstract = false;

		[XmlAttribute("isActive")]
		public bool IsActive = false;
	}

	public class UmlAbstraction : UmlModelElement
	{
		[XmlElement(ElementName = "ModelElement.stereotype", Namespace = "org.omg.xmi.namespace.UML")]
		public UmlTypedElementType ModelElementStereotype;

		[XmlElement(ElementName = "Dependency.client", Namespace = "org.omg.xmi.namespace.UML")]
		public UmlTypedElementType DependencyClient;

		[XmlElement(ElementName = "Dependency.supplier", Namespace = "org.omg.xmi.namespace.UML")]
		public UmlTypedElementType DependencySupplier;
	}

	public class UmlAttribute : UmlClassifierFeature, IUmlTyped
	{
		#region IUmlTyped Members

		[XmlElement("TypedElement.type", Namespace = "org.omg.xmi.namespace.UML2")]
		public UmlTypedElementType ElementType = null;

		public UmlTypeReference TypedElementType
		{
			set { ElementType = new UmlTypedElementType(); ElementType.Type = value; }
		}

		#endregion
	}

	public class UmlClassifierFeature : UmlVisibleModelElement
	{
		[XmlAttribute("ownerScope")]
		public string OwnerScope = null;

		public const string STATIC = "classifier";
	}

	public class UmlClassifier : UmlOwner
	{
		[XmlArray("Classifier.feature", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlOperation), ElementName = "Operation", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlAttribute), ElementName = "Attribute", Namespace = "org.omg.xmi.namespace.UML")]
		public ArrayList ClassifierFeature = new ArrayList();
		// public List<UmlClassifierFeature> ClassifierFeature = new List<UmlClassifierFeature>();

		[XmlIgnore]
		public bool PartialLoad = true;
	}

	public class UmlInterface : UmlClassifier
	{
	}

	public class UmlDataType : UmlClassifier
	{
	}

	/*
	<UML:Generalization xmi.id = 'Im1698338fm10d9acf0fccmm64ca' isSpecification = 'false'>
		<UML:Generalization.child>
			<UML:Class xmi.idref = 'Im1698338fm10d9acf0fccmm64f8'/>
		</UML:Generalization.child>
		<UML:Generalization.parent>
			<UML:Class xmi.idref = 'Im1698338fm10d9acf0fccmm64f9'/>
		</UML:Generalization.parent>
	</UML:Generalization>
 	*/
	public class UmlGeneralization : UmlModelElement
	{
		[XmlElement(ElementName = "Generalization.child", Namespace = "org.omg.xmi.namespace.UML")]
		public UmlTypedElementType GeneralizationChild;

		[XmlElement(ElementName = "Generalization.parent", Namespace = "org.omg.xmi.namespace.UML")]
		public UmlTypedElementType GeneralizationParent;
	}

	public class UmlOperation : UmlClassifierFeature
	{
		[XmlAttribute("concurrency")]
		public string Concurrency = SEQUENTIAL;

		public const string SEQUENTIAL = "sequential";
		public const string CONCURRENT = "concurrent";
		public const string SYNCHRONIZED = "synchronized";

		[XmlArray("BehavioralFeature.parameter", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlParameter), ElementName = "Parameter", Namespace = "org.omg.xmi.namespace.UML")]
		public ArrayList Parameters = new ArrayList();
		// public List<UmlParameter> Parameters = new List<UmlParameter>();

	}

	public class UmlOwner : UmlVisibleModelElement
	{
		[XmlArray("Namespace.ownedElement", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlPackage), ElementName = "Package", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlDataType), ElementName = "DataType", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlInterface), ElementName = "Interface", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlClassifier), ElementName = "Class", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlStereotype), ElementName = "Stereotype", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlGeneralization), ElementName = "Generalization", Namespace = "org.omg.xmi.namespace.UML"),
		 XmlArrayItem(typeof(UmlAbstraction), ElementName = "Abstraction", Namespace = "org.omg.xmi.namespace.UML")]
		public ArrayList OwnedElements = new ArrayList();
		// public List<UmlModelElement> OwnedElements = new List<UmlModelElement>();

		private IDictionary _Elements = new Hashtable();
		// private Dictionary<string, UmlModelElement> _Elements = new Dictionary<string, UmlModelElement>();

		public void Add(UmlModelElement element)
		{
			if (element.Name != null)
			{
				_Elements.Add(element.Name, element);
			}
			else
			{
				_Elements.Add(element.Id.ToString(), element);
			}

			OwnedElements.Add(element);
		}

		public UmlModelElement Fetch(string name)
		{
			if (_Elements.Contains(name))
			{
				return (UmlModelElement)_Elements[name];
			}
			return null;
		}
	}

	[Serializable]
	public class UmlParameter : UmlModelElement, IUmlTyped
	{
		[XmlAttribute("kind")]
		public string Kind = IN;

		public const string IN = "in";
		public const string OUT = "out";
		public const string INOUT = "inout";
		public const string RETURN = "return";

		[XmlElement("TypedElement.type", Namespace = "org.omg.xmi.namespace.UML2")]
		public UmlTypedElementType ElementType = null;

		public UmlTypeReference TypedElementType
		{
			set { ElementType = new UmlTypedElementType(); ElementType.Type = value; }
		}

	}

	public class UmlStereotype : UmlModelElement
	{
		[XmlIgnore]
		public static readonly UmlStereotype REALIZE;

		static UmlStereotype()
		{
			REALIZE = new UmlStereotype();
			REALIZE.Name = "realize";
			REALIZE.StereoTypeBaseClass = "Abstraction";
		}

		[XmlElement(ElementName = "Stereotype.baseClass", Namespace = "org.omg.xmi.namespace.UML")]
		public string StereoTypeBaseClass;
	}

	public class UmlPackage : UmlOwner
	{
		//[XmlElement("Namespace.ownedElement", Namespace = "org.omg.xmi.namespace.UML")]
		//public UmlOwner OwnedElements = new UmlOwner();
	}

	// [Serializable, XmlInclude(typeof(UmlDataTypeTypeReference)), XmlInclude(typeof(UmlClassTypeReference))]
	public class UmlTypeReference
	{
		[XmlAttribute("xmi.idref")]
		public Guid Id;

		public static UmlTypeReference Create(UmlModelElement element)
		{
			UmlTypeReference result = null;

			if (element is UmlDataType)
			{
				result = new UmlDataTypeTypeReference();
			}
			else if (element is UmlClassifier)
			{
				result = new UmlClassTypeReference();
			}
			else if (element is UmlStereotype)
			{
				result = new UmlStereotypeReference();
			}

			if (element != null)
			{
				result.Id = element.Id;
			}

			return result;
		}
	}

	[Serializable]
	public class UmlClassTypeReference : UmlTypeReference
	{
	}

	[Serializable]
	public class UmlStereotypeReference : UmlTypeReference
	{
	}

	[Serializable]
	public class UmlDataTypeTypeReference : UmlTypeReference
	{
	}

	[Serializable,
	 XmlInclude(typeof(UmlDataTypeTypeReference)),
	 XmlInclude(typeof(UmlClassTypeReference)),
	 XmlInclude(typeof(UmlStereotypeReference))]
	public class UmlTypedElementType
	{
		[XmlElement(typeof(UmlDataTypeTypeReference), ElementName = "DataType"), //, Namespace = "org.omg.xmi.namespace.UML"),
		 XmlElement(typeof(UmlClassTypeReference), ElementName = "Class"),
		 XmlElement(typeof(UmlStereotypeReference), ElementName = "Stereotype")] //, Namespace = "org.omg.xmi.namespace.UML")]
		public UmlTypeReference Type = null;

		public static UmlTypedElementType Create(UmlModelElement refFor)
		{
			UmlTypedElementType result = new UmlTypedElementType();
			result.Type = UmlTypeReference.Create(refFor);
			return result;
		}

		//public List<UmlTypeReference> TypeData = null;

		//internal UmlTypeReference Type
		//{
		//    set { TypeData = new List<UmlTypeReference>(); TypeData.Add(value); }
		//}
	}

	public class UmlVisibleModelElement : UmlStandardModelElement
	{
		[XmlAttribute("visibility")]
		public string Visibility = PUBLIC;

		public const string PUBLIC = "public";
		public const string PRIVATE = "private";
		public const string PROTECTED = "protected";
		public const string PACKAGE = "package";
	}

	public interface IUmlTyped
	{
		UmlTypeReference TypedElementType { set; }
	}
}
