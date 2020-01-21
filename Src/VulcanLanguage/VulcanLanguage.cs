// Vulcan Language view for Lutz Roeder's .NET Reflector
// Fabrice Foray - fabrice@fabtoys.net

// Based on DelphiLanguage plugin
// Copyright (C) 2004 Peter Sawatzki. All rights reserved.
// Peter@Sawatzki.de
// for latest version see http://www.sawatzki.de

// V0.1.1.0 - First release
// V0.1.2.0 - Correction after Reflector change - Publication in CodePlex
// V0.1.3.0 - Support for Delegate creation & invoke
//          - Support for static fields ( Const and Readonly are commented )
//          - Change handling of Super/Base calls
//          - Tooltips are now presented in "Vulcan way" for properties
//          - Change Binary Operator (_and, _or, _xor) to (&, |, ~)
// V0.1.4.0 - Added Reflector.VulcanLangage.cfg external file
//          - Support of zero/one Based array via cfg file
//          - Fully Qualified Type generation to avoid reference missing
//          - Super() call in Constructor
//          - Always use System types in ReferenceType methods call
//          - Better check of reserved keywords and @@ prefixing
//              - known bugs :
//                  Exit in Switch/Case
// V0.1.5.0 - Cosmetic changes
// V0.1.6.0 - Try to handle Virtual/internal a better way                 
// V0.1.7.0 - Adding References in Assembly Info
//          - Correcting TryCastExpression       
//          - Correcting visibility (protected internal) Thanks to R.vdH at VODC 2009
// V0.1.8.0 - Correction in Try...Catch...Finally...End Try
//          - Added ptr as a reserved word


namespace Reflector.Application.Languages
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using Reflector.CodeModel;
    using Reflector.CodeModel.Memory;
    using System.Xml;
    using System.Collections.Specialized;

    internal partial class VulcanLanguage : ILanguage
    {
        private bool addInMode;

        public VulcanLanguage()
        {
            this.addInMode = false;
        }

        public VulcanLanguage(bool addInMode)
        {
            this.addInMode = addInMode;
        }

        public string Name
        {
            get
            {
                return (!this.addInMode) ? "Vulcan" : "Vulcan";
            }
        }

        public string FileExtension
        {
            get
            {
                return ".prg";
            }
        }

        public bool Translate
        {
            get
            {
                return true;
            }
        }

        public ILanguageWriter GetWriter(IFormatter formatter, ILanguageWriterConfiguration configuration)
        {
            return new LanguageWriter(formatter, configuration);
        }

        internal class LanguageWriter : ILanguageWriter
        {
            private IFormatter formatter;
            private ILanguageWriterConfiguration configuration;

            private static Hashtable specialMethodNames;
            private static Hashtable specialTypeNames;
            private bool forLoop = false;
            private bool firstStmt = false;
            private int blockStatementLevel = 0;
            private NumberFormat numberFormat;
            // Initialized with cfg file
            private bool ArrayIsOneBased;
            private bool ExternalizeNestedTypes;
            private bool RemovePropAttribute;
            private bool FullyQualifiedTypes;
            private bool QualifySystemOnly;
            private bool AlwaysCallSuper;
            //
            private bool KeepSystemType;
            private bool KeepKeyword;

            private enum NumberFormat
            {
                Auto,
                Hexadecimal,
                Decimal
            }

            public LanguageWriter(IFormatter formatter, ILanguageWriterConfiguration configuration)
            {
                this.formatter = formatter;
                this.configuration = configuration;
                // My own Vulcan setup
                VulcanLanguageCfg VulcanSetup = new VulcanLanguageCfg();
                this.ExternalizeNestedTypes = VulcanSetup.ExternalizeNestedTypes;
                this.ArrayIsOneBased = VulcanSetup.ArrayIsOneBased;
                this.RemovePropAttribute = VulcanSetup.RemovePropertyAttribute;
                this.FullyQualifiedTypes = VulcanSetup.FullyQualifiedTypes;
                this.QualifySystemOnly = VulcanSetup.QualifySytemOnly;
                this.AlwaysCallSuper = VulcanSetup.AlwaysCallSuper;
                //
                this.KeepSystemType = false;
                this.KeepKeyword = false;
                //

                if (specialTypeNames == null)
                {
                    specialTypeNames = new Hashtable();
                    specialTypeNames["Void"] = "void";
                    specialTypeNames["Object"] = "Object";
                    specialTypeNames["String"] = "string";
                    specialTypeNames["SByte"] = "SByte";
                    specialTypeNames["Byte"] = "Byte";
                    specialTypeNames["Int16"] = "Short";
                    specialTypeNames["UInt16"] = "Word";
                    specialTypeNames["Int32"] = "Long";
                    specialTypeNames["UInt32"] = "DWord";
                    specialTypeNames["Int64"] = "Int64";
                    specialTypeNames["UInt64"] = "UInt64";
                    specialTypeNames["Char"] = "Char";
                    specialTypeNames["Boolean"] = "Logic";
                    specialTypeNames["Single"] = "real4";
                    specialTypeNames["Double"] = "real8";
                    specialTypeNames["Decimal"] = "Decimal";
                    // TO avoid mismatch with Vulcan Array
                    specialTypeNames["Array"] = "System.Array";

                }

                if (specialMethodNames == null)
                {
                    specialMethodNames = new Hashtable();
                    specialMethodNames["op_UnaryPlus"] = "+";
                    specialMethodNames["op_Addition"] = "+";
                    specialMethodNames["op_Increment"] = "++";
                    specialMethodNames["op_UnaryNegation"] = "-";
                    specialMethodNames["op_Subtraction"] = "-";
                    specialMethodNames["op_Decrement"] = "--";
                    specialMethodNames["op_Multiply"] = "*";
                    specialMethodNames["op_Division"] = "/";
                    specialMethodNames["op_Modulus"] = "%";
                    specialMethodNames["op_BitwiseAnd"] = "&";
                    specialMethodNames["op_BitwiseOr"] = "|";
                    specialMethodNames["op_ExclusiveOr"] = "^";
                    specialMethodNames["op_Negation"] = "!";
                    specialMethodNames["op_OnesComplement"] = "~";
                    specialMethodNames["op_LeftShift"] = "<<";
                    specialMethodNames["op_RightShift"] = ">>";
                    specialMethodNames["op_Equality"] = "==";
                    specialMethodNames["op_Inequality"] = "!=";
                    specialMethodNames["op_GreaterThanOrEqual"] = ">=";
                    specialMethodNames["op_LessThanOrEqual"] = "<=";
                    specialMethodNames["op_GreaterThan"] = ">";
                    specialMethodNames["op_LessThan"] = "<";
                    specialMethodNames["op_True"] = "True";
                    specialMethodNames["op_False"] = "False";
                    specialMethodNames["op_Implicit"] = "implicit";
                    specialMethodNames["op_Explicit"] = "explicit";
                }

                switch (configuration["NumberFormat"])
                {
                    case "Hexadecimal":
                        this.numberFormat = NumberFormat.Hexadecimal;
                        break;

                    case "Decimal":
                        this.numberFormat = NumberFormat.Decimal;
                        break;

                    default:
                        this.numberFormat = NumberFormat.Auto;
                        break;
                }
            }

            public void WriteAssembly(IAssembly value)
            {
                //
                this.formatter.Write("// Assembly");
                this.formatter.Write(" ");
                this.formatter.WriteDeclaration(value.Name);

                if (value.Version != null)
                {
                    this.formatter.Write(", ");
                    this.formatter.Write("Version");
                    this.formatter.Write(" ");
                    this.formatter.Write(value.Version.ToString());
                }

                this.formatter.WriteLine();

                if ((this.configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
                {
                    //
                    ArrayList References = new ArrayList();
                    for (int i = 0; i < value.Attributes.Count; i++)
                    {
                        ICustomAttribute attribute = value.Attributes[i];
                        ITypeReference typeReference = attribute.Constructor.DeclaringType as ITypeReference;
                        if (typeReference != null)
                        {
                            if (!References.Contains(typeReference.Namespace))
                            {
                                formatter.WriteKeyword("#using ");
                                formatter.Write(" ");
                                WriteDeclaration(typeReference.Namespace, formatter);
                                formatter.WriteLine();
                                References.Add(typeReference.Namespace);
                            }
                        }
                    }
                    //
                    this.formatter.WriteLine();
                    this.WriteCustomAttributeList(value, this.formatter);
                    this.formatter.WriteLine();
                }

                this.formatter.WriteProperty("Location", value.Location);
                this.formatter.WriteProperty("Name", value.ToString());

                switch (value.Type)
                {
                    case AssemblyType.Application:
                        this.formatter.WriteProperty("Type", "Windows Application");
                        break;

                    case AssemblyType.Console:
                        this.formatter.WriteProperty("Type", "Console Application");
                        break;

                    case AssemblyType.Library:
                        this.formatter.WriteProperty("Type", "Library");
                        break;
                }
            }

            public void WriteAssemblyReference(IAssemblyReference value)
            {
                this.formatter.Write("// Assembly Reference");
                this.formatter.Write(" ");
                this.formatter.WriteDeclaration(value.Name);
                this.formatter.WriteLine();

                this.formatter.WriteProperty("Version", value.Version.ToString());
                this.formatter.WriteProperty("Name", value.ToString());
            }

            public void WriteModule(IModule value)
            {
                this.formatter.Write("// Module");
                this.formatter.Write(" ");
                this.formatter.WriteDeclaration(value.Name);
                this.formatter.WriteLine();

                if ((this.configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
                {
                    this.formatter.WriteLine();
                    this.WriteCustomAttributeList(value, this.formatter);
                    this.formatter.WriteLine();
                }

                this.formatter.WriteProperty("Version", value.Version.ToString());
                this.formatter.WriteProperty("Location", value.Location);

                string location = Environment.ExpandEnvironmentVariables(value.Location);
                if (File.Exists(location))
                {
                    this.formatter.WriteProperty("Size", new FileInfo(location).Length + " Bytes");
                }
            }

            public void WriteModuleReference(IModuleReference value)
            {
                this.formatter.Write("// Module Reference");
                this.formatter.Write(" ");
                this.formatter.WriteDeclaration(value.Name);
                this.formatter.WriteLine();
            }

            public void WriteResource(IResource value)
            {
                this.formatter.Write("// ");

                switch (value.Visibility)
                {
                    case ResourceVisibility.Public:
                        this.formatter.WriteKeyword("public");
                        break;

                    case ResourceVisibility.Private:
                        this.formatter.WriteKeyword("private");
                        break;
                }

                this.formatter.Write(" ");
                this.formatter.WriteKeyword("resource");
                this.formatter.Write(" ");
                this.formatter.WriteDeclaration(value.Name, value);
                this.formatter.WriteLine();

                IEmbeddedResource embeddedResource = value as IEmbeddedResource;
                if ((embeddedResource != null) && (embeddedResource.Value != null))
                {
                    this.formatter.WriteProperty("Size", embeddedResource.Value.Length.ToString(CultureInfo.InvariantCulture) + " bytes");
                }

                IFileResource fileResource = value as IFileResource;
                if (fileResource != null)
                {
                    this.formatter.WriteProperty("Location", fileResource.Location);
                }
            }

            public void WriteNamespace(INamespace value)
            {
                //				formatter.Write(";");
                //if (configuration["ShowNamespaceImports"] == "true")
                {
                    formatter.WriteLine();

                    ITypeReferenceCollection typeReferenceCollection = new TypeReferenceCollection();
                    //
                    foreach (ITypeDeclaration typeDeclaration in value.Types)
                    {
                        if ((typeDeclaration.Namespace.Length != 0) || (
                            (typeDeclaration.Name != "<Module>") &&
                            (typeDeclaration.Name != "<PrivateImplementationDetails>")))
                        {
                            //
                            typeReferenceCollection.Add(typeDeclaration.BaseType);
                        }
                    }
                    // Add each Reference at the beginning of the module
                    StringCollection typeRefDone = new StringCollection();
                    foreach (ITypeReference typeReference in typeReferenceCollection)
                    {
                        if (typeReference != null)
                        {
                            if (!typeRefDone.Contains(typeReference.Namespace))
                            {
                                formatter.WriteKeyword("#using ");
                                formatter.Write(" ");
                                WriteDeclaration(typeReference.Namespace, formatter);
                                formatter.WriteLine();
                                //
                                typeRefDone.Add(typeReference.Namespace);
                            }
                        }
                    }
                }
                //
                if (value.Name.Length != 0)
                {
                    // No namespace in Vulcan.Net....currently !
                    // Err... Starting with V155
                    formatter.WriteKeyword("BEGIN");
                    formatter.Write(" ");
                    formatter.WriteKeyword("NAMESPACE");
                    formatter.Write(" ");
                    WriteDeclaration(value.Name, formatter);
                    formatter.WriteLine();
                    formatter.WriteIndent();
                }
                //if (configuration["ShowNamespaceBody"] == "true")
                {
                    formatter.WriteLine();
                    //
                    ArrayList types = new ArrayList();
                    foreach (ITypeDeclaration typeDeclaration in value.Types)
                    {
                        if (Helper.IsVisible(typeDeclaration, configuration.Visibility))
                        {
                            types.Add(typeDeclaration);
                        }
                    }

                    types.Sort();

                    for (int i = 0; i < types.Count; i++)
                    {
                        if (i != 0)
                        {
                            formatter.WriteLine();
                        }

                        this.WriteTypeDeclaration((ITypeDeclaration)types[i]);
                    }

                }
                //
                if (value.Name.Length != 0)
                {
                    // Starting with V155
                    formatter.WriteOutdent();
                    formatter.WriteLine();
                    formatter.WriteKeyword("END");
                    formatter.Write(" ");
                    formatter.WriteKeyword("NAMESPACE");
                    formatter.Write(" ");
                    formatter.WriteComment("// " + value.Name);
                    formatter.WriteLine();
                }

            }

            public void WriteTypeDeclaration(ITypeDeclaration value)
            {
                if ((configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
                {
                    this.WriteCustomAttributeList(value, formatter);
                    formatter.WriteLine();
                }
                // Visibility Internal, Private, export, ...
                WriteTypeVisibility(value.Visibility, formatter);
                //
                ITypeReference declaringType;
                string valueName = "";
                //
                if (Helper.IsDelegate(value))
                {
                    #region IsDelegate
                    IMethodDeclaration methodDeclaration = Helper.GetMethod(value, "Invoke");

                    formatter.WriteKeyword("delegate");
                    formatter.Write(" ");
                    WriteDeclaration(value.Name, value, formatter);

                    // Generic Parameters
                    this.WriteGenericArgumentList(methodDeclaration.GenericArguments, formatter);

                    // Method Parameters
                    if ((methodDeclaration.Parameters.Count > 0) || (methodDeclaration.CallingConvention == MethodCallingConvention.VariableArguments))
                    {
                        formatter.Write("(");
                        this.WriteParameterDeclarationList(methodDeclaration.Parameters, formatter, configuration);
                        formatter.Write(")");
                    }
                    this.WriteGenericParameterConstraintList(methodDeclaration, formatter);

                    formatter.Write(" ");
                    formatter.WriteKeyword("as");
                    formatter.Write(" ");
                    this.WriteType(methodDeclaration.ReturnType.Type, formatter);
                    #endregion
                }
                else
                    #region IsEnumeration
                    if (Helper.IsEnumeration(value))
                    {
                        bool first = true;

                        formatter.WriteKeyword("enum");
                        formatter.Write(" ");
                        //
                        if ((value.Visibility > TypeVisibility.Public) && this.ExternalizeNestedTypes)
                        {
                            declaringType = value.Owner as ITypeReference;
                            if (declaringType != null)
                            {
                                valueName = declaringType.Name + "." + value.Name;
                            }
                        }
                        else
                            valueName = value.Name;
                        //
                        this.WriteDeclaration(valueName, value, formatter);
                        // Retrieve Enum members and search for "value__" member
                        ICollection enumMembers = Helper.GetFields(value, configuration.Visibility);
                        if (enumMembers.Count > 0)
                        {
                            foreach (IFieldDeclaration enumDeclaration in enumMembers)
                            {
                                if ((enumDeclaration.SpecialName) && (enumDeclaration.Name == "value__"))
                                {
                                    formatter.Write(" ");
                                    formatter.WriteKeyword("as");
                                    formatter.Write(" ");
                                    this.WriteType(enumDeclaration.FieldType, formatter);
                                }
                            }
                        }


                        if (configuration["ShowTypeDeclarationBody"] == "true")
                        {
                            formatter.WriteLine();
                            formatter.WriteIndent();
                            //
                            foreach (IFieldDeclaration fieldDeclaration in Helper.GetFields(value, configuration.Visibility))
                            {
                                // Do not render underlying "value__" field
                                if ((!fieldDeclaration.SpecialName) || (!fieldDeclaration.RuntimeSpecialName) || (fieldDeclaration.FieldType.Equals(value)))
                                {
                                    if (first)
                                    {
                                        first = false;
                                    }
                                    else
                                    {
                                        formatter.WriteLine();
                                    }
                                    formatter.WriteKeyword("member");
                                    formatter.Write(" ");
                                    this.WriteDeclaration(fieldDeclaration.Name, fieldDeclaration, formatter);
                                    IExpression initializer = fieldDeclaration.Initializer;
                                    if (initializer != null)
                                    {
                                        formatter.Write(":=");
                                        this.WriteExpression(initializer, formatter);
                                    }
                                }
                            }
                            //
                            formatter.WriteOutdent();
                            formatter.WriteLine();
                            formatter.WriteKeyword("end");
                            formatter.Write(" ");
                            formatter.WriteKeyword("enum");
                            formatter.WriteLine();
                        }
                    }
                    #endregion
                    else
                    {
                        bool bracketPrinted = false;
                        //
                        if ((value.Visibility > TypeVisibility.Public) && this.ExternalizeNestedTypes)
                        {
                            declaringType = value.Owner as ITypeReference;
                            if (declaringType != null)
                            {
                                valueName = declaringType.Name + "." + value.Name;
                            }
                        }
                        else
                            valueName = value.Name;
                        //
                        if (Helper.IsValueType(value))
                        {
                            formatter.WriteKeyword("structure");
                            formatter.Write(" ");
                            this.WriteDeclaration(valueName, value, formatter);
                            this.WriteGenericArgumentList(value.GenericArguments, formatter);
                        }
                        else if (value.Interface)
                        {
                            formatter.WriteKeyword("interface");
                            formatter.Write(" ");
                            this.WriteDeclaration(valueName, value, formatter);
                            this.WriteGenericArgumentList(value.GenericArguments, formatter);
                        }
                        else
                        {
                            if ((value.Abstract) && (value.Sealed))
                            {
                                //formatter.Write(" ");
                                formatter.WriteKeyword("static ;");
                                formatter.WriteLine();
                            }
                            else
                            {
                                if (value.Abstract)
                                {
                                    //formatter.Write(" ");
                                    formatter.WriteKeyword("abstract ;");
                                    formatter.WriteLine();
                                }

                                if (value.Sealed)
                                {
                                    //formatter.Write(" ");
                                    formatter.WriteKeyword("sealed ;");
                                    formatter.WriteLine();

                                }
                            }
                            formatter.WriteKeyword("class");
                            formatter.Write(" ");
                            this.WriteDeclaration(valueName, value, formatter);

                            this.WriteGenericArgumentList(value.GenericArguments, formatter);

                            ITypeReference baseType = value.BaseType;
                            if ((baseType != null) && (!IsType(baseType, "System", "Object")))
                            {
                                formatter.Write(" ");
                                formatter.WriteKeyword("inherit");
                                formatter.Write(" ");
                                this.WriteType(baseType, formatter);
                                //bracketPrinted = true;
                            }
                        }

                        // TODO filter interfaces
                        foreach (ITypeReference interfaceType in value.Interfaces)
                        {
                            if (bracketPrinted)
                            {
                                formatter.Write(", ");
                            }
                            else
                            {
                                formatter.Write(" ");
                                formatter.WriteKeyword("implements");
                                formatter.Write(" ");
                            }
                            //
                            this.WriteType(interfaceType, formatter);
                            bracketPrinted = true;
                        }

                        this.WriteGenericParameterConstraintList(value, formatter);
                        if (bracketPrinted)
                        {
                            //formatter.Write(")");
                        }
                    }

                formatter.WriteProperty("Name", GetDelphiStyleResolutionScope(value));
                this.WriteDeclaringAssembly(Helper.GetAssemblyReference(value), formatter);

                if ((configuration["ShowTypeDeclarationBody"] == "true") && (!Helper.IsEnumeration(value)) && (!Helper.IsDelegate(value)))
                {
                    formatter.WriteLine();
                    formatter.WriteIndent();

                    bool newLine = false;

                    ICollection fields = Helper.GetFields(value, configuration.Visibility);
                    if (fields.Count > 0)
                    {
                        if (newLine)
                            formatter.WriteLine();
                        newLine = true;
                        formatter.WriteComment("// Fields");
                        formatter.WriteLine();

                        foreach (IFieldDeclaration fieldDeclaration in fields)
                            if ((!fieldDeclaration.SpecialName) || (fieldDeclaration.Name != "value__"))
                            {
                                this.WriteFieldDeclaration(fieldDeclaration);
                                formatter.WriteLine();
                            }
                    }

                    ICollection events = Helper.GetEvents(value, configuration.Visibility);
                    if (events.Count > 0)
                    {
                        if (newLine)
                            formatter.WriteLine();
                        newLine = true;
                        formatter.WriteComment("// Events");
                        formatter.WriteLine();

                        foreach (IEventDeclaration eventDeclaration in events)
                        {
                            this.WriteEventDeclaration(eventDeclaration);
                            formatter.WriteLine();
                        }
                    }

                    ICollection methods = Helper.GetMethods(value, configuration.Visibility);
                    if (methods.Count > 0)
                    {
                        if (newLine)
                            formatter.WriteLine();
                        newLine = true;
                        formatter.WriteComment("// Methods");
                        formatter.WriteLine();

                        foreach (IMethodDeclaration methodDeclaration in methods)
                        {
                            this.WriteMethodDeclaration(methodDeclaration);
                            formatter.WriteLine();
                        }
                    }

                    ICollection properties = Helper.GetProperties(value, configuration.Visibility);
                    if (properties.Count > 0)
                    {
                        if (newLine)
                            formatter.WriteLine();
                        newLine = true;
                        formatter.WriteComment("// Properties");
                        formatter.WriteLine();

                        foreach (IPropertyDeclaration propertyDeclaration in properties)
                        {
                            this.WritePropertyDeclaration(propertyDeclaration);
                            formatter.WriteLine();
                        }
                    }

                    ICollection nestedTypes;
                    // Nested Types should there !!!!!!!!
                    if (!this.ExternalizeNestedTypes)
                    {
                        nestedTypes = Helper.GetNestedTypes(value, configuration.Visibility);
                        if (nestedTypes.Count > 0)
                        {
                            if (newLine)
                                formatter.WriteLine();
                            newLine = true;

                            //formatter.WriteKeyword("type");
                            //formatter.Write(" ");
                            formatter.WriteComment("// Nested Types");
                            formatter.WriteLine();
                            formatter.WriteIndent();
                            foreach (ITypeDeclaration nestedTypeDeclaration in nestedTypes)
                            {
                                this.WriteTypeDeclaration(nestedTypeDeclaration);
                                formatter.WriteLine();
                            }
                            formatter.WriteOutdent();
                        }
                    }

                    formatter.WriteLine();
                    formatter.WriteOutdent();
                    formatter.WriteKeyword("end");
                    formatter.Write(" ");
                    if (Helper.IsValueType(value))
                    {
                        formatter.WriteKeyword("structure");
                    }
                    else if (value.Interface)
                    {
                        formatter.WriteKeyword("interface");
                    }
                    else
                    {
                        formatter.WriteKeyword("class");
                    }
                    //formatter.Write(";");
                    formatter.WriteLine();
                    // Workaround for Nested types ... :-(
                    if (ExternalizeNestedTypes)
                    {
                        nestedTypes = Helper.GetNestedTypes(value, configuration.Visibility);
                        if (nestedTypes.Count > 0)
                        {
                            if (newLine)
                                formatter.WriteLine();
                            newLine = true;

                            //formatter.WriteKeyword("type");
                            //formatter.Write(" ");
                            formatter.WriteComment("// Nested Types");
                            formatter.WriteLine();
                            formatter.WriteIndent();
                            //
                            foreach (ITypeDeclaration nestedTypeDeclaration in nestedTypes)
                            {
                                this.WriteTypeDeclaration(nestedTypeDeclaration);
                                formatter.WriteLine();
                            }
                            formatter.WriteOutdent();
                        }
                    }
                }
            }

            public void WriteTypeVisibility(TypeVisibility visibility, IFormatter formatter)
            {
                switch (visibility)
                {
                    case TypeVisibility.Public:
                        // public per default in Vulcan, could be left blank...
                        //formatter.WriteKeyword("export"); 
                        break;
                    case TypeVisibility.NestedPublic:
                    // formatter.WriteKeyword("export"); break;
                    case TypeVisibility.Private:
                        formatter.WriteKeyword("internal"); break;
                    case TypeVisibility.NestedAssembly:
                        formatter.WriteKeyword("internal"); break;
                    case TypeVisibility.NestedPrivate:
                        formatter.WriteKeyword("internal"); break;
                    case TypeVisibility.NestedFamily:
                        formatter.WriteKeyword("internal"); break;
                    //formatter.WriteKeyword("protected"); break;
                    case TypeVisibility.NestedFamilyAndAssembly:
                        formatter.WriteKeyword("internal"); break;
                    //formatter.WriteKeyword("protected"); break;
                    case TypeVisibility.NestedFamilyOrAssembly:
                        formatter.WriteKeyword("internal"); break;
                    default: throw new NotSupportedException();
                }
                if (visibility != TypeVisibility.Public)
                {
                    formatter.Write(" ");
                }
            }

            public void WriteFieldVisibility(FieldVisibility visibility, IFormatter formatter)
            {
                switch (visibility)
                {
                    case FieldVisibility.Public:
                        // public per default in Vulcan, could be left blank...
                        formatter.WriteKeyword("export");
                        break;
                    case FieldVisibility.Private:
                        formatter.WriteKeyword("private"); break;
                    case FieldVisibility.PrivateScope:
                        formatter.WriteKeyword("private"); break;
                    case FieldVisibility.Family:
                        formatter.WriteKeyword("protected"); break;
                    case FieldVisibility.Assembly:
                        formatter.WriteKeyword("internal"); break;
                    case FieldVisibility.FamilyOrAssembly:
                        formatter.WriteKeyword("protected"); break;
                    case FieldVisibility.FamilyAndAssembly:
                        formatter.WriteKeyword("protected"); break;
                    default: throw new NotSupportedException();
                }
                //if (visibility != FieldVisibility.Public)
                //{
                formatter.Write(" ");
                //}
            }

            public void WriteMethodVisibility(MethodVisibility visibility, IFormatter formatter)
            {
                switch (visibility)
                {
                    case MethodVisibility.Public:
                        // public per default in Vulcan, could be left blank...
                        //formatter.WriteKeyword("export"); 
                        break;
                    case MethodVisibility.Private:
                        formatter.WriteKeyword("private"); break;

                    case MethodVisibility.PrivateScope:
                        formatter.WriteKeyword("private"); break;

                    case MethodVisibility.Family:
                        formatter.WriteKeyword("protected"); break;

                    case MethodVisibility.Assembly:
                        formatter.WriteKeyword("internal"); break;

                    case MethodVisibility.FamilyOrAssembly:
                        formatter.WriteKeyword("protected internal"); break;

                    case MethodVisibility.FamilyAndAssembly:
                        formatter.WriteKeyword("protected"); break;

                    default: throw new NotSupportedException();
                }
                if (visibility != MethodVisibility.Public)
                {
                    formatter.Write(" ");
                }
            }

            public void WriteFieldDeclaration(IFieldDeclaration value)
            {
                byte[] data = null;
                IExpression initializer = value.Initializer;
                if (initializer != null)
                {
                    ILiteralExpression literalExpression = initializer as ILiteralExpression;
                    if ((literalExpression != null) && (literalExpression.Value != null) && (literalExpression.Value is byte[]))
                    {
                        data = (byte[])literalExpression.Value;
                    }
                    else
                    {
                        //formatter.Write(" := ");
                        //this.WriteExpression(initializer, formatter);
                    }
                }

                if ((configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
                {
                    this.WriteCustomAttributeList(value, formatter);
                    formatter.WriteLine();
                }

                if (!this.IsEnumerationElement(value))
                {
                    // First, is it a class field ?
                    if ((value.Static) && (value.Literal))
                    {
                        formatter.WriteKeyword("/*const*/");
                        formatter.Write(" ");
                    }
                    else
                    {
                        if (value.Static)
                        {
                            formatter.WriteKeyword("static");
                            formatter.Write(" ");
                        }
                        if (value.ReadOnly)
                        {
                            formatter.WriteKeyword("/*readonly*/");
                            formatter.Write(" ");
                        }
                    }
                    // Second, the visibility (Private, Export, ... )
                    this.WriteFieldVisibility(value.Visibility, formatter);
                    // Then the declaration
                    this.WriteDeclaration(value.Name, value, formatter);
                    //
                    if ((initializer != null) && (data == null))
                    {
                        formatter.Write(" := ");
                        this.WriteExpression(initializer, formatter);
                    }
                    //
                    formatter.Write(" ");
                    formatter.WriteKeyword("as");
                    formatter.Write(" ");
                    this.WriteType(value.FieldType, formatter);
                }
                else
                {
                    this.WriteDeclaration(value.Name, value, formatter);
                }



                if (!this.IsEnumerationElement(value))
                {
                    //formatter.Write(";");
                }

                if (data != null)
                {
                    this.formatter.WriteComment(" // data size: " + data.Length.ToString(CultureInfo.InvariantCulture) + " bytes");
                }

                this.WriteDeclaringType(value.DeclaringType as ITypeReference, formatter);
            }

            public void WriteMethodDeclaration(IMethodDeclaration value)
            {
                string method = "method";
                bool isFunction = false;
                string methodName = value.Name;

                if (value.Body == null)
                {
                    if ((configuration["ShowCustomAttributes"] == "true") && (value.ReturnType.Attributes.Count != 0))
                    {
                        this.WriteCustomAttributeList(value.ReturnType, formatter);
                        formatter.WriteLine();
                    }

                    if ((configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
                    {
                        this.WriteCustomAttributeList(value, formatter);
                        formatter.WriteLine();
                    }

                    if (this.GetCustomAttribute(value, "System.Runtime.InteropServices", "DllImportAttribute") != null)
                    {
                        // No Need for Extern in Vulcan
                        //formatter.WriteKeyword("extern");
                        //formatter.Write(" ");
                    }
                }

                if (this.IsConstructor(value))
                {
                    method = ""; // "constructor";
                    methodName = "constructor";
                }
                else
                    if ((value.SpecialName) && (specialMethodNames.Contains(methodName)))
                    {
                        method = "operator";
                        methodName = (string)specialMethodNames[(object)methodName];
                        isFunction = true;
                    }
                    else
                    {
                        if (!IsType(value.ReturnType.Type, "System", "Void"))
                        {
                            //method = "function";
                            isFunction = true;
                        }
                        isFunction = true;
                    }

                // protected, internal, 
                this.WriteMethodAttributes(value, formatter);
                // static, abstract, virtual, ...
                this.WriteMethodDirectives(value, formatter);
                // Method, Constructor, 
                formatter.WriteKeyword(method);
                formatter.Write(" ");
                if (value.Body != null)
                {
                    // ClassName.
                    //					this.WriteDeclaringTypeReference(value.DeclaringType as ITypeReference, formatter);
                }
                //
                this.WriteDeclaration(methodName, value, formatter);

                // Generic Parameters
                this.WriteGenericArgumentList(value.GenericArguments, formatter);

                formatter.Write("(");
                // Method Parameters
                if ((value.Parameters.Count > 0) || (value.CallingConvention == MethodCallingConvention.VariableArguments))
                {
                    this.WriteParameterDeclarationList(value.Parameters, formatter, configuration);
                    if (value.CallingConvention == MethodCallingConvention.VariableArguments)
                    {
                        formatter.Write(" /*; __arglist*/");
                    }
                }
                formatter.Write(")");

                this.WriteGenericParameterConstraintList(value, formatter);

                if (isFunction)
                {
                    formatter.Write(" ");
                    formatter.WriteKeyword("as");
                    formatter.Write(" ");
                    this.WriteType(value.ReturnType.Type, formatter);
                }
                //
                if (this.IsConstructor(value))
                {
                    bool GenSuper = true;
                    // Constructor arguments
                    IConstructorDeclaration constructorDeclaration = value as IConstructorDeclaration;
                    if ((constructorDeclaration != null) && (constructorDeclaration.Initializer != null))
                    {
                        IMethodReferenceExpression methodReferenceExpression = constructorDeclaration.Initializer.Method as IMethodReferenceExpression;
                        if (methodReferenceExpression != null)
                        {
                            IBaseReferenceExpression baseReferenceExpression = methodReferenceExpression.Target as IBaseReferenceExpression;
                            if ((baseReferenceExpression == null) || (constructorDeclaration.Initializer.Arguments.Count != 0))
                            {
                                // Inline call to base() in C#
                                formatter.WriteComment("//Inline call to base() in C#");
                                //formatter.Write(" : ");
                                // Rewrite as first body line of code in Vulcan
                                formatter.WriteLine();
                                this.WriteExpression(methodReferenceExpression.Target, formatter);
                                formatter.Write("(");
                                this.WriteExpressionList(constructorDeclaration.Initializer.Arguments, formatter);
                                formatter.Write(")");
                                //
                                GenSuper = false;
                            }
                        }
                    }
                    // 
                    if ((GenSuper) && (this.AlwaysCallSuper))
                    {
                        formatter.Write(";");
                        formatter.WriteKeyword("super");
                        formatter.Write("()");
                    }

                }
                //formatter.Write(";");

                IBlockStatement body = value.Body as IBlockStatement;
                if (body == null)
                {
                    //this.WriteMethodDirectives(value, formatter);
                }
                else
                {
                    // Method Body

                    // we need to dump the Variable list first
                    bool hasvar = false;
                    this.WriteVariableList(body.Statements, formatter, ref hasvar);
                    if (hasvar == true)
                    {
                        formatter.WriteOutdent();
                    }
                    else
                    {
                        formatter.WriteLine();
                    }
                    //formatter.WriteKeyword("begin");
                    //formatter.WriteLine();
                    formatter.WriteIndent();
                    blockStatementLevel = 0; // to optimize exit() for Delphi
                    this.WriteStatement(body, formatter);
                    formatter.WriteLine();
                    formatter.WriteOutdent();
                    //
                }

                this.WriteDeclaringType(value.DeclaringType as ITypeReference, formatter);
            }

            public void WritePropertyDeclaration(IPropertyDeclaration value)
            {
                // Name
                string propertyName = value.Name;
                IBlockStatement body = null;
                //Property
                if ((configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
                {
                    if (!this.RemovePropAttribute)
                    {
                        this.WriteCustomAttributeList(value, formatter);
                        formatter.WriteLine();
                    }
                }

                IMethodDeclaration getMethod = null;
                if (value.GetMethod != null)
                {
                    getMethod = value.GetMethod.Resolve();
                }

                IMethodDeclaration setMethod = null;
                if (value.SetMethod != null)
                {
                    setMethod = value.SetMethod.Resolve();
                }

                bool hasSameAttributes = true;
                if ((getMethod != null) && (setMethod != null))
                {
                    hasSameAttributes &= (getMethod.Visibility == setMethod.Visibility);
                    hasSameAttributes &= (getMethod.Static == setMethod.Static);
                    hasSameAttributes &= (getMethod.Final == setMethod.Final);
                    hasSameAttributes &= (getMethod.Virtual == setMethod.Virtual);
                    hasSameAttributes &= (getMethod.Abstract == setMethod.Abstract);
                    hasSameAttributes &= (getMethod.NewSlot == setMethod.NewSlot);
                }

                // Access
                if (getMethod != null)
                {
                    // protected, internal, 
                    this.WriteMethodAttributes(getMethod, formatter);
                    // static, abstract, virtual, ...
                    this.WriteMethodDirectives(getMethod, formatter);
                    formatter.WriteKeyword("access");
                    formatter.Write(" ");
                    this.WriteDeclaration(propertyName, value, formatter);
                    formatter.Write(" ");
                    formatter.WriteKeyword("as");
                    formatter.Write(" ");
                    this.WriteType(value.PropertyType, formatter);
                    //formatter.WriteLine();
                    //
                    if (getMethod.Body != null)
                    {
                        body = getMethod.Body as IBlockStatement;
                        if (body != null)
                        {
                            //this.WriteMethodDeclaration(getMethod);
                            // Method Body
                            // we need to dump the Variable list first
                            bool hasvar = false;
                            this.WriteVariableList(body.Statements, formatter, ref hasvar);
                            if (hasvar == true)
                            {
                                formatter.WriteOutdent();
                            }
                            else
                            {
                                formatter.WriteLine();
                            }
                            formatter.WriteIndent();
                            blockStatementLevel = 0; // to optimize exit() for Delphi
                            this.WriteStatement(body, formatter);
                            formatter.WriteLine();
                            formatter.WriteOutdent();
                            //
                        }
                    }
                }

                // Assign
                if (setMethod != null)
                {
                    // protected, internal, 
                    this.WriteMethodAttributes(setMethod, formatter);
                    // static, abstract, virtual, ...
                    this.WriteMethodDirectives(setMethod, formatter);
                    formatter.WriteKeyword("assign");
                    formatter.Write(" ");
                    this.WriteDeclaration(propertyName, value, formatter);
                    //
                    formatter.Write("(");
                    // Parameters
                    if (setMethod.Parameters.Count > 0)
                    {
                        this.WriteParameterDeclarationList(setMethod.Parameters, formatter, configuration);
                    }
                    formatter.Write(")");
                    //
                    //formatter.WriteLine();
                    //
                    if (setMethod.Body != null)
                    {
                        body = setMethod.Body as IBlockStatement;
                        if (body != null)
                        {
                            //this.WriteMethodDeclaration(setMethod);
                            //formatter.WriteLine();
                            // Method Body
                            // we need to dump the Variable list first
                            bool hasvar = false;
                            this.WriteVariableList(body.Statements, formatter, ref hasvar);
                            if (hasvar == true)
                            {
                                formatter.WriteOutdent();
                            }
                            else
                            {
                                formatter.WriteLine();
                            }
                            formatter.WriteIndent();
                            blockStatementLevel = 0; // to optimize exit() for Delphi
                            this.WriteStatement(body, formatter);
                            formatter.WriteLine();
                            formatter.WriteOutdent();
                            //
                        }
                    }
                }
                //
                if (value.Initializer != null)
                { // in Vulcan we do not have a property initializer. Or do we ?
                    // PS
                    //formatter.Write("{(pseudo) := ");
                    //this.WriteExpression(value.Initializer, formatter);
                    //formatter.Write(" }");
                }
                //                this.WriteDeclaringType(value.DeclaringType as ITypeReference, formatter);
            }

            public void WriteEventDeclaration(IEventDeclaration value)
            {
                if ((configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
                {
                    this.WriteCustomAttributeList(value, formatter);
                    formatter.WriteLine();
                }

                ITypeDeclaration declaringType = (value.DeclaringType as ITypeReference).Resolve();
                if (!declaringType.Interface)
                {
                    WriteMethodVisibility(Helper.GetVisibility(value), formatter);
                }

                if (Helper.IsStatic(value))
                {
                    formatter.WriteKeyword("static");
                    formatter.Write(" ");
                }
                //
                formatter.WriteLine();
                formatter.WriteLiteral("#error Sorry, currenlty not supported with Vulcan");
                formatter.WriteLine();
                //

                formatter.WriteKeyword("Event");
                formatter.Write(" ");
                formatter.Write(value.Name);
                formatter.Write(" ");
                formatter.WriteKeyword("AS");
                formatter.Write(" ");
                this.WriteType(value.EventType, formatter);
                //formatter.Write(";");
                this.WriteDeclaringType(value.DeclaringType as ITypeReference, formatter);

            }

            private void WriteDeclaringTypeReference(ITypeReference value, IFormatter formatter)
            {
                ITypeReference owner = (value.Owner as ITypeReference);
                if (owner != null)
                {
                    WriteDeclaringTypeReference(owner, formatter);
                }
                this.WriteType(value, formatter);
                formatter.Write(".");
            }

            private string GetDelphiStyleResolutionScope(ITypeReference reference)
            {
                string result = reference.ToString();
                while (true)
                {
                    ITypeReference OwnerRef = (reference.Owner as ITypeReference);
                    if (OwnerRef == null)
                    {
                        string namespacestr = reference.Namespace;
                        if (namespacestr.Length == 0)
                            return result;
                        else
                            return namespacestr + "<.>" + result;
                    }
                    reference = OwnerRef;
                    result = reference.ToString() + "<.>" + result;
                }
            }




            private void WriteType(IType type, IFormatter formatter)
            {
                ITypeReference typeReference = type as ITypeReference;
                if (typeReference != null)
                {
                    string description = Helper.GetNameWithResolutionScope(typeReference);
                    this.WriteTypeReference(typeReference, formatter, description, typeReference);
                    return;
                }

                IArrayType arrayType = type as IArrayType;
                if (arrayType != null)
                {
                    this.WriteType(arrayType.ElementType, formatter);
                    formatter.Write("[]");

                    IArrayDimensionCollection dimensions = arrayType.Dimensions;
                    for (int i = 0; i < dimensions.Count; i++)
                    {
                        if (i != 0)
                        {
                            formatter.Write(",");
                        }
                        else
                        {
                            formatter.Write("{");
                        }

                        if ((dimensions[i].LowerBound != 0) && (dimensions[i].UpperBound != -1))
                        {
                            if ((dimensions[i].LowerBound != -1) || (dimensions[i].UpperBound != -1))
                            {
                                formatter.Write((dimensions[i].LowerBound != -1) ? dimensions[i].LowerBound.ToString(CultureInfo.InvariantCulture) : ".");
                                formatter.Write("..");
                                formatter.Write((dimensions[i].UpperBound != -1) ? dimensions[i].UpperBound.ToString(CultureInfo.InvariantCulture) : ".");
                            }
                        }
                    }
                    if (dimensions.Count > 0)
                    {
                        formatter.Write("}");
                    }
                    return;
                }

                IPointerType pointerType = type as IPointerType;
                if (pointerType != null)
                {
                    this.WriteType(pointerType.ElementType, formatter);
                    formatter.Write(" ");
                    formatter.WriteKeyword("Ptr");
                    return;
                }

                IReferenceType referenceType = type as IReferenceType;
                if (referenceType != null)
                {
                    // formatter.WriteKeyword ("var"); // already done before the param name - HV
                    // formatter.Write (" ");
                    this.WriteType(referenceType.ElementType, formatter);
                    return;
                }

                IOptionalModifier optionalModifier = type as IOptionalModifier;
                if (optionalModifier != null)
                {
                    this.WriteType(optionalModifier.ElementType, formatter);
                    formatter.WriteComment("//");
                    formatter.Write(" ");
                    formatter.WriteKeyword("modopt");
                    formatter.Write("(");
                    this.WriteType(optionalModifier.Modifier, formatter);
                    formatter.Write(")");
                    return;
                }

                IRequiredModifier requiredModifier = type as IRequiredModifier;
                if (requiredModifier != null)
                {
                    this.WriteType(requiredModifier.ElementType, formatter);
                    formatter.WriteComment("//");
                    formatter.Write(" ");
                    formatter.WriteKeyword("modreq");
                    formatter.Write("(");
                    this.WriteType(requiredModifier.Modifier, formatter);
                    formatter.Write(")");
                    return;
                }

                IFunctionPointer functionPointer = type as IFunctionPointer;
                if (functionPointer != null)
                {
                    this.WriteType(functionPointer.ReturnType.Type, formatter);
                    formatter.Write(" @(");
                    for (int i = 0; i < functionPointer.Parameters.Count; i++)
                    {
                        if (i != 0)
                        {
                            formatter.Write(", ");
                        }

                        this.WriteType(functionPointer.Parameters[i].ParameterType, formatter);
                    }

                    formatter.Write(")");
                    return;
                }

                IGenericParameter genericParameter = type as IGenericParameter;
                if (genericParameter != null)
                {
                    formatter.Write(genericParameter.Name);
                    return;
                }

                IGenericArgument genericArgument = type as IGenericArgument;
                if (genericArgument != null)
                {
                    this.WriteType(genericArgument.Resolve(), formatter);
                    return;
                }

                throw new NotSupportedException();
            }

            private void WriteMethodAttributes(IMethodDeclaration methodDeclaration, IFormatter formatter)
            {
                ITypeDeclaration declaringType = (methodDeclaration.DeclaringType as ITypeReference).Resolve();
                if (!declaringType.Interface)
                {
                    WriteMethodVisibility(methodDeclaration.Visibility, formatter);

                    if (methodDeclaration.Static)
                    {
                        //formatter.WriteKeyword("class");
                        //formatter.Write(" ");
                    }
                }
            }

            private void WriteMethodDirectives(IMethodDeclaration methodDeclaration, IFormatter formatter)
            {
                ITypeDeclaration declaringType = (methodDeclaration.DeclaringType as ITypeReference).Resolve();
                if (!declaringType.Interface)
                {
                    //formatter.Write(" ");

                    if (methodDeclaration.Static)
                    {
                        formatter.WriteKeyword("static");
                        formatter.Write(" ");
                    }

                    if ((methodDeclaration.Final) && (!methodDeclaration.NewSlot))
                    {
                        formatter.WriteKeyword("sealed");
                        formatter.Write(" ");
                    }

                    if (methodDeclaration.Virtual)
                    {
                        //if (methodDeclaration.Abstract)
                        //{
                        //    formatter.WriteKeyword("abstract");
                        //    formatter.Write(" ");
                        //}
                        //else if ((methodDeclaration.NewSlot) && (!methodDeclaration.Final))
                        //{
                        //    formatter.WriteKeyword("virtual");
                        //    formatter.Write(" ");
                        //}

                        //if (!methodDeclaration.NewSlot)
                        //{
                        //    formatter.WriteKeyword("virtual");
                        //    formatter.Write(" ");
                        //}

                        if (methodDeclaration.Abstract)
                        {
                            formatter.WriteKeyword("abstract");
                            formatter.Write(" ");
                        }
                        else
                        {
                            formatter.WriteKeyword("virtual");
                            formatter.Write(" ");
                        }
                    }
                }
            }

            private void WriteParameterDeclaration(IParameterDeclaration value, IFormatter formatter, ILanguageWriterConfiguration configuration)
            {
                if ((configuration != null) && (configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
                {
                    // [Out] or [In] Or [In,Out]
                    //this.WriteCustomAttributeList(value, formatter);
                    formatter.Write(" ");
                }

                IType parameterType = value.ParameterType;

                IReferenceType referenceType = parameterType as IReferenceType;


                if ((value.Name != null) && value.Name.Length > 0)
                {
                    // Check for Reserved keywords
                    this.WriteDeclaration(value.Name, formatter);
                }
                else
                {
                    formatter.Write("A");
                    if (parameterType != null)
                    {
                        this.WriteType(parameterType, formatter);
                    }
                }


                IExpression defaultValue = this.GetDefaultParameterValue(value);
                if (defaultValue != null)
                {
                    formatter.Write(" := ");
                    this.WriteExpression(defaultValue, formatter);
                }

                if (referenceType != null)
                {
                    formatter.Write(" ");
                    formatter.WriteKeyword("ref");
                    formatter.Write(" ");
                }
                else
                {
                    formatter.Write(" ");
                    formatter.WriteKeyword("as");
                    formatter.Write(" ");
                }

                if (parameterType != null)
                {
                    this.WriteType(parameterType, formatter);
                }
                else
                {
                    formatter.Write("...");
                }


            }

            private void WriteParameterDeclarationList(IParameterDeclarationCollection parameters, IFormatter formatter, ILanguageWriterConfiguration configuration)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (i != 0)
                        formatter.Write(", ");
                    this.WriteParameterDeclaration(parameters[i], formatter, configuration);
                }
            }

            private void WriteCustomAttribute(ICustomAttribute customAttribute, IFormatter formatter)
            {
                ITypeReference type = (customAttribute.Constructor.DeclaringType as ITypeReference);
                string name = type.Name;

                if (name.EndsWith("Attribute"))
                {
                    name = name.Substring(0, name.Length - 9);
                }

                this.WriteReference(name, formatter, this.GetMethodReferenceDescription(customAttribute.Constructor), customAttribute.Constructor);

                IExpressionCollection expression = customAttribute.Arguments;
                if (expression.Count != 0)
                {
                    formatter.Write("(");
                    for (int i = 0; i < expression.Count; i++)
                    {
                        if (i != 0)
                        {
                            formatter.Write(", ");
                        }

                        this.WriteExpression(expression[i], formatter);
                    }

                    formatter.Write(")");
                }
            }

            private void WriteCustomAttributeList(ICustomAttributeProvider provider, IFormatter formatter)
            {
                // Attribute List
                // For eg, in AssemblyInfo
                //
                ArrayList attributes = new ArrayList();
                for (int i = 0; i < provider.Attributes.Count; i++)
                {
                    ICustomAttribute attribute = provider.Attributes[i];
                    if (IsType(attribute.Constructor.DeclaringType, "System.Runtime.InteropServices", "DefaultParameterValueAttribute", "System"))
                    {
                        continue;
                    }

                    attributes.Add(attribute);
                }

                if (attributes.Count > 0)
                {
                    string prefix = null;

                    IAssembly assembly = provider as IAssembly;
                    if (assembly != null)
                    {
                        prefix = "assembly:";
                    }

                    IModule module = provider as IModule;
                    if (module != null)
                    {
                        prefix = "module:";
                    }

                    IMethodReturnType methodReturnType = provider as IMethodReturnType;
                    if (methodReturnType != null)
                    {
                        prefix = "return:";
                    }

                    if ((assembly != null) || (module != null))
                    {
                        for (int i = 0; i < attributes.Count; i++)
                        {
                            ICustomAttribute attribute = (ICustomAttribute)attributes[i];
                            formatter.Write("[");
                            formatter.WriteKeyword(prefix);
                            formatter.Write(" ");
                            this.WriteCustomAttribute(attribute, formatter);
                            formatter.Write("]");
                            // In Vulcan, we don't end lines with a semi-colon !
                            //formatter.Write(";");

                            if (i != (attributes.Count - 1))
                            {
                                formatter.WriteLine();
                            }
                        }
                    }
                    else
                    {
                        formatter.Write("[");
                        if (prefix != null)
                        {
                            formatter.WriteKeyword(prefix);
                            formatter.Write(" ");
                        }

                        for (int i = 0; i < attributes.Count; i++)
                        {
                            if (i != 0)
                            {
                                formatter.Write(", ");
                            }

                            ICustomAttribute attribute = (ICustomAttribute)attributes[i];
                            this.WriteCustomAttribute(attribute, formatter);
                        }

                        formatter.Write("]");
                        // In Vulcan, we end the line with a semi-colon
                        formatter.Write(";");
                    }
                }
            }

            private void WriteGenericArgumentList(ITypeCollection parameters, IFormatter formatter)
            {
                if (parameters.Count > 0)
                {
                    formatter.Write("<");
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        if (i != 0)
                        {
                            formatter.Write("; ");
                        }

                        this.WriteType(parameters[i], formatter);
                    }

                    formatter.Write(">");
                }
            }

            private void WriteGenericParameterConstraint(IType value, IFormatter formatter)
            {
                IDefaultConstructorConstraint defaultConstructorConstraint = value as IDefaultConstructorConstraint;
                if (defaultConstructorConstraint != null)
                {
                    formatter.WriteKeyword("new");
                    formatter.Write("()");
                    return;
                }

                IReferenceTypeConstraint referenceTypeConstraint = value as IReferenceTypeConstraint;
                if (referenceTypeConstraint != null)
                {
                    formatter.WriteKeyword("class");
                    return;
                }

                IValueTypeConstraint valueTypeConstraint = value as IValueTypeConstraint;
                if (valueTypeConstraint != null)
                {
                    formatter.WriteKeyword("structure");
                    return;
                }

                this.WriteType(value, formatter);
            }

            private void WriteGenericParameterConstraintList(IGenericArgumentProvider provider, IFormatter formatter)
            {
                ITypeCollection genericArguments = provider.GenericArguments;
                if (genericArguments.Count > 0)
                {
                    for (int i = 0; i < genericArguments.Count; i++)
                    {
                        IGenericParameter parameter = genericArguments[i] as IGenericParameter;
                        if ((parameter != null) && (parameter.Constraints.Count > 0))
                        {
                            formatter.Write(" ");
                            formatter.WriteKeyword("where");
                            formatter.Write(" ");
                            formatter.Write(parameter.Name);
                            formatter.Write(":");
                            formatter.Write(" ");

                            for (int j = 0; j < parameter.Constraints.Count; j++)
                            {
                                if (j != 0)
                                {
                                    formatter.Write(", ");
                                }

                                IType constraint = (IType)parameter.Constraints[j];
                                this.WriteGenericParameterConstraint(constraint, formatter);
                            }
                        }
                    }
                }
            }

            #region Expression

            public void WriteExpression(IExpression value)
            {
                this.WriteExpression(value, formatter);
            }

            private void WriteExpression(IExpression value, IFormatter formatter)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value is ILiteralExpression)
                {
                    this.WriteLiteralExpression(value as ILiteralExpression, formatter);
                    return;
                }

                if (value is IAssignExpression)
                {
                    this.WriteAssignExpression(value as IAssignExpression, formatter);
                    return;
                }

                if (value is ITypeOfExpression)
                {
                    this.WriteTypeOfExpression(value as ITypeOfExpression, formatter);
                    return;
                }

                if (value is IFieldOfExpression)
                {
                    this.WriteFieldOfExpression(value as IFieldOfExpression, formatter);
                    return;
                }

                if (value is IMethodOfExpression)
                {
                    this.WriteMethodOfExpression(value as IMethodOfExpression, formatter);
                    return;
                }

                if (value is IMemberInitializerExpression)
                {
                    this.WriteMemberInitializerExpression(value as IMemberInitializerExpression, formatter);
                    return;
                }

                if (value is ITypeReferenceExpression)
                {
                    this.WriteTypeReferenceExpression(value as ITypeReferenceExpression, formatter);
                    return;
                }

                if (value is IFieldReferenceExpression)
                {
                    this.WriteFieldReferenceExpression(value as IFieldReferenceExpression, formatter);
                    return;
                }

                if (value is IEventReferenceExpression)
                {
                    this.WriteEventReferenceExpression(value as IEventReferenceExpression, formatter);
                    return;
                }

                if (value is IMethodReferenceExpression)
                {
                    this.WriteMethodReferenceExpression(value as IMethodReferenceExpression, formatter);
                    return;
                }

                if (value is IArgumentListExpression)
                {
                    this.WriteArgumentListExpression(value as IArgumentListExpression, formatter);
                    return;
                }

                if (value is IStackAllocateExpression)
                {
                    this.WriteStackAllocateExpression(value as IStackAllocateExpression, formatter);
                    return;
                }

                if (value is IPropertyReferenceExpression)
                {
                    this.WritePropertyReferenceExpression(value as IPropertyReferenceExpression, formatter);
                    return;
                }

                if (value is IArrayCreateExpression)
                {
                    this.WriteArrayCreateExpression(value as IArrayCreateExpression, formatter);
                    return;
                }

                if (value is IBlockExpression)
                {
                    this.WriteBlockExpression(value as IBlockExpression, formatter);
                    return;
                }

                if (value is IBaseReferenceExpression)
                {
                    this.WriteBaseReferenceExpression(value as IBaseReferenceExpression, formatter);
                    return;
                }

                if (value is IUnaryExpression)
                {
                    this.WriteUnaryExpression(value as IUnaryExpression, formatter);
                    return;
                }

                if (value is IBinaryExpression)
                {
                    this.WriteBinaryExpression(value as IBinaryExpression, formatter);
                    return;
                }

                if (value is ITryCastExpression)
                {
                    this.WriteTryCastExpression(value as ITryCastExpression, formatter);
                    return;
                }

                if (value is ICanCastExpression)
                {
                    this.WriteCanCastExpression(value as ICanCastExpression, formatter);
                    return;
                }

                if (value is ICastExpression)
                {
                    this.WriteCastExpression(value as ICastExpression, formatter);
                    return;
                }

                if (value is IConditionExpression)
                {
                    this.WriteConditionExpression(value as IConditionExpression, formatter);
                    return;
                }

                if (value is INullCoalescingExpression)
                {
                    this.WriteNullCoalescingExpression(value as INullCoalescingExpression, formatter);
                    return;
                }

                if (value is IDelegateCreateExpression)
                {
                    this.WriteDelegateCreateExpression(value as IDelegateCreateExpression, formatter);
                    return;
                }

                if (value is IAnonymousMethodExpression)
                {
                    this.WriteAnonymousMethodExpression(value as IAnonymousMethodExpression, formatter);
                    return;
                }

                if (value is IArgumentReferenceExpression)
                {
                    this.WriteArgumentReferenceExpression(value as IArgumentReferenceExpression, formatter);
                    return;
                }

                if (value is IVariableDeclarationExpression)
                {
                    this.WriteVariableDeclarationExpression(value as IVariableDeclarationExpression, formatter);
                    return;
                }

                if (value is IVariableReferenceExpression)
                {
                    this.WriteVariableReferenceExpression(value as IVariableReferenceExpression, formatter);
                    return;
                }

                if (value is IPropertyIndexerExpression)
                {
                    this.WritePropertyIndexerExpression(value as IPropertyIndexerExpression, formatter);
                    return;
                }

                if (value is IArrayIndexerExpression)
                {
                    this.WriteArrayIndexerExpression(value as IArrayIndexerExpression, formatter);
                    return;
                }

                if (value is IMethodInvokeExpression)
                {
                    this.WriteMethodInvokeExpression(value as IMethodInvokeExpression, formatter);
                    return;
                }

                if (value is IDelegateInvokeExpression)
                {
                    this.WriteDelegateInvokeExpression(value as IDelegateInvokeExpression, formatter);
                    return;
                }

                if (value is IObjectCreateExpression)
                {
                    this.WriteObjectCreateExpression(value as IObjectCreateExpression, formatter);
                    return;
                }

                if (value is IThisReferenceExpression)
                {
                    this.WriteThisReferenceExpression(value as IThisReferenceExpression, formatter);
                    return;
                }

                if (value is IAddressOfExpression)
                {
                    this.WriteAddressOfExpression(value as IAddressOfExpression, formatter);
                    return;
                }

                if (value is IAddressReferenceExpression)
                {
                    this.WriteAddressReferenceExpression(value as IAddressReferenceExpression, formatter);
                    return;
                }

                if (value is IAddressOutExpression)
                {
                    this.WriteAddressOutExpression(value as IAddressOutExpression, formatter);
                    return;
                }

                if (value is IAddressDereferenceExpression)
                {
                    this.WriteAddressDereferenceExpression(value as IAddressDereferenceExpression, formatter);
                    return;
                }

                if (value is ISizeOfExpression)
                {
                    this.WriteSizeOfExpression(value as ISizeOfExpression, formatter);
                    return;
                }

                if (value is ITypeOfTypedReferenceExpression)
                {
                    this.WriteTypeOfTypedReferenceExpression(value as ITypeOfTypedReferenceExpression, formatter);
                    return;
                }

                if (value is IValueOfTypedReferenceExpression)
                {
                    this.WriteValueOfTypedReferenceExpression(value as IValueOfTypedReferenceExpression, formatter);
                    return;
                }

                if (value is ITypedReferenceCreateExpression)
                {
                    this.WriteTypedReferenceCreateExpression(value as ITypedReferenceCreateExpression, formatter);
                    return;
                }

                if (value is IGenericDefaultExpression)
                {
                    this.WriteGenericDefaultExpression(value as IGenericDefaultExpression, formatter);
                    return;
                }

                if (value is IQueryExpression)
                {
                    this.WriteQueryExpression(value as IQueryExpression, formatter);
                    return;
                }

                if (value is ILambdaExpression)
                {
                    this.WriteLambdaExpression(value as ILambdaExpression, formatter);
                    return;
                }

                if (value is ISnippetExpression)
                {
                    this.WriteSnippetExpression(value as ISnippetExpression, formatter);
                    return;
                }

                throw new ArgumentException("Invalid expression type.", "value");
            }

            private void WriteExpressionList(IExpressionCollection expressions, IFormatter formatter)
            {
                // Indent++;
                for (int i = 0; i < expressions.Count; i++)
                {
                    if (i != 0)
                    {
                        formatter.Write(", ");
                    }

                    this.WriteExpression(expressions[i], formatter);
                }
                // Indent--;
            }

            private void WriteGenericDefaultExpression(IGenericDefaultExpression value, IFormatter formatter)
            {
                formatter.WriteKeyword("default");
                formatter.Write("(");
                this.WriteType(value.GenericArgument, formatter);
                formatter.Write(")");
            }

            private void WriteTypeOfTypedReferenceExpression(ITypeOfTypedReferenceExpression value, IFormatter formatter)
            {
                formatter.WriteKeyword("__reftype");
                formatter.Write("(");
                this.WriteExpression(value.Expression, formatter);
                formatter.Write(")");
            }

            private void WriteValueOfTypedReferenceExpression(IValueOfTypedReferenceExpression value, IFormatter formatter)
            {
                formatter.WriteKeyword("__refvalue");
                formatter.Write("(");
                this.WriteExpression(value.Expression, formatter);
                formatter.Write(")");
            }

            private void WriteTypedReferenceCreateExpression(ITypedReferenceCreateExpression value, IFormatter formatter)
            {
                formatter.WriteKeyword("__makeref");
                formatter.Write("(");
                this.WriteExpression(value.Expression, formatter);
                formatter.Write(")");
            }

            private void WriteMemberInitializerExpression(IMemberInitializerExpression value, IFormatter formatter)
            {
                this.WriteMemberReference(value.Member, formatter);
                formatter.Write(":=");
                this.WriteExpression(value.Value, formatter);
            }

            private void WriteMemberReference(IMemberReference memberReference, IFormatter formatter)
            {
                IFieldReference fieldReference = memberReference as IFieldReference;
                if (fieldReference != null)
                {
                    this.WriteFieldReference(fieldReference, formatter);
                }

                IMethodReference methodReference = memberReference as IMethodReference;
                if (methodReference != null)
                {
                    this.WriteMethodReference(methodReference, formatter);
                }

                IPropertyReference propertyReference = memberReference as IPropertyReference;
                if (propertyReference != null)
                {
                    this.WritePropertyReference(propertyReference, formatter);
                }

                IEventReference eventReference = memberReference as IEventReference;
                if (eventReference != null)
                {
                    this.WriteEventReference(eventReference, formatter);
                }
            }

            private void WriteTargetExpression(IExpression expression, IFormatter formatter)
            {
                this.WriteExpression(expression, formatter);
            }

            private void WriteTypeOfExpression(ITypeOfExpression expression, IFormatter formatter)
            {
                // Switch to fully Qualified names
                bool TempFlag = this.FullyQualifiedTypes;
                this.FullyQualifiedTypes = true;
                //
                formatter.WriteKeyword("typeof");
                formatter.Write("(");
                this.WriteType(expression.Type, formatter);
                formatter.Write(")");
                // Restore state
                this.FullyQualifiedTypes = TempFlag;
            }

            private void WriteFieldOfExpression(IFieldOfExpression value, IFormatter formatter)
            {
                formatter.WriteKeyword("fieldof");
                formatter.Write("(");
                this.WriteType(value.Field.DeclaringType, formatter);
                formatter.Write(".");
                formatter.WriteReference(value.Field.Name, this.GetFieldReferenceDescription(value.Field), value.Field);
                formatter.Write(")");
            }

            private void WriteMethodOfExpression(IMethodOfExpression value, IFormatter formatter)
            {
                formatter.WriteKeyword("methodof");
                formatter.Write("(");

                this.WriteType(value.Method.DeclaringType, formatter);
                formatter.Write(".");
                formatter.WriteReference(value.Method.Name, this.GetMethodReferenceDescription(value.Method), value.Method);

                if (value.Type != null)
                {
                    formatter.Write(", ");
                    this.WriteType(value.Type, formatter);
                }

                formatter.Write(")");
            }

            private void WriteArrayElementType(IType type, IFormatter formatter)
            {
                IArrayType arrayType = type as IArrayType;
                if (arrayType != null)
                {
                    this.WriteArrayElementType(arrayType.ElementType, formatter);
                }
                else
                {
                    this.WriteType(type, formatter);
                }
            }

            private void WriteArrayCreateExpression(IArrayCreateExpression expression, IFormatter formatter)
            {
                if (expression.Initializer == null)
                {
                    this.WriteArrayElementType(expression.Type, formatter);
                    formatter.Write("[]");
                    formatter.Write("{");
                    this.WriteExpressionList(expression.Dimensions, formatter);
                    formatter.Write("}");

                    // 	this.WriteArrayDimension(expression.Type, formatter);
                }
                else
                {
                    formatter.Write("<");
                    this.WriteArrayElementType(expression.Type, formatter);
                    formatter.Write(">");
                    formatter.Write("{");
                    this.WriteExpression(expression.Initializer, formatter);
                    formatter.Write("}");
                }

                //formatter.Write(")");
            }

            private void WriteBlockExpression(IBlockExpression expression, IFormatter formatter)
            {
                //formatter.Write(" ( ");
                //
                if (expression.Expressions.Count > 16)
                {
                    formatter.WriteLine();
                    formatter.WriteIndent();
                }

                for (int i = 0; i < expression.Expressions.Count; i++)
                {
                    if (i != 0)
                    {
                        formatter.Write(", ");

                        if ((i % 16) == 0)
                        {
                            formatter.WriteLine();
                        }
                    }

                    this.WriteExpression(expression.Expressions[i], formatter);
                }

                if (expression.Expressions.Count > 16)
                {
                    formatter.WriteOutdent();
                    formatter.WriteLine();
                }

                //formatter.Write(" ) ");
            }

            private void WriteBaseReferenceExpression(IBaseReferenceExpression expression, IFormatter formatter)
            {
                // Base == Super in Vulcan
                formatter.WriteKeyword("Super");
                //formatter.Write(":");
            }

            private void WriteTryCastExpression(ITryCastExpression expression, IFormatter formatter)
            {
                //
                formatter.Write("(");
                this.WriteType(expression.TargetType, formatter);
                formatter.Write(")"); 
                formatter.Write("(");
                this.WriteExpression(expression.Expression, formatter);
                formatter.Write(")");
            }

            private void WriteCanCastExpression(ICanCastExpression expression, IFormatter formatter)
            {
                // For eg: if ( value is string )
                /*
                formatter.Write("(");
                this.WriteExpression(expression.Expression, formatter);
                formatter.Write(" ");
                formatter.WriteKeyword("is");
                formatter.Write(" ");
                this.WriteType(expression.TargetType, formatter);
                formatter.Write(")");
                */
                // Let's try to emulate with a TypeOf construction
                // Switch to fully Qualified names
                bool TempFlag = this.FullyQualifiedTypes;
                this.FullyQualifiedTypes = true;
                //
                formatter.Write("(");
                formatter.WriteKeyword("System");
                formatter.Write(".");
                formatter.WriteKeyword("Type");
                formatter.Write(".");
                formatter.WriteKeyword("Equals");
                formatter.Write("(");
                this.WriteExpression(expression.Expression, formatter);
                formatter.Write(",");
                this.WriteType(expression.TargetType, formatter);
                formatter.Write(")");
                formatter.Write(")");
                // Restore state
                this.FullyQualifiedTypes = TempFlag;
            }

            private void WriteCastExpression(ICastExpression expression, IFormatter formatter)
            {
                // (int)MyString
                //
                // (int)
                formatter.Write("(");
                this.WriteType(expression.TargetType, formatter);
                formatter.Write(")");
                // MyString
                this.WriteExpression(expression.Expression, formatter);
                formatter.Write(" ");
            }

            private void WriteConditionExpression(IConditionExpression expression, IFormatter formatter)
            {
                // Use IIf in Vulcan
                formatter.WriteKeyword("IIF");
                formatter.Write("(");
                this.WriteExpression(expression.Condition, formatter);
                formatter.Write(",");
                this.WriteExpression(expression.Then, formatter);
                formatter.Write(",");
                this.WriteExpression(expression.Else, formatter);
                formatter.Write(")");
            }

            private void WriteNullCoalescingExpression(INullCoalescingExpression value, IFormatter formatter)
            {
                /*
                                // Use IIf in Vulcan
                                formatter.WriteKeyword("IIF");
                                formatter.Write("(");
                                this.WriteExpression(value.Condition, formatter);
                                formatter.Write(",");
                                this.WriteExpression(value.Condition, formatter);
                                formatter.Write(",");
                                this.WriteExpression(value.Expression, formatter);
                                formatter.Write(")");
                */
                formatter.WriteComment(" {pseudo} ");
                formatter.Write("(");
                formatter.WriteKeyword("if");
                formatter.Write(" ");
                this.WriteExpression(value.Condition, formatter);
                formatter.WriteKeyword(" not ");
                formatter.WriteKeyword(" nil ");
                formatter.Write(" ");
                formatter.WriteKeyword("then");
                formatter.Write(" ");
                this.WriteExpression(value.Condition, formatter);
                formatter.Write(" ");
                formatter.WriteKeyword("else");
                formatter.Write(" ");
                this.WriteExpression(value.Expression, formatter);
                formatter.Write(")");
            }


            private void WriteDelegateCreateExpression(IDelegateCreateExpression expression, IFormatter formatter)
            {
                IExpression ObjectOrClass;
                string handlerName, handlerSP;
                //
                ObjectOrClass = expression.Target;
                // eg: System.EventHandler{ SELF, @button1_Click() }
                handlerName = expression.DelegateType.Name;
                handlerSP = expression.DelegateType.Namespace;
                //
                this.WriteTypeReference(expression.DelegateType, formatter);
                formatter.Write("{");
                formatter.Write(" ");
                // Static Delegate are referenced on Classes
                if (ObjectOrClass is ITypeReferenceExpression)
                {
                    formatter.WriteKeyword("NULL");
                }
                else
                {
                    this.WriteTargetExpression(expression.Target, formatter);
                }
                formatter.Write(",");
                formatter.Write(" ");
                formatter.Write("@");
                // If you have SELF you "certainly" are in an EventHandler, 
                // so we need to remove this call : so generate only if not SELF
                if (!(expression.Target is IThisReferenceExpression))
                {
                    this.WriteTargetExpression(expression.Target, formatter);
                    formatter.Write(":");
                }
                this.WriteMethodReference(expression.Method, formatter); // TODO Escape = true
                formatter.Write("()");
                formatter.Write(" ");
                formatter.Write("}");
            }

            private void WriteAnonymousMethodExpression(IAnonymousMethodExpression value, IFormatter formatter)
            {
                bool parameters = false;

                for (int i = 0; i < value.Parameters.Count; i++)
                {
                    if ((value.Parameters[i].Name != null) && (value.Parameters[i].Name.Length > 0))
                    {
                        parameters = true;
                    }
                }

                formatter.WriteKeyword("delegate");
                formatter.Write(" ");

                if (parameters)
                {
                    formatter.Write("(");
                    this.WriteParameterDeclarationList(value.Parameters, formatter, this.configuration);
                    formatter.Write(")");
                    formatter.Write(" ");
                }

                formatter.WriteLine();
                formatter.WriteKeyword("begin");
                formatter.WriteLine();
                formatter.WriteIndent();
                this.WriteBlockStatement(value.Body, formatter);
                formatter.WriteOutdent();
                formatter.WriteLine();
                formatter.WriteKeyword("end");
            }

            private void WriteTypeReferenceExpression(ITypeReferenceExpression expression, IFormatter formatter)
            {
                // We are here if we are using a TypeReference in an expression, for eg :
                // x = Int32.Parse( ... );
                bool TempFlag;
                TempFlag = this.KeepSystemType;
                //
                this.KeepSystemType = true;
                this.WriteTypeReference(expression.Type, formatter);
                //
                this.KeepSystemType = TempFlag;

            }

            private void WriteFieldReferenceExpression(IFieldReferenceExpression expression, IFormatter formatter)
            { // TODO bool escape = true;
                // eg: SELF:MyInstanceVar
                //
                bool TempFlag = this.KeepKeyword;
                //
                if (expression.Target != null)
                {
                    this.WriteTargetExpression(expression.Target, formatter);
                    if (!(expression.Target is IBaseReferenceExpression)) // no dot for "inherited"
                    {
                        if (!(expression.Target is ITypeReferenceExpression))
                        {
                            // eg: Color.blue:ToString()
                            formatter.Write(":");
                        }
                        else
                        {
                            // eg: Application.Run
                            formatter.Write(".");
                        }
                    }
                    else
                        formatter.Write(":");
                    // Prefixed, so
                    this.KeepKeyword = true;
                }
                this.WriteFieldReference(expression.Field, formatter);
                //
                this.KeepKeyword = TempFlag;
            }

            private void WriteArgumentReferenceExpression(IArgumentReferenceExpression expression, IFormatter formatter)
            {
                // TODO Escape name?
                // TODO Should there be a Resovle() mechanism

                TextFormatter textFormatter = new TextFormatter();
                this.WriteParameterDeclaration(expression.Parameter.Resolve(), textFormatter, null);
                textFormatter.Write(" // Parameter");
                if (expression.Parameter.Name != null)
                {
                    this.WriteReference(expression.Parameter.Name, formatter, textFormatter.ToString(), null);
                }
            }

            private void WriteArgumentListExpression(IArgumentListExpression expression, IFormatter formatter)
            {
                formatter.WriteKeyword("__arglist");
            }

            private void WriteVariableReferenceExpression(IVariableReferenceExpression expression, IFormatter formatter)
            {
                this.WriteVariableReference(expression.Variable, formatter);
            }

            private void WriteVariableReference(IVariableReference value, IFormatter formatter)
            {
                string name;
                //
                IVariableDeclaration variableDeclaration = value.Resolve();

                TextFormatter textFormatter = new TextFormatter();
                this.WriteVariableDeclaration(variableDeclaration, textFormatter);
                textFormatter.Write(" // Local Variable");
                // Variable auto-generated by the compiler have a $ in name
                name = variableDeclaration.Name.Replace("$", "_");
                // Variable name is a reserved keyword ?
                if ((Array.IndexOf(this.keywords, name.ToLower()) != -1) && (!this.KeepKeyword))
                {
                    name = "@@" + name;
                }

                formatter.WriteReference(name, textFormatter.ToString(), null);
            }

            private void WritePropertyIndexerExpression(IPropertyIndexerExpression expression, IFormatter formatter)
            {
                this.WriteTargetExpression(expression.Target, formatter);
                formatter.Write("[");

                bool first = true;

                foreach (IExpression index in expression.Indices)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        formatter.Write(", ");
                    }

                    this.WriteExpression(index, formatter);
                }

                formatter.Write("]");
            }

            private void WriteArrayIndexerExpression(IArrayIndexerExpression expression, IFormatter formatter)
            {
                this.WriteTargetExpression(expression.Target, formatter);
                formatter.Write("[");

                for (int i = 0; i < expression.Indices.Count; i++)
                {
                    if (i != 0)
                    {
                        formatter.Write(", ");
                    }

                    this.WriteExpression(expression.Indices[i], formatter);
                    // In Vulcan, Array are one-based per default
                    if (this.ArrayIsOneBased)
                        formatter.Write(" + 1");
                }

                formatter.Write("]");
            }

            private void WriteMethodInvokeExpression(IMethodInvokeExpression expression, IFormatter formatter)
            {
                IMethodReferenceExpression methodReferenceExpression = expression.Method as IMethodReferenceExpression;
                if (methodReferenceExpression != null)
                    this.WriteMethodReferenceExpression(methodReferenceExpression, formatter);
                else
                {
                    formatter.Write("(");
                    this.WriteExpression(expression.Method, formatter);
                    formatter.Write("^");
                    formatter.Write(")");
                }
                formatter.Write("(");
                if (expression.Arguments.Count > 0)
                {
                    this.WriteExpressionList(expression.Arguments, formatter);
                }
                formatter.Write(")");
            }


            private void WriteMethodReferenceExpression(IMethodReferenceExpression expression, IFormatter formatter)
            { // TODO bool escape = true;
                if (expression.Target != null)
                { // TODO escape = false;
                    if (expression.Target is IBinaryExpression)
                    {
                        formatter.Write("(");
                        this.WriteExpression(expression.Target, formatter);
                        formatter.Write(")");
                    }
                    else
                        // Fully qualified name : Namespace + MethodName
                        this.WriteTargetExpression(expression.Target, formatter);

                    if (!(expression.Target is IBaseReferenceExpression)) // no dot for "inherited"
                    {
                        if (!(expression.Target is ITypeReferenceExpression))
                        {
                            // eg: Color.blue:ToString()
                            formatter.Write(":");
                        }
                        else
                        {
                            // eg: Application.Run
                            formatter.Write(".");
                        }
                    }
                    else
                        formatter.Write(":");

                }
                this.WriteMethodReference(expression.Method, formatter);
            }

            private void WriteEventReferenceExpression(IEventReferenceExpression expression, IFormatter formatter)
            { // TODO bool escape = true;
                // eg: SELF:MyEvent
                // SELF:Button1:Click
                //
                if (expression.Target != null)
                { // TODO escape = false;
                    this.WriteTargetExpression(expression.Target, formatter);
                    if (!(expression.Target is IBaseReferenceExpression)) // no dot for "inherited"
                    {
                        if (!(expression.Target is ITypeReferenceExpression))
                        {
                            // eg: Color.blue:ToString()
                            formatter.Write(":");
                        }
                        else
                        {
                            // eg: Application.Run
                            formatter.Write(".");
                        }
                    }
                    else
                        formatter.Write(":");

                }
                this.WriteEventReference(expression.Event, formatter);
            }

            private void WriteDelegateInvokeExpression(IDelegateInvokeExpression expression, IFormatter formatter)
            {
                if (expression.Target != null)
                {
                    this.WriteTargetExpression(expression.Target, formatter);
                }

                formatter.Write("(");
                this.WriteExpressionList(expression.Arguments, formatter);
                formatter.Write(")");
            }

            private void WriteObjectCreateExpression(IObjectCreateExpression value, IFormatter formatter)
            {
                if (value.Constructor != null)
                {
                    this.WriteTypeReference((ITypeReference)value.Type, formatter, this.GetMethodReferenceDescription(value.Constructor), value.Constructor);
                }
                else
                {
                    this.WriteType(value.Type, formatter);
                }

                formatter.Write("{");

                if (value.Arguments.Count > 0)
                {
                    this.WriteExpressionList(value.Arguments, formatter);
                }
                formatter.Write("}");

                IBlockExpression initializer = value.Initializer as IBlockExpression;
                if ((initializer != null) && (initializer.Expressions.Count > 0))
                {
                    formatter.Write(" ");
                    this.WriteExpression(initializer, formatter);
                }
            }

            private void WritePropertyReferenceExpression(IPropertyReferenceExpression expression, IFormatter formatter)
            { // TODO bool escape = true;
                // eg: Self:Text
                //
                if (expression.Target != null)
                { // TODO escape = false;
                    this.WriteTargetExpression(expression.Target, formatter);
                    if (!(expression.Target is IBaseReferenceExpression)) // no dot for "inherited"
                    {
                        if (!(expression.Target is ITypeReferenceExpression))
                        {
                            // eg: Color.Blue:IsEmpty
                            formatter.Write(":");
                        }
                        else
                        {
                            //
                            formatter.Write(".");
                        }
                    }
                    else
                        formatter.Write(":");
                }
                this.WritePropertyReference(expression.Property, formatter);
            }

            private void WriteThisReferenceExpression(IThisReferenceExpression expression, IFormatter formatter)
            {
                formatter.WriteKeyword("self");
            }

            private void WriteAddressOfExpression(IAddressOfExpression expression, IFormatter formatter)
            {
                formatter.Write("@(");
                this.WriteExpression(expression.Expression, formatter);
                formatter.Write(")");
            }

            private void WriteAddressReferenceExpression(IAddressReferenceExpression expression, IFormatter formatter)
            {
                formatter.Write("@(");
                this.WriteExpression(expression.Expression, formatter);
                formatter.Write(")");
            }

            private void WriteAddressOutExpression(IAddressOutExpression expression, IFormatter formatter)
            {
                formatter.Write("@(");
                this.WriteExpression(expression.Expression, formatter);
                formatter.Write(")");
            }

            private void WriteAddressDereferenceExpression(IAddressDereferenceExpression expression, IFormatter formatter)
            {
                IAddressOfExpression addressOf = expression.Expression as IAddressOfExpression;
                if (addressOf != null)
                {
                    this.WriteExpression(addressOf.Expression, formatter);
                }
                else
                {
                    // formatter.Write("*(");
                    this.WriteExpression(expression.Expression, formatter);
                    // formatter.Write(")");
                }
            }

            private void WriteSizeOfExpression(ISizeOfExpression expression, IFormatter formatter)
            {
                formatter.WriteKeyword("sizeof");
                formatter.Write("(");
                this.WriteType(expression.Type, formatter);
                formatter.Write(")");
            }

            private void WriteStackAllocateExpression(IStackAllocateExpression expression, IFormatter formatter)
            {
                formatter.WriteKeyword("stackalloc");
                formatter.Write(" ");
                this.WriteType(expression.Type, formatter);
                formatter.Write("[");
                this.WriteExpression(expression.Expression, formatter);
                formatter.Write("]");
            }

            private void WriteLambdaExpression(ILambdaExpression value, IFormatter formatter)
            {
                if (value.Parameters.Count > 1)
                {
                    formatter.Write("(");
                }

                for (int i = 0; i < value.Parameters.Count; i++)
                {
                    if (i != 0)
                    {
                        formatter.Write(", ");
                    }

                    // this.WriteVariableIdentifier(value.Parameters[i].Variable.Identifier, formatter);
                    this.WriteDeclaration(value.Parameters[i].Name, formatter);
                }

                if (value.Parameters.Count > 1)
                {
                    formatter.Write(")");
                }

                formatter.Write(" ");

                formatter.Write("=>");

                formatter.Write(" ");

                this.WriteExpression(value.Body, formatter);

            }

            private void WriteQueryExpression(IQueryExpression value, IFormatter formatter)
            {
                formatter.Write("(");

                this.WriteFromClause(value.From, formatter);

                if ((value.Body.Clauses.Count > 0) || (value.Body.Continuation != null))
                {
                    formatter.WriteLine();
                    formatter.WriteIndent();
                }
                else
                {
                    formatter.Write(" ");
                }

                this.WriteQueryBody(value.Body, formatter);

                formatter.Write(")");

                if ((value.Body.Clauses.Count > 0) || (value.Body.Continuation != null))
                {
                    formatter.WriteOutdent();
                }
            }

            private void WriteQueryBody(IQueryBody value, IFormatter formatter)
            {
                // from | where | let | join | orderby
                for (int i = 0; i < value.Clauses.Count; i++)
                {
                    this.WriteQueryClause(value.Clauses[i], formatter);
                    formatter.WriteLine();
                }

                // select | group
                this.WriteQueryOperation(value.Operation, formatter);

                // into
                if (value.Continuation != null)
                {
                    formatter.Write(" ");
                    this.WriteQueryContinuation(value.Continuation, formatter);
                }
            }

            private void WriteQueryContinuation(IQueryContinuation value, IFormatter formatter)
            {
                formatter.WriteKeyword("into");
                formatter.Write(" ");
                this.WriteDeclaration(value.Variable.Name, formatter);
                formatter.WriteLine();
                this.WriteQueryBody(value.Body, formatter);
            }

            private void WriteQueryClause(IQueryClause value, IFormatter formatter)
            {
                if (value is IWhereClause)
                {
                    this.WriteWhereClause(value as IWhereClause, formatter);
                    return;
                }

                if (value is ILetClause)
                {
                    this.WriteLetClause(value as ILetClause, formatter);
                    return;
                }

                if (value is IFromClause)
                {
                    this.WriteFromClause(value as IFromClause, formatter);
                    return;
                }

                if (value is IJoinClause)
                {
                    this.WriteJoinClause(value as IJoinClause, formatter);
                    return;
                }

                if (value is IOrderClause)
                {
                    this.WriteOrderClause(value as IOrderClause, formatter);
                    return;
                }

                throw new NotSupportedException();
            }

            private void WriteQueryOperation(IQueryOperation value, IFormatter formatter)
            {
                if (value is ISelectOperation)
                {
                    this.WriteSelectOperation(value as ISelectOperation, formatter);
                    return;
                }

                if (value is IGroupOperation)
                {
                    this.WriteGroupOperation(value as IGroupOperation, formatter);
                    return;
                }

                throw new NotSupportedException();
            }

            private void WriteFromClause(IFromClause value, IFormatter formatter)
            {
                formatter.WriteKeyword("from");
                formatter.Write(" ");
                this.WriteDeclaration(value.Variable.Name, formatter);
                formatter.Write(" ");
                formatter.WriteKeyword("in");
                formatter.Write(" ");
                this.WriteExpression(value.Expression, formatter);
            }

            private void WriteWhereClause(IWhereClause value, IFormatter formatter)
            {
                formatter.WriteKeyword("where");
                formatter.Write(" ");
                this.WriteExpression(value.Expression, formatter);
            }

            private void WriteLetClause(ILetClause value, IFormatter formatter)
            {
                formatter.WriteKeyword("let");
                formatter.Write(" ");
                this.WriteDeclaration(value.Variable.Name, formatter);
                formatter.Write(" = ");
                this.WriteExpression(value.Expression, formatter);
            }

            private void WriteJoinClause(IJoinClause value, IFormatter formatter)
            {
                formatter.WriteKeyword("join");
                formatter.Write(" ");
                this.WriteDeclaration(value.Variable.Name, formatter);
                formatter.Write(" ");
                formatter.WriteKeyword("in");
                formatter.Write(" ");
                this.WriteExpression(value.In, formatter);
                formatter.Write(" ");
                formatter.WriteKeyword("on");
                formatter.Write(" ");
                this.WriteExpression(value.On, formatter);
                formatter.Write(" ");
                formatter.WriteKeyword("equals");
                formatter.Write(" ");
                this.WriteExpression(value.Equality, formatter);

                if (value.Into != null)
                {
                    formatter.Write(" ");
                    formatter.WriteKeyword("into");
                    formatter.Write(" ");
                    this.WriteDeclaration(value.Into.Name, formatter);
                }
            }

            private void WriteOrderClause(IOrderClause value, IFormatter formatter)
            {
                formatter.WriteKeyword("orderby");
                formatter.Write(" ");
                this.WriteExpression(value.Expression, formatter);

                if (value.Direction == OrderDirection.Descending)
                {
                    formatter.Write(" ");
                    formatter.WriteKeyword("descending");
                }
            }

            private void WriteSelectOperation(ISelectOperation value, IFormatter formatter)
            {
                formatter.WriteKeyword("select");
                formatter.Write(" ");
                this.WriteExpression(value.Expression, formatter);
            }

            private void WriteGroupOperation(IGroupOperation value, IFormatter formatter)
            {
                formatter.WriteKeyword("group");
                formatter.Write(" ");
                this.WriteExpression(value.Item, formatter);
                formatter.Write(" ");
                formatter.WriteKeyword("by");
                formatter.Write(" ");
                this.WriteExpression(value.Key, formatter);
            }

            private void WriteSnippetExpression(ISnippetExpression expression, IFormatter formatter)
            {
                formatter.WriteComment(expression.Value);
            }

            private void WriteUnaryExpression(IUnaryExpression expression, IFormatter formatter)
            {
                switch (expression.Operator)
                {
                    case UnaryOperator.BitwiseNot:
                        formatter.WriteKeyword("not");
                        formatter.Write(" ");
                        this.WriteExpression(expression.Expression, formatter);
                        break;

                    case UnaryOperator.BooleanNot:
                        formatter.WriteKeyword("!");
                        formatter.Write(" ");
                        this.WriteExpression(expression.Expression, formatter);
                        break;

                    case UnaryOperator.Negate:
                        formatter.Write("-");
                        this.WriteExpression(expression.Expression, formatter);
                        break;

                    case UnaryOperator.PreIncrement:
                        formatter.Write("++");
                        this.WriteExpression(expression.Expression, formatter);
                        break;

                    case UnaryOperator.PreDecrement:
                        formatter.Write("--");
                        this.WriteExpression(expression.Expression, formatter);
                        break;

                    case UnaryOperator.PostIncrement:
                        this.WriteExpression(expression.Expression, formatter);
                        formatter.Write("++");
                        break;

                    case UnaryOperator.PostDecrement:
                        this.WriteExpression(expression.Expression, formatter);
                        formatter.Write("--");
                        break;

                    default:
                        throw new NotSupportedException(expression.Operator.ToString());
                }
            }

            private void WriteBinaryExpression(IBinaryExpression expression, IFormatter formatter)
            {
                formatter.Write("(");
                this.WriteExpression(expression.Left, formatter);
                formatter.Write(" ");
                this.WriteBinaryOperator(expression.Operator, formatter);
                formatter.Write(" ");
                this.WriteExpression(expression.Right, formatter);
                formatter.Write(")");
            }

            private void WriteBinaryOperator(BinaryOperator operatorType, IFormatter formatter)
            {
                switch (operatorType)
                {
                    case BinaryOperator.Add:
                        formatter.Write("+");
                        break;

                    case BinaryOperator.Subtract:
                        formatter.Write("-");
                        break;

                    case BinaryOperator.Multiply:
                        formatter.Write("*");
                        break;

                    case BinaryOperator.Divide:
                        formatter.WriteKeyword("/");
                        break;

                    case BinaryOperator.Modulus:
                        formatter.WriteKeyword("%");
                        break;

                    case BinaryOperator.ShiftLeft:
                        formatter.WriteKeyword("shl");
                        break;

                    case BinaryOperator.ShiftRight:
                        formatter.WriteKeyword("shr");
                        break;

                    case BinaryOperator.ValueInequality:
                    case BinaryOperator.IdentityInequality:
                        formatter.Write("!=");
                        break;

                    case BinaryOperator.ValueEquality:
                    case BinaryOperator.IdentityEquality:
                        formatter.Write("==");
                        break;

                    case BinaryOperator.BitwiseOr:
                        //                        formatter.WriteKeyword("_or");
                        formatter.WriteKeyword("|");
                        break;

                    case BinaryOperator.BitwiseAnd:
                        //                        formatter.WriteKeyword("_and");
                        formatter.WriteKeyword("&");
                        break;

                    case BinaryOperator.BitwiseExclusiveOr:
                        //                        formatter.WriteKeyword("_xor");
                        formatter.WriteKeyword("~");
                        break;

                    case BinaryOperator.BooleanOr:
                        formatter.WriteKeyword(".or.");
                        break;

                    case BinaryOperator.BooleanAnd:
                        formatter.WriteKeyword(".and.");
                        break;

                    case BinaryOperator.LessThan:
                        formatter.Write("<");
                        break;

                    case BinaryOperator.LessThanOrEqual:
                        formatter.Write("<=");
                        break;

                    case BinaryOperator.GreaterThan:
                        formatter.Write(">");
                        break;

                    case BinaryOperator.GreaterThanOrEqual:
                        formatter.Write(">=");
                        break;

                    default:
                        throw new NotSupportedException(operatorType.ToString());
                }
            }

            private void WriteLiteralExpression(ILiteralExpression value, IFormatter formatter)
            {
                if (value.Value == null)
                {
                    formatter.WriteLiteral("null");
                }
                else if (value.Value is char)
                {
                    string text = new string(new char[] { (char)value.Value });
                    text = this.QuoteLiteralExpression(text, false);

                    formatter.WriteLiteral(text);
                }
                else if (value.Value is string)
                {
                    string text = (string)value.Value;
                    text = this.QuoteLiteralExpression(text, true);
                    if (text.Length == 0)
                        formatter.WriteLiteral("\"" + text + "\"");
                    else
                        formatter.WriteLiteral(text);
                    //                    formatter.WriteLiteral("\"" + text + "\"");
                }
                else if (value.Value is byte)
                {
                    this.WriteNumber((byte)value.Value, formatter);
                }
                else if (value.Value is sbyte)
                {
                    this.WriteNumber((sbyte)value.Value, formatter);
                }
                else if (value.Value is short)
                {
                    this.WriteNumber((short)value.Value, formatter);
                }
                else if (value.Value is ushort)
                {
                    this.WriteNumber((ushort)value.Value, formatter);
                }
                else if (value.Value is int)
                {
                    this.WriteNumber((int)value.Value, formatter);
                }
                else if (value.Value is uint)
                {
                    this.WriteNumber((uint)value.Value, formatter);
                }
                else if (value.Value is long)
                {
                    this.WriteNumber((long)value.Value, formatter);
                }
                else if (value.Value is ulong)
                {
                    this.WriteNumber((ulong)value.Value, formatter);
                }
                else if (value.Value is float)
                {
                    // TODO
                    formatter.WriteLiteral(((float)value.Value).ToString(CultureInfo.InvariantCulture));
                }
                else if (value.Value is double)
                {
                    // TODO
                    formatter.WriteLiteral(((double)value.Value).ToString("R", CultureInfo.InvariantCulture));
                }
                else if (value.Value is decimal)
                {
                    formatter.WriteLiteral(((decimal)value.Value).ToString(CultureInfo.InvariantCulture));
                }
                else if (value.Value is bool)
                {
                    formatter.WriteLiteral(((bool)value.Value) ? "true" : "false");
                }
                /*
                else if (expression.Value is byte[])
                {
                    formatter.WriteComment("{ ");

                    byte[] bytes = (byte[])expression.Value;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (i != 0)
                        {
                            formatter.Write(", ");
                        }

                        formatter.WriteComment("0x" + bytes[i].ToString("X2", CultureInfo.InvariantCulture));
                    }

                    formatter.WriteComment(" }");
                }
                */
                else
                {
                    throw new ArgumentException("expression");
                }
            }

            private void WriteNumber(IConvertible value, IFormatter formatter)
            {
                IFormattable formattable = (IFormattable)value;

                switch (this.GetNumberFormat(value))
                {
                    case NumberFormat.Decimal:
                        formatter.WriteLiteral(formattable.ToString(null, CultureInfo.InvariantCulture));
                        break;

                    case NumberFormat.Hexadecimal:
                        formatter.WriteLiteral("0x" + formattable.ToString("x", CultureInfo.InvariantCulture));
                        break;
                }
            }

            private NumberFormat GetNumberFormat(IConvertible value)
            {
                NumberFormat format = this.numberFormat;
                if (format == NumberFormat.Auto)
                {
                    long number = (value is ulong) ? (long)(ulong)value : value.ToInt64(CultureInfo.InvariantCulture);

                    if (number < 16)
                    {
                        return NumberFormat.Decimal;
                    }

                    if (((number % 10) == 0) && (number < 1000))
                    {
                        return NumberFormat.Decimal;
                    }

                    return NumberFormat.Hexadecimal;
                }

                return format;
            }

            private void WriteTypeReference(ITypeReference typeReference, IFormatter formatter)
            {
                this.WriteType(typeReference, formatter);
            }

            private void WriteTypeReference(ITypeReference typeReference, IFormatter formatter, string description, object target)
            {
                // Always add the NameSpace ????
                // FAB
                //string name = typeReference.Namespace + "." +  typeReference.Name;
                //string namesp = typeReference.Namespace;
                string namesp;
                try
                {
                    namesp = Helper.GetResolutionScope(typeReference);
                }
                catch (NotSupportedException e)
                {
                    // Ok, let's try something else
                    namesp = typeReference.Namespace;
                }
                //
                string name = typeReference.Name;
                /*
                if ((value.Visibility > TypeVisibility.Public) && this.ExternalizeNestedTypes )
                {
                    declaringType = value.Owner as ITypeReference;
                    if (declaringType != null)
                    {
                        valueName = declaringType.Name + "." + value.Name;
                    }
                }
                else
                    valueName = value.Name;
                 */
                // Base System Types
                if (typeReference.Namespace == "System")
                {
                    // Convert System Types
                    if (specialTypeNames.Contains(name))
                    {
                        // to Vulcan ones
                        // except if....
                        if (!this.KeepSystemType)
                        {
                            name = (string)specialTypeNames[name];
                        }
                    }
                    else
                        if (this.FullyQualifiedTypes)
                        {
                            if (!String.IsNullOrEmpty(namesp))
                            {
                                //name = Helper.GetNameWithResolutionScope(typeReference);
                                name = namesp + "." + name;
                            }
                        }
                }
                else
                {
                    if (!String.IsNullOrEmpty(namesp))
                    {
                        if (namesp.IndexOf("Vulcan") == -1)
                        {
                            if (this.FullyQualifiedTypes)
                            {
                                if ((!this.QualifySystemOnly) || ((namesp.IndexOf("System") == 0) && this.QualifySystemOnly))
                                {
                                    //name = Helper.GetNameWithResolutionScope(typeReference);
                                    name = namesp + "." + name;
                                }
                                else if (this.ExternalizeNestedTypes && (typeReference.Owner != null))
                                {
                                    ITypeReference declaringType = typeReference.Owner as ITypeReference;
                                    if (declaringType != null)
                                    {
                                        name = declaringType.Name + "." + name;
                                    }
                                }
                            }
                        }
                    }
                }
                //
                ITypeReference genericType = typeReference.GenericType;
                if (genericType != null)
                {
                    formatter.WriteReference(name, description, target);
                    //this.WriteReference(name, formatter, description, genericType);
                    this.WriteGenericArgumentList(typeReference.GenericArguments, formatter);
                }
                else
                {
                    // Write Type, that's all
                    formatter.WriteReference(name, description, target);
                    // Here we first Check if it's a reserved word
                    //this.WriteReference(name, formatter, description, target);
                }
            }

            private void WriteFieldReference(IFieldReference fieldReference, IFormatter formatter)
            {
                // TODO Escape?
                this.WriteReference(fieldReference.Name, formatter, this.GetFieldReferenceDescription(fieldReference), fieldReference);
            }

            private void WriteMethodReference(IMethodReference methodReference, IFormatter formatter)
            {
                // TODO Escape?

                IMethodReference genericMethod = methodReference.GenericMethod;
                if (genericMethod != null)
                {
                    this.WriteReference(methodReference.Name, formatter, this.GetMethodReferenceDescription(methodReference), genericMethod);
                    this.WriteGenericArgumentList(methodReference.GenericArguments, formatter);
                }
                else
                {
                    this.WriteReference(methodReference.Name, formatter, this.GetMethodReferenceDescription(methodReference), methodReference);
                }
            }


            private void WritePropertyReference(IPropertyReference propertyReference, IFormatter formatter)
            {
                // TODO Escape?
                this.WriteReference(propertyReference.Name, formatter, this.GetPropertyReferenceDescription(propertyReference), propertyReference);
            }

            private void WriteEventReference(IEventReference eventReference, IFormatter formatter)
            {
                // TODO Escape?
                this.WriteReference(eventReference.Name, formatter, this.GetEventReferenceDescription(eventReference), eventReference);
            }

            #endregion

            #region Statement

            public void WriteStatement(IStatement value)
            {
                this.WriteStatement(value, this.formatter);
            }

            private void WriteStatement(IStatement value, IFormatter formatter)
            {
                WriteStatement(value, formatter, false);
            }

            private void WriteStatement(IStatement value, IFormatter formatter, bool lastStatement)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value is IBlockStatement)
                {
                    this.WriteBlockStatement(value as IBlockStatement, formatter);
                    return;
                }

                if (value is IExpressionStatement)
                {
                    this.WriteExpressionStatement(value as IExpressionStatement, formatter);
                    return;
                }

                if (value is IGotoStatement)
                {
                    this.WriteGotoStatement(value as IGotoStatement, formatter);
                    return;
                }

                if (value is ILabeledStatement)
                {
                    this.WriteLabeledStatement(value as ILabeledStatement, formatter);
                    return;
                }

                if (value is IConditionStatement)
                {
                    this.WriteConditionStatement(value as IConditionStatement, formatter);
                    return;
                }

                if (value is IMethodReturnStatement)
                {
                    this.WriteMethodReturnStatement(value as IMethodReturnStatement, formatter, lastStatement);
                    return;
                }

                if (value is IForStatement)
                {
                    this.WriteForStatement(value as IForStatement, formatter);
                    return;
                }

                if (value is IForEachStatement)
                {
                    this.WriteForEachStatement(value as IForEachStatement, formatter);
                    return;
                }

                if (value is IUsingStatement)
                {
                    this.WriteUsingStatement(value as IUsingStatement, formatter);
                    return;
                }

                if (value is IFixedStatement)
                {
                    this.WriteFixedStatement(value as IFixedStatement, formatter);
                    return;
                }

                if (value is IWhileStatement)
                {
                    this.WriteWhileStatement(value as IWhileStatement, formatter);
                    return;
                }

                if (value is IDoStatement)
                {
                    this.WriteDoStatement(value as IDoStatement, formatter);
                    return;
                }

                if (value is ITryCatchFinallyStatement)
                {
                    this.WriteTryCatchFinallyStatement(value as ITryCatchFinallyStatement, formatter);
                    return;
                }

                if (value is IThrowExceptionStatement)
                {
                    this.WriteThrowExceptionStatement(value as IThrowExceptionStatement, formatter);
                    return;
                }

                if (value is IAttachEventStatement)
                {
                    this.WriteAttachEventStatement(value as IAttachEventStatement, formatter);
                    return;
                }

                if (value is IRemoveEventStatement)
                {
                    this.WriteRemoveEventStatement(value as IRemoveEventStatement, formatter);
                    return;
                }

                if (value is ISwitchStatement)
                {
                    this.WriteSwitchStatement(value as ISwitchStatement, formatter);
                    return;
                }

                if (value is IBreakStatement)
                {
                    this.WriteBreakStatement(value as IBreakStatement, formatter);
                    return;
                }

                if (value is IContinueStatement)
                {
                    this.WriteContinueStatement(value as IContinueStatement, formatter);
                    return;
                }

                if (value is IMemoryCopyStatement)
                {
                    this.WriteMemoryCopyStatement(value as IMemoryCopyStatement, formatter);
                    return;
                }

                if (value is IMemoryInitializeStatement)
                {
                    this.WriteMemoryInitializeStatement(value as IMemoryInitializeStatement, formatter);
                    return;
                }

                if (value is IDebugBreakStatement)
                {
                    this.WriteDebugBreakStatement(value as IDebugBreakStatement, formatter);
                    return;
                }

                if (value is ILockStatement)
                {
                    this.WriteLockStatement(value as ILockStatement, formatter);
                    return;
                }

                if (value is ICommentStatement)
                {
                    this.WriteCommentStatement(value as ICommentStatement, formatter);
                    return;
                }

                throw new ArgumentException("Invalid statement type.", "value");
            }

            private void WriteStatementSeparator(IFormatter formatter)
            {
                if (this.firstStmt)
                {
                    this.firstStmt = false;
                }
                else
                {
                    if (!this.forLoop)
                    {
                        //formatter.Write(";");
                        formatter.WriteLine();
                    }
                }
            }

            private void WriteBlockStatement(IBlockStatement statement, IFormatter formatter)
            {
                blockStatementLevel++;
                if (statement.Statements.Count > 0)
                {
                    this.WriteStatementList(statement.Statements, formatter);
                }
                blockStatementLevel++;
            }

            private void WriteStatementList(IStatementCollection statements, IFormatter formatter)
            {
                this.firstStmt = true;
                //  Statement Start
                formatter.WriteComment("//");
                formatter.WriteLine();
                //
                for (int i = 0; i < statements.Count; i++)
                {
                    this.WriteStatement(statements[i], formatter, (i == statements.Count - 1));
                }
            }


            private void WriteMemoryCopyStatement(IMemoryCopyStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);

                formatter.WriteKeyword("MemCopy");
                formatter.Write("(");
                this.WriteExpression(statement.Destination, formatter);
                formatter.Write(", ");
                this.WriteExpression(statement.Source, formatter);
                formatter.Write(", ");
                this.WriteExpression(statement.Length, formatter);
                formatter.Write(")");
            }

            private void WriteMemoryInitializeStatement(IMemoryInitializeStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);

                formatter.WriteKeyword("MemSet");
                formatter.Write("(");
                this.WriteExpression(statement.Offset, formatter);
                formatter.Write(", ");
                this.WriteExpression(statement.Value, formatter);
                formatter.Write(", ");
                this.WriteExpression(statement.Length, formatter);
                formatter.Write(")");
            }

            private void WriteDebugBreakStatement(IDebugBreakStatement value, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);

                formatter.WriteKeyword("debug");
            }

            private void WriteLockStatement(ILockStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);

                formatter.WriteKeyword("lock");
                formatter.Write(" ");
                formatter.Write("(");
                this.WriteExpression(statement.Expression, formatter);
                formatter.Write(")");
                formatter.WriteLine();

                formatter.WriteKeyword("begin");
                formatter.WriteIndent();

                if (statement.Body != null)
                {
                    this.WriteStatement(statement.Body, formatter);
                }

                formatter.WriteLine();
                formatter.WriteOutdent();
                formatter.WriteKeyword("end");
            }

            internal static IExpression InverseBooleanExpression(IExpression expression)
            {
                IBinaryExpression binaryExpression = expression as IBinaryExpression;
                if (binaryExpression != null)
                {
                    switch (binaryExpression.Operator)
                    {
                        case BinaryOperator.GreaterThan:
                            {
                                IBinaryExpression target = new BinaryExpression();
                                target.Left = binaryExpression.Left;
                                target.Operator = BinaryOperator.LessThanOrEqual;
                                target.Right = binaryExpression.Right;
                                return target;
                            }

                        case BinaryOperator.GreaterThanOrEqual:
                            {
                                IBinaryExpression target = new BinaryExpression();
                                target.Left = binaryExpression.Left;
                                target.Operator = BinaryOperator.LessThan;
                                target.Right = binaryExpression.Right;
                                return target;
                            }

                        case BinaryOperator.LessThan:
                            {
                                IBinaryExpression target = new BinaryExpression();
                                target.Left = binaryExpression.Left;
                                target.Operator = BinaryOperator.GreaterThanOrEqual;
                                target.Right = binaryExpression.Right;
                                return target;
                            }

                        case BinaryOperator.LessThanOrEqual:
                            {
                                IBinaryExpression target = new BinaryExpression();
                                target.Left = binaryExpression.Left;
                                target.Operator = BinaryOperator.GreaterThan;
                                target.Right = binaryExpression.Right;
                                return target;
                            }

                        case BinaryOperator.IdentityEquality:
                            {
                                IBinaryExpression target = new BinaryExpression();
                                target.Left = binaryExpression.Left;
                                target.Operator = BinaryOperator.IdentityInequality;
                                target.Right = binaryExpression.Right;
                                return target;
                            }

                        case BinaryOperator.IdentityInequality:
                            {
                                IBinaryExpression target = new BinaryExpression();
                                target.Left = binaryExpression.Left;
                                target.Operator = BinaryOperator.IdentityEquality;
                                target.Right = binaryExpression.Right;
                                return target;
                            }

                        case BinaryOperator.ValueInequality:
                            {
                                IBinaryExpression target = new BinaryExpression();
                                target.Left = binaryExpression.Left;
                                target.Operator = BinaryOperator.ValueEquality;
                                target.Right = binaryExpression.Right;
                                return target;
                            }
                        case BinaryOperator.ValueEquality:
                            {
                                IBinaryExpression target = new BinaryExpression();
                                target.Left = binaryExpression.Left;
                                target.Operator = BinaryOperator.ValueInequality;
                                target.Right = binaryExpression.Right;
                                return target;
                            }

                        case BinaryOperator.BooleanAnd: // De Morgan
                            {
                                IExpression left = InverseBooleanExpression(binaryExpression.Left);
                                IExpression right = InverseBooleanExpression(binaryExpression.Right);
                                if ((left != null) && (right != null))
                                {
                                    IBinaryExpression target = new BinaryExpression();
                                    target.Left = left;
                                    target.Operator = BinaryOperator.BooleanOr;
                                    target.Right = right;
                                    return target;
                                }
                            }
                            break;


                        case BinaryOperator.BooleanOr: // De Morgan
                            {
                                IExpression left = InverseBooleanExpression(binaryExpression.Left);
                                IExpression right = InverseBooleanExpression(binaryExpression.Right);
                                if ((left != null) && (right != null))
                                {
                                    IBinaryExpression target = new BinaryExpression();
                                    target.Left = left;
                                    target.Operator = BinaryOperator.BooleanAnd;
                                    target.Right = right;
                                    return target;
                                }
                            }
                            break;
                    }
                }

                IUnaryExpression unaryExpression = expression as IUnaryExpression;
                if (unaryExpression != null)
                {
                    if (unaryExpression.Operator == UnaryOperator.BooleanNot)
                    {
                        return unaryExpression.Expression;
                    }
                }

                IUnaryExpression unaryOperator = new UnaryExpression();
                unaryOperator.Operator = UnaryOperator.BooleanNot;
                unaryOperator.Expression = expression;
                return unaryOperator;
            }

            //-------------------------------------------
            // this writes one line of variable declaration 
            // 
            private void WriteVariableListEntry(IVariableDeclaration variable, IFormatter formatter, ref bool hasvar)
            {
                if (variable != null)
                {
                    formatter.WriteLine();
                    formatter.WriteIndent();
                    formatter.WriteKeyword("local");
                    formatter.Write(" ");
                    this.WriteVariableDeclaration(variable, formatter);
                    formatter.WriteOutdent();
                }
            }

            private void WriteVariableList(IVariableDeclarationExpression expression, IFormatter formatter, ref bool hasvar)
            {
                if (expression != null)
                    WriteVariableListEntry(expression.Variable, formatter, ref hasvar);
            }

            private void WriteVariableList(IStatement statement, IFormatter formatter, ref bool hasvar)
            {
                IBlockStatement blockStatement = statement as IBlockStatement;
                if (blockStatement != null)
                {
                    WriteVariableList(blockStatement.Statements, formatter, ref hasvar);
                    return;
                }

                ILabeledStatement labeledStatement = statement as ILabeledStatement;
                if (labeledStatement != null)
                {
                    WriteVariableList(labeledStatement.Statement, formatter, ref hasvar);
                    return;
                }

                IForEachStatement forEachStatement = statement as IForEachStatement;
                if (forEachStatement != null)
                {
                    WriteVariableListEntry(forEachStatement.Variable, formatter, ref hasvar);
                    WriteVariableList(forEachStatement.Body, formatter, ref hasvar);
                    return;
                }

                IConditionStatement conditionStatement = statement as IConditionStatement;
                if (conditionStatement != null)
                {
                    WriteVariableList(conditionStatement.Then, formatter, ref hasvar);
                    WriteVariableList(conditionStatement.Else, formatter, ref hasvar);
                    return;
                }

                IForStatement forStatement = statement as IForStatement;
                if (forStatement != null)
                {
                    WriteVariableList(forStatement.Initializer, formatter, ref hasvar);
                    WriteVariableList(forStatement.Body, formatter, ref hasvar);
                    return;
                }

                ISwitchStatement switchStatement = statement as ISwitchStatement;
                if (switchStatement != null)
                {
                    foreach (ISwitchCase switchCase in switchStatement.Cases)
                        WriteVariableList(switchCase.Body, formatter, ref hasvar);
                    return;
                }

                IDoStatement doStatement = statement as IDoStatement;
                if (doStatement != null)
                {
                    WriteVariableList(doStatement.Body, formatter, ref hasvar);
                    return;
                }

                ILockStatement lockStatement = statement as ILockStatement;
                if (lockStatement != null)
                {
                    WriteVariableList(lockStatement.Body, formatter, ref hasvar);
                    return;
                }

                IWhileStatement whileStatement = statement as IWhileStatement;
                if (whileStatement != null)
                {
                    WriteVariableList(whileStatement.Body, formatter, ref hasvar);
                    return;
                }

                IFixedStatement fixedStatement = statement as IFixedStatement;
                if (fixedStatement != null)
                {
                    WriteVariableListEntry(fixedStatement.Variable, formatter, ref hasvar);
                    WriteVariableList(fixedStatement.Body, formatter, ref hasvar);
                    return;
                }

                IUsingStatement usingStatement = statement as IUsingStatement;
                if (usingStatement != null)
                {
                    IAssignExpression assignExpression = usingStatement.Expression as IAssignExpression;
                    if (assignExpression != null)
                    {
                        IVariableDeclarationExpression variableDeclarationExpression = assignExpression.Target as IVariableDeclarationExpression;
                        if (variableDeclarationExpression != null)
                        {
                            this.WriteVariableListEntry(variableDeclarationExpression.Variable, formatter, ref hasvar);
                        }
                    }

                    return;
                }


                ITryCatchFinallyStatement tryCatchFinallyStatement = statement as ITryCatchFinallyStatement;
                if (tryCatchFinallyStatement != null)
                {
                    WriteVariableList(tryCatchFinallyStatement.Try, formatter, ref hasvar);
                    foreach (ICatchClause catchClause in tryCatchFinallyStatement.CatchClauses)
                        WriteVariableList(catchClause.Body, formatter, ref hasvar);
                    WriteVariableList(tryCatchFinallyStatement.Fault, formatter, ref hasvar);
                    WriteVariableList(tryCatchFinallyStatement.Finally, formatter, ref hasvar);
                    return;
                }

                IExpressionStatement expressionStatement = statement as IExpressionStatement;
                if (expressionStatement != null)
                {
                    IAssignExpression assignExpression = expressionStatement.Expression as IAssignExpression;
                    if (assignExpression != null)
                    {
                        WriteVariableList(assignExpression.Target as IVariableDeclarationExpression, formatter, ref hasvar);
                        return;
                    }
                    else
                    {
                        //                        VariableDeclarationExpression
                        WriteVariableList(expressionStatement.Expression as IVariableDeclarationExpression, formatter, ref hasvar);
                        return;
                    }

                }

            }

            // write a list of variable definitions by recursing through the statements and define
            //  the corresponding variable names
            private void WriteVariableList(IStatementCollection statements, IFormatter formatter, ref bool hasvar)
            {
                foreach (IStatement statement in statements)
                    WriteVariableList(statement, formatter, ref hasvar);
            }

            private void WriteCommentStatement(ICommentStatement statement, IFormatter formatter)
            {
                this.WriteComment(statement.Comment, formatter);
            }

            private void WriteComment(IComment comment, IFormatter formatter)
            {
                string[] parts = comment.Text.Split(new char[] { '\n' });
                if (parts.Length <= 1)
                {
                    foreach (string part in parts)
                    {
                        formatter.WriteComment("// ");
                        formatter.WriteComment(part);
                        formatter.WriteLine();
                    }
                }
                else
                {
                    formatter.WriteComment("/* ");
                    formatter.WriteLine();

                    foreach (string part in parts)
                    {
                        formatter.WriteComment(part);
                        formatter.WriteLine();
                    }

                    formatter.WriteComment(" */");
                    formatter.WriteLine();
                }
            }

            private void WriteMethodReturnStatement(IMethodReturnStatement statement, IFormatter formatter, bool lastStatement)
            {
                this.WriteStatementSeparator(formatter);
                if (statement.Expression == null)
                {
                    formatter.WriteKeyword("return");
                }
                else
                {
                    if ((!lastStatement) || (blockStatementLevel > 1))
                    {
                        //formatter.WriteKeyword("begin");
                        //formatter.WriteLine();
                        formatter.WriteIndent();
                    }

                    if ((!lastStatement) || (blockStatementLevel > 1))
                    {
                        //formatter.Write(";");
                        //formatter.WriteLine();
                        //formatter.WriteKeyword("exit");
                        //formatter.WriteLine();
                        formatter.WriteOutdent();
                        //formatter.WriteKeyword("end");
                    }

                    formatter.WriteKeyword("return");
                    formatter.Write(" ");
                    this.WriteExpression(statement.Expression, formatter);
                }
            }

            private void WriteConditionStatement(IConditionStatement statement, IFormatter formatter)
            {
                //
                this.WriteStatementSeparator(formatter);
                formatter.WriteKeyword("if");
                formatter.Write(" ");
                if (statement.Condition is IBinaryExpression)
                    this.WriteExpression(statement.Condition, formatter);
                else
                {
                    formatter.Write("(");
                    this.WriteExpression(statement.Condition, formatter);
                    formatter.Write(")");
                }
                //
                formatter.WriteLine();
                formatter.WriteIndent();
                //
                if ((statement.Then != null) && (statement.Then.Statements.Count > 0))
                {
                    this.WriteStatement(statement.Then, formatter);
                }
                //
                if ((statement.Else != null) && (statement.Else.Statements.Count > 0))
                {
                    formatter.WriteOutdent();
                    formatter.WriteLine();
                    formatter.WriteKeyword("else");
                    formatter.WriteLine();
                    formatter.WriteIndent();
                    //
                    this.WriteStatement(statement.Else, formatter);
                }
                formatter.WriteLine();
                formatter.WriteOutdent();
                formatter.WriteKeyword("endif");
            }

            private void WriteTryCatchFinallyStatement(ITryCatchFinallyStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                //
                if ((statement.Finally != null) && (statement.Finally.Statements.Count > 0) && (statement.CatchClauses.Count > 0))
                {
                    formatter.WriteKeyword("try");
                    formatter.WriteLine();
                    formatter.WriteIndent();
                }
                else
                {
                    //
                    if ((statement.Finally != null) && (statement.Finally.Statements.Count > 0) || (statement.CatchClauses.Count > 0))
                    {
                        formatter.WriteKeyword("try");
                        formatter.WriteLine();
                        formatter.WriteIndent();
                    }
                }
                // Do we have code here ?
                if (statement.Try != null)
                {
                    this.WriteStatement(statement.Try, formatter);
                }
                // Outdent
                formatter.WriteLine();
                formatter.WriteOutdent();
                //
                if (statement.CatchClauses.Count > 0)
                {
                    this.firstStmt = true;
                    foreach (ICatchClause catchClause in statement.CatchClauses)
                    {
                        formatter.WriteKeyword("catch");
                        WriteStatementSeparator(formatter);
                        ITypeReference catchType = (ITypeReference)catchClause.Variable.VariableType;
                        bool hiddenName = (catchClause.Variable.Name.Length == 0);
                        bool hiddenType = IsType(catchType, "System", "Object");

                        if ((!hiddenName) || (!hiddenType))
                        {
                            if (!hiddenName)
                            {
                                formatter.Write(" ");
                                formatter.WriteDeclaration(catchClause.Variable.Name);
                                formatter.Write(" ");
                                formatter.WriteKeyword("as");
                                formatter.Write(" ");
                                this.WriteType(catchClause.Variable.VariableType, formatter);
                                formatter.WriteLine();
                            }
                        }
                        //
                        if (catchClause.Condition != null)
                        {
                            formatter.Write(" ");
                            formatter.WriteKeyword("if");
                            formatter.Write(" ");
                            this.WriteExpression(catchClause.Condition, formatter);
                            formatter.Write(" ");
                            formatter.WriteKeyword("then");
                        }

                        if ((catchClause.Body != null) && (catchClause.Body.Statements.Count > 1))
                        {
                            //formatter.WriteKeyword("begin");
                            //formatter.WriteLine();
                        }
                        formatter.WriteIndent();
                        if (catchClause.Body != null)
                        {
                            this.WriteStatement(catchClause.Body, formatter);
                        }
                        if ((catchClause.Body != null) && (catchClause.Body.Statements.Count > 1))
                        {
                            formatter.WriteLine();
                            formatter.WriteOutdent();
                            //formatter.WriteKeyword("end");
                        }
                    }
                }
                // Finally Clause ?
                if ((statement.Finally != null) && (statement.Finally.Statements.Count > 0))
                {
                    formatter.WriteKeyword("finally");
                    formatter.WriteLine();
                    formatter.WriteIndent();
                    if (statement.Finally != null)
                    {
                        this.WriteStatement(statement.Finally, formatter);
                    }
                    formatter.WriteLine();
                    formatter.WriteOutdent();
                    //formatter.WriteKeyword("end finally");
                }
                /*
                if (statement.CatchClauses.Count > 0)
                {
                    formatter.WriteLine();
                    formatter.WriteOutdent();
                    //formatter.WriteKeyword("end catch");
                }
                 */
                formatter.WriteKeyword("end try");
            }

            private void WriteAssignExpression(IAssignExpression value, IFormatter formatter)
            {
                IBinaryExpression binaryExpression = value.Expression as IBinaryExpression;
                if (binaryExpression != null)
                {
                    if (value.Target.Equals(binaryExpression.Left))
                    {
                        string operatorText = string.Empty;
                        switch (binaryExpression.Operator)
                        {
                            case BinaryOperator.Add:
                                operatorText = "+";
                                break;

                            case BinaryOperator.Subtract:
                                operatorText = "-";
                                break;
                        }

                        if (operatorText.Length != 0)
                        {
                            // Op(a,b)
                            this.WriteExpression(value.Target, formatter);
                            formatter.Write(" := ");
                            this.WriteExpression(value.Target, formatter);
                            formatter.Write(" ");
                            formatter.Write(operatorText);
                            formatter.Write(" ");
                            this.WriteExpression(binaryExpression.Right, formatter);
                            return;
                        }
                    }
                }

                // x := y + z
                this.WriteExpression(value.Target, formatter);
                formatter.Write(" := ");
                this.WriteExpression(value.Expression, formatter);
            }

            private void WriteExpressionStatement(IExpressionStatement statement, IFormatter formatter)
            { // in Delphi we have to filter the IExpressionStatement that is a IVariableDeclarationExpression
                // as this is defined/dumped in the method's var section by WriteVariableList
                if (!(statement.Expression is IVariableDeclarationExpression))
                {
                    this.WriteStatementSeparator(formatter);
                    IUnaryExpression unaryExpression = statement.Expression as IUnaryExpression;
                    if (unaryExpression != null && unaryExpression.Operator == UnaryOperator.PostIncrement)
                    {
                        //formatter.Write("inc(");
                        this.WriteExpression(unaryExpression.Expression, formatter);
                        formatter.Write("++");
                    }
                    else if (unaryExpression != null && unaryExpression.Operator == UnaryOperator.PostDecrement)
                    {
                        //formatter.Write("dec(");
                        this.WriteExpression(unaryExpression.Expression, formatter);
                        formatter.Write("--");
                    }
                    else
                    {
                        this.WriteExpression(statement.Expression, formatter);
                    }
                }
            }

            private void WriteForStatement(IForStatement statement, IFormatter formatter)
            {
                bool canUseForLoop = false;

                // Check if the iteration statement is limited enough to emit a Delphi for-loop
                IExpressionStatement InitAssignmentStatement = statement.Initializer as IExpressionStatement;
                IExpressionStatement IncrementAssignmentStatement = statement.Increment as IExpressionStatement;

                IAssignExpression InitAssignment = null;
                IAssignExpression IncrementAssignment = null;

                if (InitAssignmentStatement != null)
                {
                    InitAssignment = InitAssignmentStatement.Expression as IAssignExpression;
                }

                if (IncrementAssignmentStatement != null)
                {
                    IncrementAssignment = IncrementAssignmentStatement.Expression as IAssignExpression;
                }

                IBinaryExpression TestOperator = statement.Condition as IBinaryExpression;
                IBinaryExpression IncrRight = null;

                if ((InitAssignment != null) && (IncrementAssignment != null) && (TestOperator != null))
                {
                    IVariableReferenceExpression InitLeft = InitAssignment.Target as IVariableReferenceExpression;
                    // ILiteralExpression InitRight = InitAssignment.Right as ILiteralExpression;
                    IVariableReferenceExpression IncrLeft = IncrementAssignment.Target as IVariableReferenceExpression;
                    IncrRight = IncrementAssignment.Expression as IBinaryExpression;
                    IVariableReferenceExpression TestLeft = TestOperator.Left as IVariableReferenceExpression;

                    if ((InitLeft != null) && (IncrLeft != null) && (IncrRight != null) && (TestLeft != null))
                    {
                        if ((InitLeft.Variable == IncrLeft.Variable) && (InitLeft.Variable == TestLeft.Variable))
                        {
                            IVariableReferenceExpression IncrFromVar = IncrRight.Left as IVariableReferenceExpression;
                            ILiteralExpression IncrExp = IncrRight.Right as ILiteralExpression;
                            if ((IncrFromVar != null) && (IncrExp != null))
                            {
                                if ((InitLeft.Variable == IncrFromVar.Variable) && (IncrExp.Value.Equals(1)))
                                {
                                    if ((IncrRight.Operator == BinaryOperator.Add) &&
                                    ((TestOperator.Operator == BinaryOperator.LessThan) ||
                                     (TestOperator.Operator == BinaryOperator.LessThanOrEqual)))
                                        canUseForLoop = true;
                                    else
                                        if ((IncrRight.Operator == BinaryOperator.Subtract) &&
                                    ((TestOperator.Operator == BinaryOperator.GreaterThan) ||
                                    (TestOperator.Operator == BinaryOperator.GreaterThanOrEqual)))
                                            canUseForLoop = true;

                                }
                            }
                        }
                    }
                }

                if (canUseForLoop)
                {
                    this.WriteStatementSeparator(formatter);
                    formatter.WriteKeyword("for ");
                    this.firstStmt = true;
                    this.WriteStatement(statement.Initializer, formatter);
                    if (IncrRight.Operator == BinaryOperator.Add)
                        formatter.WriteKeyword(" to ");
                    else
                        formatter.WriteKeyword(" downto ");
                    this.WriteExpression(TestOperator.Right, formatter);
                    // TODO: Handle special case of literal+1 -> 1 etc.
                    if (IncrRight.Operator == BinaryOperator.Add)
                    {
                        if (TestOperator.Operator == BinaryOperator.LessThan)
                            formatter.WriteLiteral("step -1 ");
                    }
                    else
                    {
                        if (TestOperator.Operator == BinaryOperator.GreaterThan)
                            formatter.WriteLiteral("step 1 ");
                    }
                }
                else
                { // Fall back to version that emits while-loops!
                    if (statement.Initializer != null)
                    {
                        this.WriteStatement(statement.Initializer, formatter);
                        this.WriteStatementSeparator(formatter);
                    }

                    formatter.WriteKeyword("while");
                    formatter.Write(" ");
                    formatter.Write("(");
                    if (statement.Condition != null)
                        this.WriteExpression(statement.Condition, formatter);
                    else
                        formatter.WriteLiteral("true");
                    formatter.Write(")");
                }
                //				formatter.Write(" ");
                //				formatter.WriteKeyword("do");
                //				formatter.WriteLine();
                //				formatter.WriteKeyword("begin");
                formatter.WriteLine();
                formatter.WriteIndent();
                if (statement.Body != null)
                {
                    this.WriteStatement(statement.Body, formatter);
                }
                if (!canUseForLoop)
                {
                    if (statement.Increment != null)
                        this.WriteStatement(statement.Increment, formatter);
                }
                formatter.WriteLine();
                formatter.WriteOutdent();
                if (!canUseForLoop)
                {
                    formatter.WriteKeyword("enddo");
                }
                else
                {
                    formatter.WriteKeyword("next");
                }

            }

            private void WriteForEachStatement(IForEachStatement value, IFormatter formatter)
            {
                // FOREACH is a reserved word, but is not implemented currently in Vulcan
                //

                this.WriteStatementSeparator(formatter);
                // Tooltip Helper
                TextFormatter description = new TextFormatter();
                this.WriteVariableDeclaration(value.Variable, description);
                //
                formatter.WriteLine();
                formatter.WriteLiteral("#error Sorry, currenlty not supported with Vulcan");
                formatter.WriteLine();
                //
                formatter.WriteKeyword("foreach");
                formatter.Write(" ");
                this.WriteReference(value.Variable.Name, formatter, description.ToString(), null);
                formatter.Write(" ");
                formatter.WriteKeyword("in");
                formatter.Write(" ");
                this.WriteExpression(value.Expression, formatter);
                //
                formatter.WriteLine();
                formatter.WriteIndent();

                if (value.Body != null)
                {
                    this.WriteStatement(value.Body, formatter);
                }
                //
                formatter.WriteLine();
                formatter.WriteOutdent();
                formatter.WriteKeyword("next");
            }

            private void WriteUsingStatement(IUsingStatement statement, IFormatter formatter)
            {
                IVariableReference variable = null;

                IAssignExpression assignExpression = statement.Expression as IAssignExpression;
                if (assignExpression != null)
                {
                    IVariableDeclarationExpression variableDeclarationExpression = assignExpression.Target as IVariableDeclarationExpression;
                    if (variableDeclarationExpression != null)
                    {
                        variable = variableDeclarationExpression.Variable;
                    }

                    IVariableReferenceExpression variableReferenceExpression = assignExpression.Target as IVariableReferenceExpression;
                    if (variableReferenceExpression != null)
                    {
                        variable = variableReferenceExpression.Variable;
                    }
                }

                this.WriteStatementSeparator(formatter);
                // make a comment that Reflector detected this as a using statement
                formatter.WriteComment("// using");

                if (variable != null)
                {
                    formatter.Write(" ");
                    this.WriteVariableReference(variable, formatter);
                }

                formatter.WriteComment("//");
                formatter.WriteLine();

                // and replace this with
                // - create obj
                // - try ... finally obj.Dispose end

                if (variable != null)
                {
                    this.WriteVariableReference(variable, formatter);
                    formatter.Write(" ");
                    formatter.WriteKeyword(":=");
                    formatter.Write(" ");
                    this.WriteExpression(assignExpression.Expression, formatter);
                    this.WriteStatementSeparator(formatter);
                }

                formatter.WriteKeyword("try");
                formatter.WriteLine();
                formatter.WriteIndent();

                if (statement.Body != null)
                {
                    this.WriteBlockStatement(statement.Body, formatter);
                }

                formatter.WriteLine();
                formatter.WriteOutdent();
                formatter.WriteKeyword("finally");
                formatter.WriteLine();
                formatter.WriteIndent();

                if (variable != null)
                {
                    this.firstStmt = true;
                    this.WriteVariableReference(variable, formatter);
                    formatter.Write(".");
                    formatter.Write("Dispose()");
                    formatter.WriteLine();
                }
                else
                {
                    this.firstStmt = true;
                    this.WriteExpression(statement.Expression);
                    formatter.Write(".");
                    formatter.Write("Dispose()");
                    formatter.WriteLine();
                }

                formatter.WriteOutdent();
                formatter.WriteKeyword("end try");
                formatter.WriteLine();
                formatter.WriteOutdent();
                formatter.WriteComment("// end using");
                formatter.WriteLine();
            }

            private void WriteFixedStatement(IFixedStatement statement, IFormatter formatter)
            {
                // Fixed vars are not moveabel by Garbage Collector
                // Does Vulcan support Fixed Vars ? ... sorry, I don't know....
                // so, let's improvise....
                //
                this.WriteStatementSeparator(formatter);

                formatter.WriteLine();
                formatter.WriteLiteral("#error Sorry, currenlty not supported with Vulcan");
                formatter.WriteLine();

                formatter.WriteKeyword("fixed");
                formatter.Write(" ");
                formatter.Write("(");
                // This would write myvar as mytype
                //this.WriteVariableDeclaration(statement.Variable, formatter);
                this.WriteDeclaration(statement.Variable.Name, formatter);
                formatter.Write(" ");
                formatter.WriteKeyword(":=");
                formatter.Write(" ");
                this.WriteExpression(statement.Expression, formatter);
                formatter.Write(")");

                formatter.WriteLine();
                formatter.WriteIndent();

                if (statement.Body != null)
                {
                    this.WriteBlockStatement(statement.Body, formatter);
                }

                formatter.WriteOutdent();
                formatter.WriteLine();
                formatter.WriteKeyword("end fixed");
            }

            private void WriteWhileStatement(IWhileStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                formatter.WriteKeyword("while");
                formatter.Write(" ");
                if (statement.Condition != null)
                {
                    formatter.Write("(");
                    this.WriteExpression(statement.Condition, formatter);
                    formatter.Write(")");
                }
                else
                    formatter.WriteLiteral("true");
                //
                formatter.WriteLine();
                formatter.WriteIndent();
                if (statement.Body != null)
                {
                    this.WriteStatement(statement.Body, formatter);
                }
                formatter.WriteLine();
                formatter.WriteOutdent();
                formatter.WriteKeyword("enddo");
            }

            private void WriteDoStatement(IDoStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                formatter.WriteKeyword("while");
                formatter.Write(" ");
                formatter.WriteLiteral("true");
                formatter.WriteLine();
                formatter.WriteIndent();
                if (statement.Body != null)
                {
                    this.WriteStatement(statement.Body, formatter);
                }
                //
                if (statement.Condition != null)
                {
                    formatter.WriteLine();
                    formatter.WriteKeyword("if");
                    formatter.Write(" ");
                    if (statement.Condition is IBinaryExpression)
                        this.WriteExpression(InverseBooleanExpression(statement.Condition), formatter);
                    else
                    {
                        formatter.Write("(");
                        this.WriteExpression(InverseBooleanExpression(statement.Condition), formatter);
                        formatter.Write(")");
                    }
                    formatter.WriteLine();
                    formatter.WriteIndent();
                    formatter.WriteKeyword("exit");
                    formatter.WriteLine();
                    formatter.WriteOutdent();
                    formatter.WriteKeyword("endif");
                }
                //
                formatter.WriteLine();
                formatter.WriteOutdent();
                formatter.WriteKeyword("enddo");
                //
            }

            private void WriteBreakStatement(IBreakStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                formatter.WriteKeyword("exit");
                //formatter.Write(";");
                formatter.WriteLine();
            }

            private void WriteContinueStatement(IContinueStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                formatter.WriteKeyword("loop");
                //formatter.Write(";");
                formatter.WriteLine();
            }

            private void WriteThrowExceptionStatement(IThrowExceptionStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                formatter.WriteKeyword("throw");
                formatter.Write(" ");
                if (statement.Expression != null)
                    this.WriteExpression(statement.Expression, formatter);
                else
                {
                    this.WriteDeclaration("Exception", formatter);
                    formatter.Write(".");
                    formatter.WriteKeyword("Create");
                }
            }

            private void WriteVariableDeclarationExpression(IVariableDeclarationExpression expression, IFormatter formatter)
            {
                //this.WriteVariableDeclaration(expression.Variable, formatter); // this is for C#
                //
                // no variable declaration expression in Delphi. Convert this to a variable reference only!
                this.WriteVariableReference(expression.Variable, formatter);
            }

            private void WriteVariableDeclaration(IVariableDeclaration variableDeclaration, IFormatter formatter)
            {
                // Write Variable Name
                this.WriteDeclaration(variableDeclaration.Name, formatter);
                //
                formatter.Write(" ");
                formatter.WriteKeyword("as");
                formatter.Write(" ");
                // Write variable type
                this.WriteType(variableDeclaration.VariableType, formatter);

                if (variableDeclaration.Pinned)
                {
                    formatter.Write(" ");
                    formatter.WriteKeyword("pinned");
                }

                if (!this.forLoop)
                {
                    //formatter.Write(";");
                    //formatter.WriteLine();
                }
            }

            private void WriteAttachEventStatement(IAttachEventStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                this.WriteEventReferenceExpression(statement.Event, formatter);
                formatter.Write(" += ");
                this.WriteExpression(statement.Listener, formatter);
                //formatter.Write(";");
                formatter.WriteLine();
            }

            private void WriteRemoveEventStatement(IRemoveEventStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                this.WriteEventReferenceExpression(statement.Event, formatter);
                formatter.Write(" -= ");
                this.WriteExpression(statement.Listener, formatter);
                //formatter.Write(";");
                formatter.WriteLine();
            }

            private void WriteSwitchStatement(ISwitchStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);

                formatter.WriteKeyword("do case");
                formatter.WriteLine();
                foreach (ISwitchCase switchCase in statement.Cases)
                {

                    IDefaultCase defaultCase = switchCase as IDefaultCase;
                    if (defaultCase != null)
                    {
                        formatter.WriteKeyword("otherwise");
                        formatter.WriteLine();
                    }
                    else
                    {
                        formatter.WriteKeyword("case");
                        formatter.Write(" ( ");
                        //this.WriteExpression(statement.Expression, formatter);
                        //formatter.Write(" == ");
                        //formatter.WriteKeyword("of");
                        //formatter.WriteLine();
                        //formatter.WriteIndent();

                        IConditionCase conditionCase = switchCase as IConditionCase;
                        if (conditionCase != null)
                        {
                            this.WriteSwitchCaseCondition(conditionCase.Condition, formatter, statement.Expression);
                        }
                        // 
                        formatter.Write(" ) ");
                        formatter.WriteLine();

                    }
                    //formatter.WriteLine();
                    formatter.WriteIndent();

                    if (switchCase.Body != null)
                    {
                        this.WriteStatement(switchCase.Body, formatter);
                    }

                    //formatter.WriteLine();
                    formatter.WriteOutdent();

                    formatter.WriteLine();
                }
                //formatter.WriteOutdent();
                formatter.WriteKeyword("endcase");
            }

            private void WriteSwitchCaseCondition(IExpression condition, IFormatter formatter, IExpression value)
            {
                IBinaryExpression binaryExpression = condition as IBinaryExpression;
                if ((binaryExpression != null) && (binaryExpression.Operator == BinaryOperator.BooleanOr))
                {
                    this.WriteSwitchCaseCondition(binaryExpression.Left, formatter, value);
                    formatter.Write(" ) ");
                    formatter.WriteKeyword(".or.");
                    formatter.Write(" ;");
                    formatter.WriteLine();
                    formatter.Write(" ( ");
                    this.WriteSwitchCaseCondition(binaryExpression.Right, formatter, value);
                }
                else
                {
                    this.WriteExpression(value, formatter);
                    formatter.Write(" == ");
                    this.WriteExpression(condition, formatter);
                }
            }

            private void WriteGotoStatement(IGotoStatement statement, IFormatter formatter)
            {
                this.WriteStatementSeparator(formatter);
                formatter.WriteKeyword("goto");
                formatter.Write(" ");
                this.WriteDeclaration(statement.Name, formatter);
            }

            private void WriteLabeledStatement(ILabeledStatement statement, IFormatter formatter)
            {
                if (statement.Statement != null)
                {
                    this.WriteStatementSeparator(formatter);
                }
                formatter.WriteLine();
                formatter.WriteOutdent();
                this.WriteDeclaration(statement.Name, formatter);
                formatter.Write(":");
                formatter.WriteLine();
                formatter.WriteIndent();
                this.firstStmt = true;
                if (statement.Statement != null)
                {
                    this.WriteStatement(statement.Statement, formatter);

                }
            }
            #endregion

            private void WriteDeclaringType(ITypeReference value, IFormatter formatter)
            {
                formatter.WriteProperty("Declaring Type", GetDelphiStyleResolutionScope(value));
                this.WriteDeclaringAssembly(Helper.GetAssemblyReference(value), formatter);
            }

            private void WriteDeclaringAssembly(IAssemblyReference value, IFormatter formatter)
            {
                if (value != null)
                {
                    string text = ((value.Name != null) && (value.Version != null)) ? (value.Name + ", Version=" + value.Version.ToString()) : value.ToString();
                    formatter.WriteProperty("Assembly", text);
                }
            }

            private string GetTypeReferenceDescription(ITypeReference typeReference)
            {
                return Helper.GetNameWithResolutionScope(typeReference);
            }

            private string GetFieldReferenceDescription(IFieldReference fieldReference)
            {
                IFormatter formatter = new TextFormatter();

                this.WriteType(fieldReference.FieldType, formatter);
                formatter.Write(" ");
                formatter.Write(this.GetTypeReferenceDescription(fieldReference.DeclaringType as ITypeReference));
                formatter.Write(".");
                // Qualified value, ignore Keyword prefixing (@@)
                bool TempFlag = this.KeepKeyword;
                this.KeepKeyword = true;
                this.WriteDeclaration(fieldReference.Name, formatter);
                this.KeepKeyword = TempFlag;
                //

                return formatter.ToString();
            }

            private string GetMethodReferenceDescription(IMethodReference value)
            {
                IFormatter formatter = new TextFormatter();

                if (this.IsConstructor(value))
                {
                    formatter.Write(this.GetTypeReferenceDescription(value.DeclaringType as ITypeReference));
                    formatter.Write(".");
                    formatter.Write(Helper.GetNameWithResolutionScope(value.DeclaringType as ITypeReference));
                }
                else
                {
                    // TODO custom attributes [return: ...]
                    this.WriteType(value.ReturnType.Type, formatter);
                    formatter.Write(" ");
                    formatter.Write(Helper.GetNameWithResolutionScope(value.DeclaringType as ITypeReference));
                    formatter.Write(".");
                    formatter.Write(value.Name);
                }

                this.WriteGenericArgumentList(value.GenericArguments, formatter);

                formatter.Write("(");

                this.WriteParameterDeclarationList(value.Parameters, formatter, null);

                if (value.CallingConvention == MethodCallingConvention.VariableArguments)
                {
                    formatter.WriteKeyword(", __arglist");
                }

                formatter.Write(")");

                return formatter.ToString();
            }

            private string GetPropertyReferenceDescription(IPropertyReference propertyReference)
            {
                // Build Description String to be shown in Tooltip
                IFormatter formatter = new TextFormatter();
                //
                // Name
                string propertyName = propertyReference.Name;
                if (propertyName == "Item")
                {
                    propertyName = "self<<";
                }
                formatter.Write("access ");
                formatter.Write(this.GetTypeReferenceDescription(propertyReference.DeclaringType as ITypeReference));
                formatter.Write(".");

                this.WriteDeclaration(propertyName, formatter);

                // Parameters
                IParameterDeclarationCollection parameters = propertyReference.Parameters;
                if (parameters.Count > 0)
                {
                    formatter.Write("[");
                    this.WriteParameterDeclarationList(parameters, formatter, null);
                    formatter.Write("]");
                }
                formatter.Write(" as ");
                this.WriteType(propertyReference.PropertyType, formatter);
                //
                return formatter.ToString();
            }

            private string GetEventReferenceDescription(IEventReference eventReference)
            {
                IFormatter formatter = new TextFormatter();

                formatter.WriteKeyword("event");
                formatter.Write(" ");
                this.WriteType(eventReference.EventType, formatter);
                formatter.Write(" ");
                formatter.Write(this.GetTypeReferenceDescription(eventReference.DeclaringType as ITypeReference));
                formatter.Write(".");
                this.WriteDeclaration(eventReference.Name, formatter);


                return formatter.ToString();
            }

            private static bool IsType(IType value, string namespaceName, string name)
            {
                return IsType(value, namespaceName, name, "mscorlib");
            }

            private static bool IsType(IType value, string namespaceName, string name, string assemblyName)
            {
                ITypeReference typeReference = value as ITypeReference;
                if (typeReference != null)
                {
                    return ((typeReference.Name == name) && (typeReference.Namespace == namespaceName) && (IsAssemblyReference(typeReference, assemblyName)));
                }

                IRequiredModifier requiredModifier = value as IRequiredModifier;
                if (requiredModifier != null)
                {
                    return IsType(requiredModifier.ElementType, namespaceName, name);
                }

                IOptionalModifier optionalModifier = value as IOptionalModifier;
                if (optionalModifier != null)
                {
                    return IsType(optionalModifier.ElementType, namespaceName, name);
                }

                return false;
            }

            private static bool IsAssemblyReference(ITypeReference value, string assemblyName)
            {
                return (Helper.GetAssemblyReference(value).Name == assemblyName);
            }

            private ICustomAttribute GetCustomAttribute(ICustomAttributeProvider value, string namespaceName, string name)
            {
                return this.GetCustomAttribute(value, namespaceName, name, "mscorlib");
            }

            private ICustomAttribute GetCustomAttribute(ICustomAttributeProvider value, string namespaceName, string name, string assemblyName)
            {
                foreach (ICustomAttribute customAttribute in value.Attributes)
                {
                    if (IsType(customAttribute.Constructor.DeclaringType, namespaceName, name, assemblyName))
                    {
                        return customAttribute;
                    }
                }

                return null;
            }

            private ILiteralExpression GetDefaultParameterValue(IParameterDeclaration value)
            {
                ICustomAttribute customAttribute = this.GetCustomAttribute(value, "System.Runtime.InteropServices", "DefaultParameterValueAttribute", "System");
                if ((customAttribute != null) && (customAttribute.Arguments.Count == 1))
                {
                    return customAttribute.Arguments[0] as ILiteralExpression;
                }

                return null;
            }

            private bool IsConstructor(IMethodReference value)
            {
                return ((value.Name == ".ctor") || (value.Name == ".cctor"));
            }

            private bool IsEnumerationElement(IFieldDeclaration value)
            {
                IType fieldType = value.FieldType;
                IType declaringType = value.DeclaringType;
                if (fieldType.Equals(declaringType))
                {
                    ITypeReference typeReference = fieldType as ITypeReference;
                    if (typeReference != null)
                    {
                        return Helper.IsEnumeration(typeReference);
                    }
                }

                return false;
            }

            private string QuoteLiteralExpression(string text, bool IsString)
            {
                // Write a string, char by char, checking for special ones
                string newtext = "";
                string specials = "\r\t\'\"\0\n";
                bool IsOpen = false;
                bool LastWasSpecial = false;
                //
                for (int i = 0; i < text.Length; i++)
                {
                    char character = text[i];
                    ushort value = (ushort)character;
                    //
                    if (value > 0x00ff)
                    {
                        newtext = newtext + "0x" + value.ToString("x4", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        if (LastWasSpecial)
                            newtext += "+";
                        if (specials.IndexOf(character) != -1)
                        {

                            if ((newtext.Length > 0) && IsOpen)
                            {
                                if (IsString)
                                    newtext += "\"+";   //Strings needs double quote
                                else
                                    newtext += "'+";    //Chars needs single quote
                                IsOpen = false;

                            }
                            LastWasSpecial = true;
                        }
                        //
                        switch (character)
                        {
                            case '\r':
                                newtext += "chr(13)";
                                break;
                            case '\t':
                                newtext += "chr(9)";
                                break;
                            case '\'':
                                newtext += "chr(39)";
                                break;
                            case '"':
                                newtext += "chr(34)";
                                break;
                            case '\0':
                                newtext += "chr(0)";
                                break;
                            case '\n':
                                newtext += "chr(10)";
                                break;
                            default:
                                if (!IsOpen)
                                {
                                    if (IsString)
                                        newtext += "\"";
                                    else
                                        newtext += "'";
                                    IsOpen = true;
                                }
                                newtext += character;
                                LastWasSpecial = false;
                                break;
                        }
                    }
                }
                //
                if (IsOpen)
                {
                    if (IsString)
                        newtext += "\"";
                    else
                        newtext += "'";
                }
                //
                return newtext; //writer.ToString();
            }

            private void WriteDeclaration(string name, IFormatter formatter)
            {
                // Variable auto-generated by the compiler have a $ in name
                name = name.Replace("$", "_");
                // Variable name is a reserved keyword ?
                if ((Array.IndexOf(this.keywords, name.ToLower()) != -1) && (!this.KeepKeyword))
                {
                    formatter.WriteDeclaration("@@" + name);
                }
                else
                {
                    formatter.WriteDeclaration(name);
                }
                //
            }

            private void WriteDeclaration(string name, object target, IFormatter formatter)
            {
                // Variable auto-generated by the compiler have a $ in name
                name = name.Replace("$", "_");
                // Variable name is a reserved keyword ?
                if ((Array.IndexOf(this.keywords, name.ToLower()) != -1) && (!this.KeepKeyword))
                {
                    formatter.WriteDeclaration("@@" + name, target);
                }
                else
                {
                    formatter.WriteDeclaration(name, target);
                }
                //
            }

            private void WriteReference(string name, IFormatter formatter, string toolTip, object reference)
            {
                string text = name;
                if (name.Equals(".ctor"))
                {
                    text = "constructor";
                }
                if (name.Equals("..ctor"))
                {
                    text = "constructor";
                }
                //
                text = name;
                if ((Array.IndexOf(this.keywords, name.ToLower()) != -1) && (!this.KeepKeyword))
                {
                    text = "@@" + text;
                }
                formatter.WriteReference(text, toolTip, reference);
            }

            private string[] keywords = new string[] {
					"and",            "array",         "as",           "asm",
					"begin",          "case",          "class",        "const",
					"date",           "hidden",        "dispinterface","div",
					"do",             "downto",        "else",         "end",
                    "enum",           "local",
					"except",         "exports",       "file",         "finalization",
					"finally",        "for",           "function",     "goto",
					"if",             "implements",    "in",           "inherit",
                    "instance",
					"initialization", "inline",        "interface",    "is",
					"label",          "library",       "mod",          "nil",
					"not",            "object",        "of",           "or",
					"out",            "packed",        "procedure",    "ptr",
					"property",       "raise",         "record",       "repeat",
					"resourcestring", "set",           "shl",          "shr",
					/*"string", */    "then",          "threadvar",    "to",
					"try",            "type",          "unit",         "until",
					"uses",           "var",           "while",        "with",
					"xor",            "return",        "sequence",     "super"
				};

            private class TextFormatter : IFormatter
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

        class VulcanLanguageCfg
        {
            XmlReader reader;
            bool OneBased;
            bool Externalize;
            bool RemovePropAttribute;
            bool FullyQualifiedType;
            bool Qualify_SystemOnly;
            bool AlwaysSuper;
            //
            public bool ArrayIsOneBased
            {
                get
                {
                    return this.OneBased;
                }
            }

            public bool ExternalizeNestedTypes
            {
                get
                {
                    return this.Externalize;
                }
            }

            public bool RemovePropertyAttribute
            {
                get
                {
                    return this.RemovePropAttribute;
                }
            }

            public bool FullyQualifiedTypes
            {
                get
                {
                    return this.FullyQualifiedType;
                }
            }

            public bool QualifySytemOnly
            {
                get
                {
                    return this.Qualify_SystemOnly;
                }
            }

            public bool AlwaysCallSuper
            {
                get
                {
                    return this.AlwaysSuper;
                }
            }
            public VulcanLanguageCfg()
            {
                string ApplicationPath;
                string CfgFile;
                // Default Values
                this.OneBased = true;
                this.Externalize = true;
                this.RemovePropAttribute = true;
                this.FullyQualifiedType = true;
                this.Qualify_SystemOnly = true;
                this.AlwaysSuper = true;
                //
                ApplicationPath = Directory.GetCurrentDirectory();
                CfgFile = ApplicationPath + "\\" + "Reflector.VulcanLanguage.cfg";
                //
                if (File.Exists(CfgFile))
                {
                    reader = XmlReader.Create(CfgFile);
                    //
                    reader.MoveToContent();
                    // 
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                //
                                switch (reader.Name)
                                {
                                    case "Array":
                                        if (reader.MoveToAttribute("OneBased"))
                                            this.OneBased = Convert.ToBoolean(reader.Value);
                                        break;
                                    case "NestedTypes":
                                        if (reader.MoveToAttribute("Externalize"))
                                            this.Externalize = Convert.ToBoolean(reader.Value);
                                        break;
                                    case "Attributes":
                                        if (reader.MoveToAttribute("RemoveForProperties"))
                                            this.RemovePropAttribute = Convert.ToBoolean(reader.Value);
                                        break;
                                    case "Types":
                                        if (reader.MoveToAttribute("FullyQualified"))
                                            this.FullyQualifiedType = Convert.ToBoolean(reader.Value);
                                        if (reader.MoveToAttribute("QualifySystemOnly"))
                                            this.Qualify_SystemOnly = Convert.ToBoolean(reader.Value);
                                        break;
                                    case "Constructor":
                                        if (reader.MoveToAttribute("AlwaysCallSuper"))
                                            this.AlwaysSuper = Convert.ToBoolean(reader.Value);
                                        break;

                                }
                                break;
                            case XmlNodeType.EndElement:
                                //
                                break;
                        }
                    }
                    //
                }
                else
                {
                    // First, settings
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = false;
                    //
                    XmlWriter writer = XmlWriter.Create(CfgFile, settings);
                    // 
                    writer.WriteStartElement("Vulcan.net");
                    // 
                    writer.WriteStartElement("Array");
                    writer.WriteAttributeString("OneBased", this.OneBased.ToString());
                    writer.WriteEndElement();
                    //
                    writer.WriteStartElement("NestedTypes");
                    writer.WriteAttributeString("Externalize", this.Externalize.ToString());
                    writer.WriteEndElement();
                    //
                    writer.WriteStartElement("Types");
                    writer.WriteAttributeString("FullyQualified", this.FullyQualifiedType.ToString());
                    writer.WriteAttributeString("QualifySystemOnly", this.Qualify_SystemOnly.ToString());
                    writer.WriteEndElement();
                    //
                    writer.WriteStartElement("Attributes");
                    writer.WriteAttributeString("RemoveForProperties", this.RemovePropAttribute.ToString());
                    writer.WriteEndElement();
                    //
                    writer.WriteStartElement("Constructor");
                    writer.WriteAttributeString("AlwaysCallSuper", this.AlwaysSuper.ToString());
                    writer.WriteEndElement();
                    //
                    writer.WriteEndElement();
                    // 
                    writer.Close();
                }

            }

        }
    }
}
