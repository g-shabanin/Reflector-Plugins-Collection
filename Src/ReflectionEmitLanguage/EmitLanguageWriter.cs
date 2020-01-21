// ---------------------------------------------------------
// Jonathan de Halleux Reflection Emit Language for Reflector
// Copyright (c) 2007 Jonathan de Halleux. All rights reserved.
// ---------------------------------------------------------
using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using Reflector.CodeModel;

namespace Reflector.ReflectionEmitLanguage
{
    internal sealed class EmitLanguageWriter : ILanguageWriter
    {
        private readonly IFormatter formatter;
        private readonly ILanguageWriterConfiguration configuration;
        private readonly CodeModelSearch search;
        private Hashtable operandLocals;

        public EmitLanguageWriter(
            IServiceProvider serviceProvider,
            IFormatter formatter,
            ILanguageWriterConfiguration configuration
            )
        {
            this.search = new CodeModelSearch(serviceProvider);
            this.formatter = formatter;
            this.configuration = configuration;
        }

        public void WriteAssembly(IAssembly value)
        {
            ITypeDeclaration assemblyBuilder = search.FindType(typeof(AssemblyBuilder));
            ITypeDeclaration appDomain = search.FindType(typeof(AppDomain));
            IMethodDeclaration defineDynamicAssembly = Helper.GetMethod(appDomain, "DefineDynamicAssembly");
            ITypeDeclaration assemblyBuilderAccess = search.FindType(typeof(AssemblyBuilderAccess));
            IFieldDeclaration assemblyBuidlerAccessRunSave = search.FindField(assemblyBuilderAccess, "RunAndSave");

            this.formatter.WriteKeyword("public");
            this.formatter.Write(" ");
            this.formatter.WriteReference(
                "AssemblyBuilder",
                "",
                assemblyBuilder);
            this.formatter.Write(" BuildAssembly" + value.Name);
            this.formatter.Write("(");
            this.formatter.WriteReference(
                "AppDomain",
                "",
                appDomain);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("domain");
            this.formatter.Write(")");
            this.formatter.WriteLine();
            this.formatter.Write("{");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            this.WriteAssemblyReference(value);

            this.formatter.WriteReference("AssemblyBuilder", "", assemblyBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("assembly");
            this.formatter.Write(" = domain.");
            this.formatter.WriteReference("DefineDynamicAssembly", "", defineDynamicAssembly);
            this.formatter.Write("(assemblyName, ");
            this.formatter.WriteReference("AssemblyBuilderAccess", "", assemblyBuilderAccess);
            this.formatter.Write(".");
            this.formatter.WriteReference("RunAndSave", "", assemblyBuidlerAccessRunSave);
            this.formatter.Write(");");
            this.formatter.WriteLine();
            this.formatter.WriteKeyword("return");
            this.formatter.Write(" ");
            this.formatter.Write("assembly;");

            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
            this.formatter.Write("}");
            this.formatter.WriteLine();
        }

        public void WriteAssemblyReference(IAssemblyReference value)
        {
            ITypeDeclaration assemblyName = search.FindType(typeof(AssemblyName));
            ITypeDeclaration version = search.FindType(typeof(Version));
            IMethodDeclaration setPublicKey = Helper.GetMethod(assemblyName, "SetPublicKey");
            IMethodDeclaration setPublicKeyToken = Helper.GetMethod(assemblyName, "SetPublicKeyToken");
            IPropertyDeclaration versionP = search.FindProperty(assemblyName, "Version");

            this.formatter.WriteReference("AssemblyName", "", assemblyName);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("assemblyName");
            this.formatter.Write(" = ");
            this.formatter.WriteKeyword("new");
            this.formatter.Write(" ");
            this.formatter.WriteReference("AssemblyName", "", assemblyName);
            this.formatter.Write("(");
            this.formatter.WriteLiteral(value.Name);
            this.formatter.Write(");");
            this.formatter.WriteLine();

            if (value.PublicKey.Length != 0)
            {
                this.formatter.WriteComment("setting public key");
                WriteByteArray("publicKey", value.PublicKey);

                this.formatter.Write("assemblyName.");
                this.formatter.WriteReference("SetPublicKey", "", setPublicKey);
                this.formatter.Write("(publicKey);");
                this.formatter.WriteLine();
            }
            if (value.PublicKeyToken.Length != 0)
            {
                this.formatter.WriteComment("setting public key token");
                WriteByteArray("publicKeyToken", value.PublicKeyToken);

                this.formatter.Write("assemblyName.");
                this.formatter.WriteReference("SetPublicKeyToken", "", setPublicKeyToken);
                this.formatter.Write("(publicKeyToken);");
                this.formatter.WriteLine();
            }

            this.formatter.Write("assemblyName");
            this.formatter.Write(".");
            this.formatter.WriteReference("Version", "", versionP);
            this.formatter.Write(" = ");
            this.formatter.WriteKeyword("new");
            this.formatter.Write(" ");
            this.formatter.WriteReference("Version", "", version);
            this.formatter.Write(
                String.Format("({0},{1},{2},{3});",
                    value.Version.Major,
                    value.Version.Minor,
                    value.Version.Revision,
                    value.Version.Build
                    )
                );
            this.formatter.WriteLine();
        }

        private void WriteByteArray(string local, byte[] array)
        {
            ITypeDeclaration byteType = search.FindType(typeof(Byte));

            this.formatter.WriteReference("byte", "", byteType);
            this.formatter.Write("[] ");
            this.formatter.WriteDeclaration(local);
            this.formatter.Write(" = ");
            this.formatter.WriteKeyword("new");
            this.formatter.Write(" ");
            this.formatter.WriteReference("byte", "", byteType);
            this.formatter.Write("[]{");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            for (int i = 0; i < array.Length; ++i)
            {
                if (i != 0)
                    this.formatter.Write(", ");
                if ((i + 1) % 8 == 0)
                    this.formatter.WriteLine();
                this.formatter.Write(array[i].ToString());
            }

            this.formatter.WriteLine();
            this.formatter.Write("};");
            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
        }

        public void WriteEventDeclaration(IEventDeclaration value)
        {
            //TODO
            this.formatter.WriteDeclaration(value.ToString());
        }

        public void WriteExpression(IExpression value)
        {
            this.formatter.Write("Not supported by the Reflection.Emit language");
        }

        public void WriteFieldDeclaration(IFieldDeclaration value)
        {
            ITypeDeclaration fieldBuilder = this.search.FindType(typeof(FieldBuilder));
            ITypeDeclaration typeBuilder = this.search.FindType(typeof(TypeBuilder));
            IMethodDeclaration defineField = Helper.GetMethod(typeBuilder, "DefineField");
            ITypeDeclaration fieldAttributes = this.search.FindType(typeof(FieldAttributes));

            this.formatter.WriteKeyword("public");
            this.formatter.Write(" ");
            this.formatter.WriteReference("FieldBuilder", "", fieldBuilder);
            this.formatter.Write(" BuildField");
            this.formatter.Write(value.Name);
            this.formatter.Write("(");
            this.formatter.WriteReference(
                "TypeBuilder",
                "",
                typeBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("type");
            this.formatter.Write(")");
            this.formatter.WriteLine();
            this.formatter.Write("{");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            this.formatter.WriteReference("FieldBuilder", "", fieldBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("field");
            this.formatter.Write(" = type.");
            this.formatter.WriteReference("DefineField", "", defineField);
            this.formatter.Write("(");
            this.formatter.WriteIndent();
            this.formatter.WriteLine();
            this.formatter.WriteLiteral(value.Name);
            this.formatter.Write(", ");
            this.formatter.WriteLine();

            this.WriteTypeOf(value.FieldType);
            this.formatter.Write(", ");

            this.formatter.WriteLine();
            this.formatter.Write("  ");
            this.formatter.WriteReference("FieldAttributes", "", fieldAttributes);
            this.formatter.Write(".");
            switch (value.Visibility)
            {
                case FieldVisibility.Public:
                    this.formatter.WriteReference("Public", "", search.FindField(fieldAttributes, "Public"));
                    break;
                case FieldVisibility.Assembly:
                    this.formatter.WriteReference("Assembly", "", search.FindField(fieldAttributes, "Assembly"));
                    break;
                case FieldVisibility.Family:
                    this.formatter.WriteReference("Family", "", search.FindField(fieldAttributes, "Family"));
                    break;
                case FieldVisibility.FamilyAndAssembly:
                    this.formatter.WriteReference("FamANDAssem", "", search.FindField(fieldAttributes, "FamANDAssem"));
                    break;
                case FieldVisibility.FamilyOrAssembly:
                    this.formatter.WriteReference("FamORAssem", "", search.FindField(fieldAttributes, "FamORAssem"));
                    break;
                case FieldVisibility.Private:
                    this.formatter.WriteReference("Private", "", search.FindField(fieldAttributes, "Private"));
                    break;
                case FieldVisibility.PrivateScope:
                    this.formatter.WriteReference("PrivateScope", "", search.FindField(fieldAttributes, "PrivateScope"));
                    break;
            }
            if (value.Static)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                this.formatter.WriteReference("FieldAttributes", "", fieldAttributes);
                this.formatter.Write(".");
                this.formatter.WriteReference("Static", "", search.FindField(fieldAttributes, "Static"));
            }
            if (value.SpecialName)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                this.formatter.WriteReference("FieldAttributes", "", fieldAttributes);
                this.formatter.Write(".");
                this.formatter.WriteReference("SpecialName", "", search.FindField(fieldAttributes, "SpecialName"));
            }
            if (value.Literal)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                this.formatter.WriteReference("FieldAttributes", "", fieldAttributes);
                this.formatter.Write(".");
                this.formatter.WriteReference("Literal", "", search.FindField(fieldAttributes, "Literal"));
            }

            this.formatter.WriteLine();
            this.formatter.Write(");");
            this.formatter.WriteOutdent();
            this.formatter.WriteLine();

            this.formatter.WriteKeyword("return");
            this.formatter.Write(" field;");


            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
            this.formatter.Write("}");
            this.formatter.WriteLine();

        }

        public void WriteMethodDeclaration(IMethodDeclaration value)
        {
            this.operandLocals = new Hashtable();
            ITypeDeclaration methodBuilder = search.FindType(typeof(MethodBuilder));

            // public void Buildxxx(TypeBuilder type)
            // {
            DeclareMethod(value);
            this.formatter.Write("{");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            GenerateDefineMethod(value);
            GenerateGenericArguments(value);
            GenerateOperandLocals(value);
            GenerateCallingConvention(value);
            GenerateCustomAttributes(value, methodBuilder, "method");
            GenerateParameters(value);
            GenerateGetIlGenerator();

            IMethodBody body = value.Body as IMethodBody;
            if (body != null)
            {
                GenerateLocals(body);
                Hashtable labels = PrepareLabels(body);
                GenerateBody(body, labels);
            }

            this.formatter.WriteComment("finished");
            this.formatter.WriteKeyword("return");
            this.formatter.Write(" ");
            this.formatter.Write("method");
            this.formatter.Write(";");
            this.formatter.WriteLine();
            this.formatter.WriteOutdent();
            this.formatter.Write("}");
            this.formatter.WriteLine();
        }

        private void GenerateGenericArguments(IMethodDeclaration value)
        {
            if (value.GenericArguments.Count == 0)
                return;

            ITypeDeclaration genericTypeParameterBuilder = search.FindType("System.Reflection.Emit.GenericTypeParameterBuilder");
            ITypeDeclaration methodBuidler = search.FindType(typeof(MethodBuilder));
            ITypeDeclaration int32 = search.FindType(typeof(int));
            IMemberDeclaration defineGenericParameters = Helper.GetMethod(methodBuidler, "DefineGenericParameters");
            ITypeDeclaration genericParameterAttributes = search.FindType("System.Reflection.GenericParameterAttributes");
            IMethodDeclaration setGenericParameterAttributes = Helper.GetMethod(genericParameterAttributes, "SetGenericParameterAttributes");

            // first we definie the generic arguments
            this.formatter.WriteReference("GenericTypeParameterBuilder", "", genericTypeParameterBuilder);
            this.formatter.Write("[] ");
            this.formatter.WriteDeclaration("genericParameters");
            this.formatter.Write(" = ");
            this.formatter.Write("method");
            this.formatter.Write(".");
            this.formatter.WriteReference(
                "DefineGenericParameters",
                "",
                defineGenericParameters
                );
            this.formatter.Write("(");
            for (int i = 0; i < value.GenericArguments.Count; ++i)
            {
                IGenericParameter parameter = (IGenericParameter)value.GenericArguments[i];
                if (i != 0)
                    this.formatter.Write(", ");
                this.formatter.WriteLiteral(parameter.Name);
            }
            this.formatter.Write(");");
            this.formatter.WriteLine();

            // updating each generic parameter
            for (int i = 0; i < value.GenericArguments.Count; ++i)
            {
                IGenericParameter parameter = (IGenericParameter)value.GenericArguments[i];
                // GenericTypeParameterBuilder b;

                this.formatter.WriteComment("Generic parameter " + parameter.Name);
                this.formatter.Write("genericParameters[" + i.ToString() + "].");
                this.formatter.WriteReference("SetGenericParameterAttributes", "", setGenericParameterAttributes);
                this.formatter.Write("(");
                this.formatter.WriteReference("GenericParameterAttributes", "", genericParameterAttributes);
                this.formatter.Write(".");
                this.formatter.WriteReference(
                    parameter.Variance.ToString(),
                    "",
                    search.FindField(genericParameterAttributes, parameter.Variance.ToString())
                    );
                this.formatter.Write(");");
                this.formatter.WriteLine();
            }

            // base class constraint ?
            // interface constraint ?
            // constructor constraint ?

            this.formatter.WriteLine();
        }

        const string methodAttributesLocalName = "methodAttributes";
        private void GenerateMethodAttributesLocal(IMethodDeclaration value)
        {
            ITypeDeclaration methodAttributes = this.search.FindType(typeof(MethodAttributes));
            ITypeDeclaration methodBuilder = this.search.FindType(typeof(MethodBuilder));

            this.formatter.WriteComment("Method attributes");
            this.formatter.WriteReference(
                Helper.GetNameWithResolutionScope(methodAttributes),
                "",
                methodAttributes);
            this.formatter.Write(" ");
            this.formatter.Write(methodAttributesLocalName);
            this.formatter.Write(" = ");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();
            this.formatter.Write("  ");
            this.formatter.WriteReference(
                Helper.GetNameWithResolutionScope(methodAttributes),
                "",
                methodAttributes);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                value.Visibility.ToString(),
                "",
                search.FindField(methodAttributes, value.Visibility.ToString())
                );

            if (value.Abstract)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                WriteEnumValue(methodAttributes, "Abstract");
            }
            else if (value.Virtual)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                WriteEnumValue(methodAttributes, "Virtual");
            }
            if (value.Final)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                WriteEnumValue(methodAttributes, "Final");
            }
            if (value.HideBySignature)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                WriteEnumValue(methodAttributes, "HideBySig");
            }
            if (value.Static)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                WriteEnumValue(methodAttributes, "Static");
            }
            if (value.NewSlot)
            {
                this.formatter.WriteLine();
                this.formatter.Write("| ");
                WriteEnumValue(methodAttributes, "NewSlot");
            }

            this.formatter.Write(";");
            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
        }

        private void WriteEnumValue(ITypeDeclaration methodAttributes, string value)
        {
            this.formatter.WriteReference(
                Helper.GetNameWithResolutionScope(methodAttributes),
                "",
                methodAttributes);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                value,
                "",
                search.FindField(methodAttributes, value)
                );
        }

        private void GenerateCallingConvention(IMethodDeclaration value)
        {
        }

        private void GenerateCustomAttributes(
            Reflector.CodeModel.ICustomAttributeProvider value,
            ITypeDeclaration memberType,
            string ownerName)
        {
            if (value.Attributes.Count == 0)
                return;

            this.formatter.WriteComment("Adding custom attributes to " + ownerName);
            foreach (ICustomAttribute attribute in value.Attributes)
            {
                GenerateCustomAttribute(memberType, ownerName, attribute);
            }
        }

        private void GenerateCustomAttribute(
            ITypeDeclaration memberType,
            string ownerName,
            ICustomAttribute attribute)
        {
            ITypeDeclaration typeType = search.FindType(typeof(Type));
            ITypeDeclaration propertyInfo = search.FindType(typeof(PropertyInfo));
            ITypeDeclaration fieldInfo = search.FindType(typeof(FieldInfo));
            ITypeDeclaration objectType = search.FindType(typeof(Object));
            ITypeDeclaration customAttributeBuilder = search.FindType(typeof(CustomAttributeBuilder));
            IMethodDeclaration setCustomAttribute = Helper.GetMethod(memberType, "SetCustomAttribute");

            this.formatter.WriteComment(attribute.ToString());
            this.formatter.Write(ownerName);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                "SetCustomAttribute",
                "",
                setCustomAttribute);
            this.formatter.Write("(");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            this.formatter.WriteKeyword("new");
            this.formatter.Write(" ");
            this.formatter.WriteReference(
                "CustomAttributeBuilder",
                "",
                customAttributeBuilder);
            this.formatter.Write("(");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            string operand = (string)operandLocals[attribute.Constructor];
            this.formatter.Write(operand);
            this.formatter.Write(",");
            this.formatter.WriteLine();

            // we need to split the arguments in 
            // normal expression
            // and INamedArgumentExpression
            ArrayList regulars = new ArrayList();
            Hashtable fields = new Hashtable();
            Hashtable properties = new Hashtable();
            foreach (IExpression argument in attribute.Arguments)
            {
                IMemberInitializerExpression namedArgument = argument as IMemberInitializerExpression;
                if (namedArgument != null)
                {
                    if (namedArgument.Member is IFieldReference)
                        fields.Add(namedArgument.Member, namedArgument);
                    else if (namedArgument.Member is IPropertyReference)
                        properties.Add(namedArgument.Member, namedArgument);
                    else
                        throw new NotSupportedException(namedArgument.Member.ToString());
                }
                else
                    regulars.Add(argument);
            }

            // we create the array of regulars
            this.formatter.WriteKeyword("new");
            this.formatter.Write(" ");
            this.formatter.WriteReference(
                "Type",
                "",
                typeType);
            this.formatter.Write("[]{");
            this.formatter.WriteIndent();
            for (int i = 0; i < regulars.Count; ++i)
            {
                if (i != 0)
                {
                    this.formatter.Write(",");
                }
                this.formatter.WriteLine();
                IExpression expression = regulars[i] as IExpression;
                GenerateCustomAttributeArgumentExpression(expression);
            }
            if (regulars.Count > 0)
                this.formatter.WriteLine();
            this.formatter.Write("}");
            if (properties.Count > 0 || fields.Count > 0)
                this.formatter.Write(",");
            this.formatter.WriteOutdent();
            this.formatter.WriteLine();

            if (properties.Count > 0)
            {
                this.formatter.WriteComment("properties");
                this.formatter.WriteKeyword("new");
                this.formatter.Write(" ");
                this.formatter.WriteReference(
                    "PropertyInfo",
                    "",
                    propertyInfo);
                this.formatter.Write("[]{");
                this.formatter.WriteLine();
                this.formatter.WriteIndent();

                foreach (IPropertyReference property in properties.Keys)
                {
                    string local = (string)operandLocals[property];
                    this.formatter.Write(local);
                    this.formatter.Write(",");
                    this.formatter.WriteLine();
                }

                this.formatter.Write("},");
                this.formatter.WriteOutdent();
                this.formatter.WriteLine();


                this.formatter.WriteKeyword("new");
                this.formatter.Write(" ");
                this.formatter.WriteReference(
                    "Object",
                    "",
                    objectType);
                this.formatter.Write("[]{");
                this.formatter.WriteLine();
                this.formatter.WriteIndent();

                int ip = 0;
                foreach (IPropertyReference property in properties.Keys)
                {
                    if (ip != 0)
                    {
                        this.formatter.Write(",");
                        this.formatter.WriteLine();
                    }
                    IMemberInitializerExpression argument = (IMemberInitializerExpression)properties[property];
                    GenerateCustomAttributeArgumentExpression(argument.Value);
                    ip++;
                }
                this.formatter.WriteLine();
                this.formatter.Write("}");
                if (fields.Count > 0)
                    this.formatter.Write(",");
                this.formatter.WriteOutdent();
                this.formatter.WriteLine();
            }

            if (fields.Count > 0)
            {
                this.formatter.WriteComment("fields");
                this.formatter.WriteKeyword("new");
                this.formatter.Write(" ");
                this.formatter.WriteReference(
                    "FieldInfo",
                    "",
                    fieldInfo);
                this.formatter.Write("[]{");
                this.formatter.WriteLine();
                this.formatter.WriteIndent();

                foreach (IFieldReference field in fields.Keys)
                {
                    string local = (string)operandLocals[field];
                    this.formatter.Write(local);
                    this.formatter.Write(",");
                    this.formatter.WriteLine();
                }

                this.formatter.Write("},");
                this.formatter.WriteOutdent();
                this.formatter.WriteLine();

                this.formatter.WriteKeyword("new");
                this.formatter.Write(" ");
                this.formatter.WriteReference(
                    "Object",
                    "",
                    objectType);
                this.formatter.Write("[]{");
                this.formatter.WriteLine();
                this.formatter.WriteIndent();

                int ip = 0;
                foreach (IFieldReference field in fields.Keys)
                {
                    if (ip != 0)
                    {
                        this.formatter.Write(",");
                        this.formatter.WriteLine();
                    }
                    IMemberInitializerExpression argument = (IMemberInitializerExpression)fields[field];
                    GenerateCustomAttributeArgumentExpression(argument.Value);
                    ip++;
                }
                this.formatter.WriteLine();
                this.formatter.Write("}");
                this.formatter.WriteOutdent();
                this.formatter.WriteLine();
            }

            this.formatter.Write(")");
            this.formatter.WriteLine();
            this.formatter.WriteOutdent();
            this.formatter.Write(");");
            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
        }

        private void GenerateCustomAttributeArgumentExpression(IExpression expression)
        {
            ILiteralExpression literal = expression as ILiteralExpression;
            if (literal != null)
            {
                if (literal.Value is string)
                    this.formatter.WriteLiteral(literal.Value.ToString());
                else
                    this.formatter.Write(literal.Value.ToString());
            }
            else
            {
                ITypeOfExpression typeoff = expression as ITypeOfExpression;
                if (typeoff != null)
                {
                    this.WriteTypeOf(typeoff.Type);
                }
                else
                {
                    this.formatter.Write(expression.ToString());
                }
            }
        }

        private void GenerateDefineMethod(IMethodDeclaration value)
        {
            this.formatter.WriteComment("Declaring method builder");
            ITypeDeclaration methodBuilder = this.search.FindType(typeof(MethodBuilder));
            ITypeDeclaration typeBuilder = this.search.FindType(typeof(TypeBuilder));
            IMethodDeclaration defineMethod = Helper.GetMethod(typeBuilder, "DefineMethod");

            this.GenerateMethodAttributesLocal(value);

            this.formatter.WriteReference("MethodBuilder", "", methodBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("method");
            this.formatter.Write(" = ");
            this.formatter.Write("type");
            this.formatter.Write(".");
            this.formatter.WriteReference("DefineMethod", "", defineMethod);
            this.formatter.Write("(");
            this.formatter.WriteLiteral(value.Name);
            this.formatter.Write(", ");
            this.formatter.Write(methodAttributesLocalName);
            this.formatter.Write(");");
            this.formatter.WriteLine();
        }

        private void GenerateGetIlGenerator()
        {
            ITypeDeclaration ilGenerator = this.search.FindType(typeof(ILGenerator));
            ITypeDeclaration methodBuilder = this.search.FindType(typeof(MethodBuilder));
            IMethodDeclaration getILGenerator = Helper.GetMethod(methodBuilder, "GetILGenerator");

            this.formatter.WriteReference("ILGenerator", "", ilGenerator);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("gen");
            this.formatter.Write(" =  method.");
            this.formatter.WriteReference("GetILGenerator", "", getILGenerator);
            this.formatter.Write("();");
            this.formatter.WriteLine();
        }

        private Hashtable BuildEHOffsetTable(IMethodBody body)
        {
            Hashtable offsets = new Hashtable();
            Hashtable endBlocks = new Hashtable();
            foreach (IExceptionHandler eh in body.ExceptionHandlers)
            {
                offsets[eh.TryOffset] = eh;
                if (eh.Type == ExceptionHandlerType.Filter)
                    offsets[eh.FilterOffset] = eh;
                else
                    offsets[eh.HandlerOffset] = eh;

                IExceptionHandler end = endBlocks[eh.TryOffset] as IExceptionHandler;
                if (end == null)
                    endBlocks[eh.TryOffset] = eh;
                else if (end.HandlerOffset + end.HandlerLength < eh.HandlerOffset + eh.HandlerLength)
                    endBlocks[eh.TryOffset] = eh;
            }

            foreach (IExceptionHandler eh in endBlocks.Values)
                offsets[eh.HandlerOffset + eh.HandlerLength] = eh;

            return offsets;
        }

        private void GenerateBody(IMethodBody body, Hashtable labels)
        {
            ITypeDeclaration opCodes = this.search.FindType(typeof(OpCodes));
            ITypeDeclaration ilGenerator = this.search.FindType(typeof(ILGenerator));
            IMethodDeclaration markLabel = Helper.GetMethod(ilGenerator, "MarkLabel");
            IMethodDeclaration beginExceptionBlock = Helper.GetMethod(ilGenerator, "BeginExceptionBlock");
            IMethodDeclaration beginCatchBlock = Helper.GetMethod(ilGenerator, "BeginCatchBlock");
            IMethodDeclaration beginFinallyBlock = Helper.GetMethod(ilGenerator, "BeginFinallyBlock");
            IMethodDeclaration beginFaultBlock = Helper.GetMethod(ilGenerator, "BeginFaultBlock");
            IMethodDeclaration endExceptionBlock = Helper.GetMethod(ilGenerator, "EndExceptionBlock");
            IMethodDeclaration beginExceptFilterBlock = Helper.GetMethod(ilGenerator, "BeginExceptFilterBlock");

            Hashtable codes = BuildOpCodeTable(opCodes);
            Hashtable eh = BuildEHOffsetTable(body);

            this.formatter.WriteComment("Writing body");
            foreach (IInstruction instruction in body.Instructions)
            {
                // is this EH offset ?
                IExceptionHandler handler = eh[instruction.Offset] as IExceptionHandler;
                if (handler != null)
                {
                    if (handler.TryOffset == instruction.Offset)
                    {
                        this.formatter.Write("gen.");
                        this.formatter.WriteReference("BeginExceptionBlock", "", beginExceptionBlock);
                        this.formatter.Write("();");
                        this.formatter.WriteLine();
                    }
                    else
                    {
                        this.formatter.Write("gen.");
                        if (handler.Type == ExceptionHandlerType.Filter)
                        {
                            this.formatter.Write("FILTERS NOT SUPPORTED");
                        }
                        else
                        {
                            if (instruction.Offset == handler.HandlerOffset)
                            {
                                switch (handler.Type)
                                {
                                    case ExceptionHandlerType.Catch:
                                        this.formatter.WriteReference("BeginCatchBlock", "", beginCatchBlock);
                                        this.formatter.Write("(");
                                        this.WriteTypeOf(handler.CatchType);
                                        this.formatter.Write(");");
                                        break;
                                    case ExceptionHandlerType.Fault:
                                        this.formatter.WriteReference("BeginFaultBlock", "", beginFaultBlock);
                                        this.formatter.Write("();");
                                        break;
                                    case ExceptionHandlerType.Filter:
                                        this.formatter.WriteReference("BeginExceptFilterBlock", "", beginExceptFilterBlock);
                                        this.formatter.Write("();");
                                        break;
                                    case ExceptionHandlerType.Finally:
                                        this.formatter.WriteReference("BeginFinallyBlock", "", beginFinallyBlock);
                                        this.formatter.Write("();");
                                        break;
                                    default:
                                        throw new InvalidOperationException();
                                }
                            }
                            else
                            {
                                this.formatter.WriteReference("EndExceptionBlock", "", endExceptionBlock);
                                this.formatter.Write("();");
                            }
                        }
                        this.formatter.WriteLine();
                    }
                }

                // do we mark this instruction ? 
                string labelName = (string)labels[instruction.Offset];
                if (labelName != null)
                {
                    this.formatter.Write("gen.");
                    this.formatter.WriteReference("MarkLabel", "", markLabel);
                    this.formatter.Write("(" + labelName + ");");
                    this.formatter.WriteLine();
                }

                // emitting
                this.formatter.Write("gen.Emit(");
                this.formatter.WriteReference("OpCodes", "OpCodes", opCodes);
                this.formatter.Write(".");
                string fieldName = InstructionHelper.GetOpCodeFieldName(instruction.Code);

                this.formatter.WriteReference(
                        fieldName,
                        InstructionHelper.GetInstructionName(instruction.Code),
                        codes[fieldName]
                        );
                if (instruction.Value != null)
                {
                    this.formatter.Write(",");
                    string operand = (string)operandLocals[instruction.Value];

                    if (InstructionHelper.GetOperandType(instruction.Code) == Reflector.CodeModel.OperandType.ShortBranchTarget
                        || InstructionHelper.GetOperandType(instruction.Code) == Reflector.CodeModel.OperandType.BranchTarget)
                    {
                        string ln = (string)labels[instruction.Value];
                        this.formatter.Write(ln);
                    }
                    else if (operand != null)
                    {
                        this.formatter.Write(operand);
                    }
                    else if (instruction.Value is string)
                    {
                        this.formatter.WriteLiteral(instruction.Value.ToString());
                    }
                    else
                    {
                        this.formatter.Write(instruction.Value.ToString());
                    }
                }
                this.formatter.Write(");");
                this.formatter.WriteLine();
            }
        }

        private Hashtable PrepareLabels(IMethodBody body)
        {
            ITypeDeclaration label = this.search.FindType(typeof(Label));
            ITypeDeclaration ilGenerator = this.search.FindType(typeof(ILGenerator));
            IMethodDeclaration defineLabel = Helper.GetMethod(ilGenerator, "DefineLabel");

            Hashtable labels = new Hashtable();
            foreach (int offset in GetLabels(body))
            {
                if (!labels.ContainsKey(offset))
                {
                    if (labels.Count == 0)
                        this.formatter.WriteComment("Preparing labels");

                    string name = String.Format("label{0}", offset);
                    this.formatter.WriteReference("Label", "", label);
                    this.formatter.Write(" ");
                    this.formatter.WriteDeclaration(name);
                    this.formatter.Write(" =  gen.");
                    this.formatter.WriteReference("DefineLabel", "", defineLabel);
                    this.formatter.Write("();");
                    this.formatter.WriteLine();

                    labels.Add(offset, name);
                }
            }
            return labels;
        }

        private void GenerateLocals(IMethodBody body)
        {
            if (body.LocalVariables.Count == 0)
                return;

            ITypeDeclaration ilGenerator = this.search.FindType(typeof(ILGenerator));
            IMethodDeclaration declareLocal = Helper.GetMethod(ilGenerator, "DeclareLocal");
            ITypeDeclaration localBuilder = this.search.FindType(typeof(LocalBuilder));

            this.formatter.WriteComment("Preparing locals");
            foreach (IVariableDeclaration local in body.LocalVariables)
            {
                this.formatter.WriteReference("LocalBuilder", "", localBuilder);
                this.formatter.Write(" ");
                this.formatter.WriteDeclaration(local.Name);
                this.formatter.Write(" =  gen.");
                this.formatter.WriteReference("DeclareLocal", "", declareLocal);
                this.formatter.Write("(");
                this.WriteTypeOf(local.VariableType);
                this.formatter.Write(");");
                this.formatter.WriteLine();
            }
        }

        private void GenerateParameters(IMethodDeclaration value)
        {
            ITypeDeclaration parameterBuilder = this.search.FindType(typeof(ParameterBuilder));
            ITypeDeclaration methodBuilder = this.search.FindType(typeof(MethodBuilder));
            IMethodDeclaration defineParameter = Helper.GetMethod(methodBuilder, "DefineParameter");
            IMethodDeclaration setParameters = Helper.GetMethod(methodBuilder, "SetParameters");
            IMethodDeclaration setReturnType = Helper.GetMethod(methodBuilder, "SetReturnType");
            ITypeDeclaration parameterAttributes = this.search.FindType(typeof(ParameterAttributes));
            IFieldDeclaration parameterAttributeNone = this.search.FindField(parameterAttributes, "None");
            IFieldDeclaration parameterAttributeRetVal = this.search.FindField(parameterAttributes, "RetVal");


            this.formatter.WriteComment("Setting return type");
            this.formatter.Write("method");
            this.formatter.Write(".");
            this.formatter.WriteReference("SetReturnType", "", setReturnType);
            this.formatter.Write("(");
            this.WriteTypeOf(value.ReturnType.Type);
            this.formatter.Write(");");
            this.formatter.WriteLine();

            if (value.ReturnType.Attributes.Count > 0)
            {
                this.formatter.WriteComment("return value");
                // Defining return type attributes
                this.formatter.WriteReference("ParameterBuilder", "", parameterBuilder);
                this.formatter.Write(" ");
                this.formatter.WriteDeclaration("returnValue");
                this.formatter.Write(" =  method.");
                this.formatter.WriteReference("DefineParameter", "", defineParameter);
                this.formatter.Write("(0, ");
                this.formatter.WriteReference("ParameterAttributes", "", parameterAttributes);
                this.formatter.Write(".");
                this.formatter.WriteReference("RetVal", "", parameterAttributeRetVal);
                this.formatter.Write(", ");
                this.formatter.WriteLiteral("");
                this.formatter.Write(");");
                this.formatter.WriteLine();

                this.GenerateCustomAttributes(value.ReturnType, parameterBuilder, "returnValue");
            }

            this.formatter.WriteComment("Adding parameters");

            if (value.Parameters.Count != 0)
            {
                this.formatter.Write("method.");
                this.formatter.WriteReference("SetParameters", "", setParameters);
                this.formatter.Write("(");
                this.formatter.WriteIndent();
                for (int i = 0; i < value.Parameters.Count; ++i)
                {
                    if (i != 0)
                        this.formatter.Write(",");
                    this.formatter.WriteLine();

                    IParameterDeclaration parameter = value.Parameters[i];
                    this.WriteTypeOf(parameter.ParameterType);
                }
                this.formatter.WriteLine();
                this.formatter.Write(");");
                this.formatter.WriteOutdent();
                this.formatter.WriteLine();
            }

            int position = 1;
            foreach (IParameterDeclaration parameter in value.Parameters)
            {
                this.formatter.WriteComment("Parameter " + parameter.Name);
                this.formatter.WriteReference("ParameterBuilder", "", parameterBuilder);
                this.formatter.Write(" ");
                this.formatter.WriteDeclaration(parameter.Name);
                this.formatter.Write(" =  method.");
                this.formatter.WriteReference("DefineParameter", "", defineParameter);
                this.formatter.Write("(");
                this.formatter.Write(position.ToString() + ", ");
                this.formatter.WriteReference("ParameterAttributes", "", parameterAttributes);
                this.formatter.Write(".");
                this.formatter.WriteReference(
                    "None", "",
                    parameterAttributeNone);
                this.formatter.Write(", ");
                this.formatter.WriteLiteral(parameter.Name);
                this.formatter.Write(");");
                this.formatter.WriteLine();

                this.GenerateCustomAttributes(parameter, parameterBuilder, parameter.Name);

                position++;
            }
        }

        private void DeclareMethod(IMethodDeclaration value)
        {
            ITypeDeclaration methodBuilder = this.search.FindType(typeof(MethodBuilder));
            ITypeDeclaration typeBuilder = this.search.FindType(typeof(TypeBuilder));

            this.formatter.WriteKeyword("public");
            this.formatter.Write(" ");
            this.formatter.WriteReference(
                "MethodBuilder",
                "",
                methodBuilder);
            this.formatter.Write(" BuildMethod");
            this.formatter.Write(value.Name);
            this.formatter.Write("(");
            this.formatter.WriteReference(
                "TypeBuilder",
                "",
                typeBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("type");
            this.formatter.Write(")");
            this.formatter.WriteLine();
        }

        private void WriteTypeOf(IType type)
        {
            ITypeReference typeInstance = type as ITypeReference;
            if (typeInstance != null && typeInstance.GenericType != null)
            {
                ITypeDeclaration typeType = search.FindType(typeof(Type));
                IMethodDeclaration makeGenericType = Helper.GetMethod(typeType, "MakeGenericType");

                WriteTypeOf(typeInstance.GenericType);
                this.formatter.Write(".");
                this.formatter.WriteReference("MakeGenericType", "", makeGenericType);
                this.formatter.Write("(");
                for (int i = 0; i < typeInstance.GenericArguments.Count; ++i)
                {
                    if (i != 0)
                        this.formatter.Write(", ");
                    IType argument = typeInstance.GenericArguments[i];
                    this.WriteTypeOf(argument);
                }
                this.formatter.Write(")");
            }
            else
            {
                IGenericArgument argument = type as IGenericArgument;
                if (argument != null)
                {
                    this.formatter.Write("genericParameters[" + argument.Position.ToString() + "]");
                }
                else
                {

                    this.formatter.WriteKeyword("typeof");
                    this.formatter.Write("(");
                    ITypeReference typeReference = type as ITypeReference;
                    if (typeReference != null)
                    {
                        if (typeReference.GenericArguments.Count != 0)
                        {
                            this.formatter.WriteReference(
                                string.Format("{0}.{1}<>",
                                    typeReference.Namespace,
                                    typeReference.Name
                                    ),
                                "",
                                type);
                        }
                        else
                        {
                            this.formatter.WriteReference(
                                type.ToString(),
                                "",
                                type);
                        }
                    }
                    else
                    {
                        this.formatter.WriteReference(
                            String.Format("{0}", type),
                            "",
                            type
                            );
                    }
                    this.formatter.Write(")");
                }
            }
        }

        private void GenerateOperandLocals(IMethodDeclaration value)
        {
            this.formatter.WriteComment("Preparing Reflection instances");
            this.operandLocals = new Hashtable();
            int i = 1;

            foreach (ICustomAttribute attribute in value.Attributes)
            {
                i = DeclareCustomAttributeConstructor(i, attribute);
            }
            foreach (IParameterDeclaration parameter in value.Parameters)
            {
                foreach (ICustomAttribute attribute in parameter.Attributes)
                {
                    i = DeclareCustomAttributeConstructor(i, attribute);
                }
            }
            foreach (ICustomAttribute attribute in value.ReturnType.Attributes)
            {
                i = DeclareCustomAttributeConstructor(i, attribute);
            }

            IMethodBody body = value.Body as IMethodBody;
            if (body != null)
            {
                foreach (IInstruction instruction in body.Instructions)
                {
                    IMethodReference method = instruction.Value as IMethodReference;
                    if (method != null)
                    {
                        if (!operandLocals.ContainsKey(method))
                        {
                            if (method.Name == ".ctor")
                            {
                                string name = string.Format(
                                    "{0}{1}",
                                    method.Name.TrimStart('.')
                                    , i++);
                                operandLocals.Add(method, name);
                                DeclareConstructorInfoLocal(method, name);
                            }
                            else
                            {
                                string name = string.Format("method{0}", i++);
                                operandLocals.Add(method, name);
                                DeclareMethodInfoLocal(method, name);
                            }
                        }
                    }
                    else
                    {
                        IFieldReference field = instruction.Value as IFieldReference;
                        if (field != null)
                        {
                            if (!operandLocals.ContainsKey(field))
                            {
                                string name = string.Format("field{0}", i++);
                                operandLocals.Add(field, name);
                                DeclareFieldInfoLocal(field, name);
                            }
                        }
                        else
                        {
                            IEventReference eventt = instruction.Value as IEventReference;
                            if (eventt != null)
                            {
                                string name = string.Format("eventt{0}");
                                this.formatter.Write("EVENT NOT SUPPORTED");
                            }
                        }
                    }
                }
            }
        }

        private int DeclareCustomAttributeConstructor(int i, ICustomAttribute attribute)
        {
            if (!operandLocals.ContainsKey(attribute.Constructor))
            {
                string name = string.Format(
                    "{0}{1}",
                    attribute.Constructor.Name.TrimStart('.')
                    , i++);
                operandLocals.Add(attribute.Constructor, name);
                DeclareConstructorInfoLocal(attribute.Constructor, name);
            }

            foreach (IExpression arg in attribute.Arguments)
            {
                IMemberInitializerExpression named = arg as IMemberInitializerExpression;
                if (named != null)
                {
                    if (!operandLocals.ContainsKey(named.Member))
                    {
                        IFieldReference field = named.Member as IFieldReference;
                        if (field != null)
                        {
                            string name = string.Format("field{0}", i++);
                            operandLocals.Add(field, name);
                            DeclareFieldInfoLocal(field, name);
                        }
                        IPropertyReference property = named.Member as IPropertyReference;
                        if (property != null)
                        {
                            string name = string.Format("property{0}", i++);
                            operandLocals.Add(property, name);
                            DeclarePropertyInfoLocal(property, name);
                        }
                    }
                }
            }

            return i;
        }

        private void DeclarePropertyInfoLocal(IPropertyReference property, string name)
        {
            ITypeDeclaration propertyInfo = this.search.FindType(typeof(PropertyInfo));
            ITypeDeclaration bindingFlags = this.search.FindType(typeof(BindingFlags));
            IFieldDeclaration bindingFlagPublic = this.search.FindField(bindingFlags, "Public");
            IFieldDeclaration bindingFlagNonPublic = this.search.FindField(bindingFlags, "NonPublic");

            this.formatter.WriteReference(
                "PropertyInfo", "", propertyInfo);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration(name);
            this.formatter.Write(" = ");
            this.WriteTypeOf(property.DeclaringType);
            this.formatter.Write(".");
            this.formatter.WriteReference("GetProperty", "", null);
            this.formatter.Write("(");
            this.formatter.WriteLiteral(property.Name);
            this.formatter.Write(", ");
            this.formatter.WriteReference(
                 "BindingFlags", "", bindingFlags);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                    "Public", "", bindingFlagPublic);
            this.formatter.Write(" | ");
            this.formatter.WriteReference(
                "BindingFlags", "", bindingFlags);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                    "NonPublic", "", bindingFlagNonPublic);
            this.formatter.Write(", ");
            this.formatter.WriteKeyword("null");
            this.formatter.Write(", ");
            this.WriteTypeOf(property.PropertyType);
            this.formatter.Write(", ");
            this.formatter.WriteKeyword("null");
            this.formatter.Write(", ");
            this.formatter.WriteKeyword("null");
            this.formatter.Write(");");
            this.formatter.WriteLine();
        }

        private void DeclareFieldInfoLocal(IFieldReference field, string name)
        {
            ITypeDeclaration fieldInfo = this.search.FindType(typeof(FieldInfo));
            ITypeDeclaration bindingFlags = this.search.FindType(typeof(BindingFlags));
            IFieldDeclaration bindingFlagPublic = this.search.FindField(bindingFlags, "Public");
            IFieldDeclaration bindingFlagNonPublic = this.search.FindField(bindingFlags, "NonPublic");

            this.formatter.WriteReference("FieldInfo", "", fieldInfo);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration(name);
            this.formatter.Write(" = ");
            this.WriteTypeOf(field.DeclaringType);
            this.formatter.Write(".");
            this.formatter.WriteReference("GetField", "", null);
            this.formatter.Write("(");
            this.formatter.WriteLiteral(field.Name);
            this.formatter.Write(", ");
            this.formatter.WriteReference(
                 "BindingFlags", "", bindingFlags);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                    "Public", "", bindingFlagPublic);
            this.formatter.Write(" | ");
            this.formatter.WriteReference(
                "BindingFlags", "", bindingFlags);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                    "NonPublic", "", bindingFlagNonPublic);
            this.formatter.Write(");");
            this.formatter.WriteLine();
        }

        private void DeclareMethodInfoLocal(IMethodReference method, string name)
        {
            ITypeDeclaration methodInfo = this.search.FindType(typeof(MethodInfo));

            this.formatter.WriteReference("MethodInfo", "", methodInfo);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration(name);
            this.formatter.Write(" = ");
            this.WriteTypeOf(method.DeclaringType);
            this.formatter.Write(".");
            this.formatter.WriteReference("GetMethod", "", null);
            this.formatter.Write("(");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();
            {
                this.formatter.WriteLiteral(method.Name);
                this.formatter.Write(", ");
                this.formatter.WriteLine();
                GenerateMethodSignature(method);
            }
            this.formatter.Write(");");
            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
        }

        private void GenerateMethodSignature(IMethodReference method)
        {
            ITypeDeclaration type = this.search.FindType(typeof(Type));
            ITypeDeclaration bindingFlags = this.search.FindType(typeof(BindingFlags));
            IFieldDeclaration bindingFlagPublic = this.search.FindField(bindingFlags, "Public");
            IFieldDeclaration bindingFlagNonPublic = this.search.FindField(bindingFlags, "NonPublic");
            IFieldDeclaration bindingFlagStatic = this.search.FindField(bindingFlags, "Static");
            IFieldDeclaration bindingFlagInstance = this.search.FindField(bindingFlags, "Instance");

            // binding flags
            this.formatter.WriteReference(
                "BindingFlags", "", bindingFlags);
            this.formatter.Write(".");
            if (method.HasThis)
                this.formatter.WriteReference(
                    "Instance", "", bindingFlagInstance);
            else
                this.formatter.WriteReference(
                    "Static", "", bindingFlagStatic);
            this.formatter.Write(" | ");
            this.formatter.WriteReference(
                "BindingFlags", "", bindingFlags);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                    "Public", "", bindingFlagPublic);
            this.formatter.Write(" | ");
            this.formatter.WriteReference(
                "BindingFlags", "", bindingFlags);
            this.formatter.Write(".");
            this.formatter.WriteReference(
                    "NonPublic", "", bindingFlagNonPublic);
            this.formatter.Write(", ");
            this.formatter.WriteLine();
            this.formatter.WriteKeyword("null");
            this.formatter.Write(", ");
            this.formatter.WriteLine();

            this.formatter.WriteKeyword("new");
            this.formatter.Write(" ");
            this.formatter.WriteReference("Type", "", type);
            this.formatter.Write("[]{");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();
            for (int i = 0; i < method.Parameters.Count; ++i)
            {
                IParameterDeclaration parameter = method.Parameters[i];
                this.WriteTypeOf(parameter.ParameterType);
                if (i + 1 != method.Parameters.Count)
                    this.formatter.Write(",");
                this.formatter.WriteLine();
            }
            this.formatter.Write("}, ");
            this.formatter.WriteLine();
            this.formatter.WriteOutdent();
            this.formatter.WriteKeyword("null");
            this.formatter.WriteLine();
        }

        private void DeclareConstructorInfoLocal(
            IMethodReference constructor,
            string name)
        {
            ITypeDeclaration constructorInfo = this.search.FindType(typeof(ConstructorInfo));

            this.formatter.WriteReference("ConstructorInfo", "", constructorInfo);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration(name);
            this.formatter.Write(" = ");
            this.WriteTypeOf(constructor.DeclaringType);
            this.formatter.Write(".");
            this.formatter.WriteReference("GetConstructor", "", null);
            this.formatter.Write("(");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();
            {
                GenerateMethodSignature(constructor);
            }
            this.formatter.Write(");");
            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
        }

        private ArrayList GetLabels(IMethodBody body)
        {
            ArrayList list = new ArrayList();
            foreach (IInstruction instruction in body.Instructions)
            {
                if (InstructionHelper.GetOperandType(instruction.Code) == Reflector.CodeModel.OperandType.BranchTarget
                    || InstructionHelper.GetOperandType(instruction.Code) == Reflector.CodeModel.OperandType.ShortBranchTarget)
                    list.Add(instruction.Value);
            }
            return list;
        }

        private Hashtable BuildOpCodeTable(ITypeDeclaration opCodes)
        {
            Hashtable codes = new Hashtable(opCodes.Fields.Count);
            foreach (IFieldDeclaration field in opCodes.Fields)
            {
                string name = field.Name;
                codes.Add(name, field);
            }
            return codes;
        }

        public void WriteModule(IModule value)
        {
            ITypeDeclaration assemblyBuilder = search.FindType(typeof(AssemblyBuilder));
            ITypeDeclaration moduleBuilder = search.FindType(typeof(ModuleBuilder));
            IMethodDeclaration defineModule = Helper.GetMethod(assemblyBuilder, "DefineDynamicModule");

            this.formatter.WriteKeyword("public");
            this.formatter.Write(" ");
            this.formatter.WriteReference(
                "ModuleBuilder",
                "",
                moduleBuilder);
            this.formatter.Write(" BuildModule" + value.Name.Replace('.', '_'));
            this.formatter.Write("(");
            this.formatter.WriteReference(
                "AssemblyBuilder",
                "",
                assemblyBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("assembly");
            this.formatter.Write(")");
            this.formatter.WriteLine();
            this.formatter.Write("{");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            this.formatter.WriteReference("ModuleBuilder", "", moduleBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("module");
            this.formatter.Write(" = assembly.");
            this.formatter.WriteReference("DefineDynamicModule", "", defineModule);
            this.formatter.Write("(");
            this.formatter.WriteLiteral(value.Name);
            this.formatter.Write(");");
            this.formatter.WriteLine();
            this.formatter.WriteKeyword("return");
            this.formatter.Write(" ");
            this.formatter.Write("module;");

            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
            this.formatter.Write("}");
            this.formatter.WriteLine();
        }

        public void WriteModuleReference(IModuleReference value)
        {
            this.formatter.Write("Not suppored by the Reflection.Emit language.");
        }

        public void WriteNamespace(INamespace value)
        {
            this.formatter.Write("Not suppored by the Reflection.Emit language.");
        }

        public void WritePropertyDeclaration(IPropertyDeclaration value)
        {
            this.formatter.Write("Not suppored by the Reflection.Emit language.");
        }

        public void WriteResource(IResource value)
        {
            this.formatter.Write("Not suppored by the Reflection.Emit language.");
        }

        public void WriteStatement(IStatement value)
        {
            this.formatter.Write("Not suppored by the Reflection.Emit language.");
        }

        public void WriteTypeDeclaration(ITypeDeclaration value)
        {
            ITypeDeclaration moduleBuilder = this.search.FindType(typeof(ModuleBuilder));
            ITypeDeclaration typeBuilder = this.search.FindType(typeof(TypeBuilder));
            IMethodDeclaration defineType = Helper.GetMethod(moduleBuilder, "DefineType");
            ITypeDeclaration typeAttributes = this.search.FindType(typeof(TypeAttributes));

            this.formatter.WriteKeyword("public");
            this.formatter.Write(" ");
            this.formatter.WriteReference("TypeBuilder", "", typeBuilder);
            this.formatter.Write(" Build" + Helper.GetNameWithResolutionScope(value).Replace('.', '_'));
            this.formatter.Write("(");
            this.formatter.WriteReference("ModuleBuilder", "", moduleBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("module");
            this.formatter.Write(")");
            this.formatter.WriteLine();
            this.formatter.Write("{");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            this.formatter.WriteReference("TypeBuilder", "", typeBuilder);
            this.formatter.Write(" ");
            this.formatter.WriteDeclaration("type");
            this.formatter.Write(" = module.");
            this.formatter.WriteReference("DefineType", "", defineType);
            this.formatter.Write("(");
            this.formatter.WriteLine();
            this.formatter.WriteIndent();

            this.formatter.WriteLiteral(Helper.GetNameWithResolutionScope(value));
            this.formatter.Write(", ");
            this.formatter.WriteLine();
            this.formatter.WriteReference("TypeAttributes", "", typeAttributes);
            this.formatter.Write(".");
            this.formatter.WriteReference(value.Visibility.ToString(), "", search.FindField(typeAttributes, value.Visibility.ToString()));
            this.formatter.Write(", ");
            this.formatter.WriteLine();
            this.WriteTypeOf(value.BaseType);
            this.formatter.Write(", ");
            this.formatter.WriteLine();
            this.formatter.WriteKeyword("new");
            this.formatter.WriteReference("Type", "", search.FindType(typeof(Type)));
            this.formatter.Write("[]{");
            this.formatter.WriteIndent();
            for (int i = 0; i < value.Interfaces.Count; ++i)
            {
                if (i != 0)
                    this.formatter.Write(", ");
                this.formatter.WriteLine();
                this.WriteTypeOf(value.Interfaces[i]);
            }
            this.formatter.WriteLine();
            this.formatter.Write("}");
            this.formatter.WriteOutdent();

            this.formatter.WriteLine();
            this.formatter.Write(");");
            this.formatter.WriteOutdent();
            this.formatter.WriteLine();

            this.formatter.WriteKeyword("return");
            this.formatter.Write(" type;");

            this.formatter.WriteOutdent();
            this.formatter.WriteLine();
            this.formatter.Write("}");
        }
    }
}
