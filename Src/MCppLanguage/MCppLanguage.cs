namespace Reflector.Application.Languages
{
	using System;
	using System.Collections;
	using System.Globalization;
	using System.IO;
	using Reflector;
	using Reflector.CodeModel;
	using Reflector.CodeModel.Memory;

	internal class MCppLanguage : ILanguage
	{
		private bool addInMode;
		
		public MCppLanguage()
		{
			this.addInMode = false;
		}
		
		public MCppLanguage(bool addInMode)
		{
			this.addInMode = addInMode;
		}
		
		public string Name
		{
			get
			{
				return (!this.addInMode) ? "MC++" : "MC++ Add-In";
			}	
		}

		public string FileExtension
		{
			get
			{
				return ".cpp";	
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
	
		private class LanguageWriter : ILanguageWriter
		{
			private IFormatter formatter;
			private ILanguageWriterConfiguration configuration;
			
			private static Hashtable specialMethodNames = new Hashtable();
			private static Hashtable specialTypeNames = new Hashtable();
			// private static Hashtable keywords = new Hashtable();
			// private ArrayList ambiguousTypeNames = null;
			private bool statementLineBreak = true;
			private NumberFormat numberFormat;

			private enum NumberFormat
			{
				Auto,
				Hexadecimal,
				Decimal	
			}

			// [JLC]
			private IMethodDeclaration methodContext;

			public LanguageWriter(IFormatter formatter, ILanguageWriterConfiguration configuration)
			{
				this.formatter = formatter;
				this.configuration = configuration;

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
	
			static LanguageWriter()
			{
				specialTypeNames["Void"] = "void";
				//specialTypeNames["Object"] = "Object";
				//				specialTypeNames["String"] = "System::String";
				//				specialTypeNames["SByte"] = "sbyte";
				//				specialTypeNames["Byte"] = "System::Byte";
				//specialTypeNames["Int16"] = "short";
				//				specialTypeNames["UInt16"] = "ushort";
				//				specialTypeNames["Int32"] = "System::Int32";
				//				specialTypeNames["UInt32"] = "uint";
				//specialTypeNames["Int64"] = "long";
				//				specialTypeNames["UInt64"] = "ulong";
				// specialTypeNames["Char"] = "char";
				//specialTypeNames["Boolean"] = "bool";
				//specialTypeNames["Single"] = "float";
				//specialTypeNames["Double"] = "double";
				//				specialTypeNames["Decimal"] = "decimal";

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
				specialMethodNames["op_True"] = "true";
				specialMethodNames["op_False"] = "false";
				specialMethodNames["op_Implicit"] = "implicit";
				specialMethodNames["op_Explicit"] = "explicit";

				// [TODO] Include all keywords from C# language spec
				// [JLC]
				//				foreach(DictionaryEntry specialTypeName in specialTypeNames)
				//				{
				//					keywords[specialTypeName.Value] = specialTypeName.Key;
				//				}
				//				foreach(DictionaryEntry specialMethodName in specialMethodNames)
				//				{
				//					keywords[specialMethodName.Value] = specialMethodName.Value;
				//				}
				//				keywords["ref"] = "ref";
				//				keywords["out"] = "out";
				//				keywords["in"] = "in";
			}

			public void WriteAssembly(IAssembly value)
			{
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
					this.formatter.WriteLine();
					this.WriteCustomAttributeList(this.formatter, value);
					this.formatter.WriteLine();
				}

				this.formatter.WriteProperty("Location", value.Location);
				this.formatter.WriteProperty("Name", value.ToString());
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
					this.WriteCustomAttributeList(this.formatter, value);
					this.formatter.WriteLine();
				}

				this.formatter.WriteProperty("Version", value.Version.ToString());
				this.formatter.WriteProperty("Location", value.Location);

				string location = Environment.ExpandEnvironmentVariables(value.Location);
				if (File.Exists(location))
				{
					using (FileStream stream = new FileStream(location, FileMode.Open, FileAccess.Read))
					{
						this.formatter.WriteProperty("Size", stream.Length + " Bytes");
					}
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
					this.formatter.WriteProperty("Size", embeddedResource.Value.Length.ToString() + " bytes");
				}
	
				IFileResource fileResource = value as IFileResource;
				if (fileResource != null)
				{
					this.formatter.WriteProperty("Location", fileResource.Location);
				}
			}
	
			public void WriteNamespace(INamespace value)
			{
				string[] nameParts = value.Name.Split(new char[] { '.' });
				
				if (configuration["ShowNamespaceBody"] == "true")
				{
					for (int i = 0; i < nameParts.Length; i++)
					{
						this.formatter.WriteKeyword("namespace");
						this.formatter.Write(" ");
						this.formatter.WriteDeclaration(nameParts[i]);
						this.formatter.WriteLine();
						this.formatter.Write("{");
						this.formatter.WriteLine();
						this.formatter.WriteIndent();
					}	
				}
				else
				{
					for (int i = 0; i < nameParts.Length; i++)
					{
						this.formatter.WriteKeyword("namespace");
						this.formatter.Write(" ");
						this.formatter.WriteDeclaration(nameParts[i]);
						this.formatter.Write(" ");
						this.formatter.Write("{");
						this.formatter.Write(" ");
					}
				}
				
				if (configuration["ShowNamespaceBody"] == "true")
				{
					//					if (this.configuration.ShowNamespaceImports)
					//					{
					//						Performance.Start("UsingNamespaceVisitor");
					//						
					//						UsingNamespaceVisitor usingNamespaceVisitor = new UsingNamespaceVisitor();
					//						usingNamespaceVisitor.VisitNamespace(namespaceDeclaration);
					//	
					//						Performance.Stop("UsingNamespaceVisitor");
					//	
					//						bool lineBreak = false;
					//						foreach (string usingNamespace in usingNamespaceVisitor.Namespaces)
					//						{
					//							if ((usingNamespace != null) && (usingNamespace.Length != 0))
					//							{
					//								this.formatter.WriteKeyword("using");
					//								this.formatter.Write(" ");
					//								this.formatter.Write(usingNamespace);
					//								this.formatter.Write(";");
					//								this.formatter.WriteLine();
					//								lineBreak = true;
					//							}
					//						}
					//	
					//						if (lineBreak)
					//						{
					//							this.formatter.WriteLine();
					//						}
					//	
					//						ArrayList namespaces = new ArrayList();
					//						namespaces.Add(namespaceDeclaration.Name);
					//						namespaces.AddRange(usingNamespaceVisitor.Namespaces);
					//	
					//						Performance.Start("AmbiguousTypeVisitor");
					//	
					//						AmbiguousTypeVisitor ambiguousTypeVisitor = new AmbiguousTypeVisitor(namespaces);
					//						ambiguousTypeVisitor.VisitNamespace(namespaceDeclaration);
					//	
					//						Performance.Stop("AmbiguousTypeVisitor");
					//	
					//						this.ambiguousTypeNames = new ArrayList();
					//						this.ambiguousTypeNames.AddRange(ambiguousTypeVisitor.AmbiguousTypeNames);
					//					}
	
					ArrayList types = new ArrayList(0);
					foreach (ITypeDeclaration typeDeclaration in value.Types)
					{
						if (Helper.IsVisible(typeDeclaration, this.configuration.Visibility))
						{
							types.Add(typeDeclaration);						
						}
					}

					if (configuration["SortAlphabetically"] == "true")
					{
						types.Sort();
					}
	
					for (int i = 0; i < types.Count; i++)
					{
						if (i != 0)
						{
							formatter.WriteLine();
						}

						this.WriteTypeDeclaration((ITypeDeclaration)types[i]);
					}
				}

				if (configuration["ShowNamespaceBody"] == "true")
				{
					for (int i = 0; i < nameParts.Length; i++)
					{
						this.formatter.WriteOutdent();
						this.formatter.Write("}");
						this.formatter.WriteLine();
					}
				}
				else
				{
					for (int i = 0; i < nameParts.Length; i++)
					{
						this.formatter.Write(" ");
						this.formatter.Write("}");
						this.formatter.Write(" ");
					}
				}
			}

			public void WriteTypeDeclaration(ITypeDeclaration value)
			{
				this.WriteTypeDeclaration(value, false);
			}

			public void WriteTypeDeclaration(ITypeDeclaration value, bool isNested)
			{
				if ((configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
				{
					this.WriteCustomAttributeList(formatter, value);
					formatter.WriteLine();
				}
	
				switch (value.Visibility)
				{
					case TypeVisibility.Public:
					case TypeVisibility.NestedPublic:
						formatter.WriteKeyword("public");
						break;
	
					case TypeVisibility.Private:
					case TypeVisibility.NestedAssembly:
						formatter.WriteKeyword("private");
						break;
	
					case TypeVisibility.NestedPrivate:
						formatter.WriteKeyword("private");
						break;
	
					case TypeVisibility.NestedFamily:
						formatter.WriteKeyword("protected");
						break;
	
					case TypeVisibility.NestedFamilyAndAssembly:
						formatter.WriteKeyword("protected");
						formatter.Write(" ");
						formatter.WriteKeyword("private");
						break;
	
					case TypeVisibility.NestedFamilyOrAssembly:
						formatter.WriteKeyword("public");
						formatter.Write(" ");
						formatter.WriteKeyword("protected");
						break;
	
					default:
						throw new NotSupportedException();
				}

				if(isNested)
				{
					formatter.Write(": ");
					// formatter.WriteLine();
				}
				else
				{
					formatter.Write(" ");
				}
	
				if (Helper.IsDelegate(value))
				{
					formatter.WriteKeyword("__delegate");
					formatter.Write(" ");

					IMethodDeclaration invokeMethod = Helper.GetMethod(value, "Invoke");
	
					this.WriteType(invokeMethod.ReturnType.Type, formatter, false);
					formatter.Write(" ");
					formatter.WriteDeclaration(value.Name, value);
					this.WriteGenericArgumentList(value.GenericArguments, formatter);
					formatter.Write("(");
					this.WriteParameterDeclarationList(invokeMethod.Parameters, formatter, configuration);
					formatter.Write(")");
					this.WriteGenericParameterConstraintList(formatter, value.GenericArguments);
				}
				else if (Helper.IsEnumeration(value))
				{
					formatter.WriteKeyword("__value");
					formatter.Write(" ");
					formatter.WriteKeyword("enum");
					formatter.Write(" ");
					formatter.WriteDeclaration(value.Name, value);
				}
				else
				{
					bool colonPrinted = false;

					if (Helper.IsValueType(value))
					{
						formatter.WriteKeyword("__gc");
						formatter.Write(" ");
						formatter.WriteKeyword("struct");
						formatter.Write(" ");
						formatter.WriteDeclaration(value.Name, value);
						this.WriteGenericArgumentList(value.GenericArguments, formatter);
					}
					else if (value.Interface)
					{
						formatter.WriteKeyword("__gc");
						formatter.Write(" ");
						formatter.WriteKeyword("__interface");
						formatter.Write(" ");
						formatter.WriteDeclaration(value.Name, value);
						this.WriteGenericArgumentList(value.GenericArguments, formatter);
					}
					else
					{
						formatter.WriteKeyword("__gc");
						formatter.Write(" ");

						if (value.Abstract)
						{
							formatter.WriteKeyword("abstract");
							formatter.Write(" ");
						}

						if (value.Sealed)
						{
							formatter.WriteKeyword("sealed");
							formatter.Write(" ");
						}
	
						formatter.WriteKeyword("class");
						formatter.Write(" ");
						formatter.WriteDeclaration(value.Name, value);
						this.WriteGenericArgumentList(value.GenericArguments, formatter);
	
						ITypeReference baseType = value.BaseType;
						if ((baseType != null) && (!(IsType(baseType, "System", "Object"))))
						{
							formatter.Write(" : ");
							formatter.WriteKeyword("public");
							formatter.Write(" ");
							// [JLC]  HACK: Write as expresion?
							this.WriteType(baseType, formatter, true);
							colonPrinted = true;
						}
					}
	
					// TODO filter interfaces
					foreach (ITypeReference interfaceType in value.Interfaces)
					{
						formatter.Write(colonPrinted ? ", " : " : ");
						formatter.WriteKeyword("public");
						formatter.Write(" ");
						// [JLC]  HACK: Write as expresion?
						this.WriteType(interfaceType, formatter, true);
						colonPrinted = true;
					}
	
					this.WriteGenericParameterConstraintList(formatter, value.GenericArguments);
				}
	
				formatter.WriteProperty("Name", Helper.GetNameWithResolutionScope(value));
				this.WriteDeclaringAssembly(Helper.GetAssemblyReference(value), formatter);

				if (configuration["ShowTypeDeclarationBody"] == "true")
				{
					if (Helper.IsDelegate(value))
					{
						formatter.Write(";");
						formatter.WriteLine();
					}
					else
					{
						formatter.WriteLine();
						formatter.Write("{");
						formatter.WriteLine();
						formatter.WriteIndent();
						
						bool newLine = false;

						ICollection nestedTypes = Helper.GetNestedTypes(value, configuration.Visibility);
						if (nestedTypes.Count > 0)
						{
							if (newLine)
							{
								formatter.WriteLine();
							}
								
							newLine = true;
		
							formatter.WriteComment("// Nested Types");

							foreach (ITypeDeclaration nestedTypeDeclaration in nestedTypes)
							{
								formatter.WriteLine();
								this.WriteTypeDeclaration(nestedTypeDeclaration, true);
								formatter.WriteLine();
							}
						}

						ICollection events = Helper.GetEvents(value, configuration.Visibility);
						if (events.Count > 0)
						{
							if (newLine)
							{
								formatter.WriteLine();
							}
								
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
							{
								formatter.WriteLine();
							}
								
							newLine = true;
		
							formatter.WriteComment("// Methods");
							formatter.WriteLine();
		
							foreach (IMethodDeclaration methodDeclaration in methods)
							{
								this.WriteMethodDeclaration(methodDeclaration);
								formatter.WriteLine();

								if (this.configuration["ShowMethodDeclarationBody"] == "true")
								{
									formatter.WriteLine();
								}
							}
						}

						ICollection properties = Helper.GetProperties(value, configuration.Visibility);
						if (properties.Count > 0)
						{
							if (newLine)
							{
								formatter.WriteLine();
							}
								
							newLine = true;
		
							formatter.WriteComment("// Properties");
							formatter.WriteLine();
							
							foreach (IPropertyDeclaration propertyDeclaration in properties)
							{
								this.WritePropertyDeclaration(propertyDeclaration);
								formatter.WriteLine();

								if (this.configuration["ShowMethodDeclarationBody"] == "true")
								{
									formatter.WriteLine();
								}
							}
						}

						IDictionary eventLookup = new Hashtable();
						foreach (IEventDeclaration eventDeclaration in value.Events)
						{
							eventLookup[eventDeclaration.Name] = eventDeclaration;
						}

						// Filter "value__" element.
						ArrayList fields = new ArrayList();
						foreach (IFieldDeclaration fieldDeclaration in Helper.GetFields(value, configuration.Visibility))
						{
							if ( (!fieldDeclaration.SpecialName) && (fieldDeclaration.Name != "value__")
								&& eventLookup[fieldDeclaration.Name] == null )	// [JLC]
							{
								fields.Add(fieldDeclaration);
							}					
						}
						
						if (fields.Count > 0)
						{
							if (newLine)
							{
								formatter.WriteLine();
							}
								
							newLine = true;
	
							formatter.WriteComment("// Fields");
							formatter.WriteLine();
	
							for (int i = 0; i < fields.Count; i++)
							{
								IFieldDeclaration fieldDeclaration = (IFieldDeclaration) fields[i];
								this.WriteFieldDeclaration(fieldDeclaration);

								if ((Helper.IsEnumeration(value)) && (i != (fields.Count - 1)))
								{
									formatter.Write(",");
								}
			
								formatter.WriteLine();
							}
						}
	
						//						ICollection nestedTypes = information.GetNestedTypes(configuration.Visibility, configuration);
						//						if (nestedTypes.Count > 0)
						//						{
						//							if (newLine)
						//							{
						//								formatter.WriteLine();
						//							}
						//								
						//							newLine = true;
						//		
						//							formatter.WriteComment("// Nested Types");
						//
						//							foreach (ITypeDeclaration nestedTypeDeclaration in nestedTypes)
						//							{
						//								formatter.WriteLine();
						//								this.WriteTypeDeclaration(nestedTypeDeclaration);
						//								formatter.WriteLine();
						//							}
						//						}
		
						formatter.WriteOutdent();
						formatter.Write("};");
						formatter.WriteLine();
					}
				}
				else
				{
					formatter.WriteLine();
				}
			}

			public void WriteFieldDeclaration(IFieldDeclaration value)
			{
				if ((this.configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
				{
					this.WriteCustomAttributeList(this.formatter, value);
					this.formatter.WriteLine();
				}
	
				if (!this.IsEnumerationElement(value))
				{
					switch (value.Visibility)
					{
						case FieldVisibility.Public:
							this.formatter.WriteKeyword("public");
							break;
	
						case FieldVisibility.Private:
							this.formatter.WriteKeyword("private");
							break;
	
						case FieldVisibility.PrivateScope:
							this.formatter.WriteComment("/* private scope */");
							break;
	
						case FieldVisibility.Assembly:
							formatter.WriteKeyword("public");
							formatter.Write(" ");
							formatter.WriteKeyword("private");
							break;
	
						case FieldVisibility.Family:
							this.formatter.WriteKeyword("protected");
							break;
	
						case FieldVisibility.FamilyOrAssembly:
							formatter.WriteKeyword("public");
							formatter.Write(" ");
							formatter.WriteKeyword("protected");
							break;
	
						case FieldVisibility.FamilyAndAssembly:
							this.formatter.WriteKeyword("protected private");
							break;
	
						default:
							throw new NotSupportedException();
					}

					this.formatter.Write(": ");
					// this.formatter.WriteLine();

					if ((value.Static) && (value.Literal))
					{
						this.formatter.WriteKeyword("const");
						this.formatter.Write(" ");
					}
					else
					{
						if (value.Static)
						{
							this.formatter.WriteKeyword("static");
							this.formatter.Write(" ");
						}
	
						if (value.ReadOnly)
						{
							this.formatter.Write(" ");
							this.formatter.WriteComment("/* readonly */");
							this.formatter.Write(" ");
						}
					}

					// [JLC]
					//this.WriteType(value.FieldType, formatter);
					// this.formatter.WriteDeclaration(value.Name);
					// this.formatter.Write(" ");
					this.WriteType(value.FieldType, formatter, value.Name);
				}
				else
				{
					this.formatter.WriteDeclaration(value.Name, value);
				}

				//				// [JLC]
				//				IArrayType arrayType = value.FieldType as IArrayType;
				//				if (arrayType != null)
				//				{
				//					WriteArrayTypePostfix(arrayType);
				//				}

				IExpression initializer = value.Initializer;
				if (initializer != null)
				{
					formatter.Write(" = ");
					this.WriteExpression(initializer, formatter);
				}
	
				if (!this.IsEnumerationElement(value))
				{
					formatter.Write(";");
				}
				
				this.WriteDeclaringType(formatter, value.DeclaringType as ITypeReference);
			}

			public void WriteMethodDeclaration(IMethodDeclaration value)
			{
				this.methodContext = value;
				this.statementLineBreak = true;

				if ((configuration["ShowCustomAttributes"] == "true") && (value.ReturnType.Attributes.Count != 0))
				{
					this.WriteCustomAttributeList(formatter, value.ReturnType);
					formatter.WriteLine();
				}

				if ((configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
				{
					this.WriteCustomAttributeList(formatter, value);
					formatter.WriteLine();
				}
	
				if (this.TryWriteMethodFinalize(value, formatter, configuration))
				{
					return;
				}
	
				this.WriteMethodAttributes(formatter, value);
	
				if (this.GetCustomAttribute(value, "System.Runtime.InteropServices", "DllImportAttribute") != null)
				{
					formatter.WriteKeyword("extern");
					formatter.Write(" ");
				}

				if (this.IsConstructor(value))
				{
					formatter.WriteDeclaration((value.DeclaringType as ITypeReference).Name, value);
					WriteMethodDeclarationMiddle(formatter, value);
				}
					//				else if ((methodDeclaration.IsSpecialName) && (specialMethodNames.Contains(methodName)))
					//				{
					//					methodName = (string)specialMethodNames[methodName];
					//					if ((methodName == "explicit") || (methodName == "implicit"))
					//					{
					//						// TODO custom attributes [return: ...]
					//						formatter.WriteDeclaration(methodName);
					//						formatter.Write(" ");
					//						formatter.WriteDeclaration("operator");
					//						formatter.Write(" ");
					//						this.WriteType(methodDeclaration.ReturnType.Type, formatter);
					//					}
					//					else
					//					{
					//						// TODO cusotm attribtues[return: ...]
					//						this.WriteType(methodDeclaration.ReturnType.Type, formatter);
					//						formatter.Write(" ");
					//						formatter.WriteDeclaration("operator");
					//						formatter.Write(" ");
					//						formatter.WriteDeclaration(methodName);
					//					}
					//				}
				else
				{
					// TODO custom attributes [return: ...]
					this.WriteType(value.ReturnType.Type, formatter, false, new WriteTypeMiddleCallback(WriteMethodDeclarationMiddle), value);
				}

				this.WriteDeclaringType(formatter, value.DeclaringType as ITypeReference);

				this.WriteMethodBody(value.Body as IBlockStatement);
			}

			private void WriteMethodDeclarationMiddle(IFormatter formatter, object methodDeclarationObject)
			{
				IMethodDeclaration value = methodDeclarationObject as IMethodDeclaration;

				if (!this.IsConstructor(value))
				{
					string methodName = value.Name;	

					if (this.IsExplicitInterfaceImplementation(value))
					{
						methodName = value.Name.Replace(".", "::");
					}

					formatter.WriteDeclaration(methodName, value);
				}

				// Generic Parameters
				this.WriteGenericArgumentList(value.GenericArguments, formatter);
	
				// Method Parameters
				formatter.Write("(");
				this.WriteParameterDeclarationList(value.Parameters, formatter, configuration);
				if (value.CallingConvention == MethodCallingConvention.VariableArguments)
				{
					formatter.WriteKeyword(", __arglist");
				}
	
				formatter.Write(")");
				this.WriteGenericParameterConstraintList(formatter, value.GenericArguments);
	
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
							formatter.Write(" : ");
							this.WriteExpression(methodReferenceExpression.Target, formatter);
							formatter.Write("(");
							this.WriteExpressionList(constructorDeclaration.Initializer.Arguments, formatter);
							formatter.Write(")");
						}
					}
				}
			}

			// [JLC]
			private bool IsExplicitInterfaceImplementation(IMethodDeclaration methodDeclaration)
			{
				return methodDeclaration.Name.IndexOf('.', 1) != -1;
			}

			private bool TryWriteMethodFinalize(IMethodDeclaration value, IFormatter formatter, ILanguageWriterConfiguration configuration)
			{
				IBlockStatement aBody = value.Body as IBlockStatement;
				if ((aBody != null) && (aBody.Statements.Count == 1))
				{
					if ((value.Name == "Finalize") && (value.Visibility == MethodVisibility.Family))
					{
						ITryCatchFinallyStatement tryCatchFinallyStatement = aBody.Statements[0] as ITryCatchFinallyStatement;
						if ((tryCatchFinallyStatement != null) && (tryCatchFinallyStatement.CatchClauses.Count == 0) && (tryCatchFinallyStatement.Finally != null) && (tryCatchFinallyStatement.Finally.Statements.Count == 1))
						{
							IExpressionStatement expressionStatement = tryCatchFinallyStatement.Finally.Statements[0] as IExpressionStatement;
							if (expressionStatement != null)
							{
								IMethodInvokeExpression methodInvokeExpression = expressionStatement.Expression as IMethodInvokeExpression;
								if ((methodInvokeExpression != null) && (methodInvokeExpression.Arguments.Count == 0))
								{
									IMethodReferenceExpression methodReferenceExpression = methodInvokeExpression.Method as IMethodReferenceExpression;
									if (methodReferenceExpression != null)
									{
										IBaseReferenceExpression baseReferenceExpression = methodReferenceExpression.Target as IBaseReferenceExpression;
										if (baseReferenceExpression != null)
										{
											if (methodReferenceExpression.Method.Name == "Finalize")
											{
												formatter.Write("~");
												formatter.WriteDeclaration((value.DeclaringType as ITypeReference).Name, value);
												formatter.Write("(");
												formatter.Write(")");
												this.WriteMethodBody(tryCatchFinallyStatement.Try);
												return true;
											}
										}
									}
								}
							}
						}
					}
				}
	
				return false;
			}
			
			private void WriteMethodBody(IBlockStatement statement)
			{
				if ((configuration["ShowMethodDeclarationBody"] == "true") && (statement != null))
				{
					formatter.WriteLine();
					formatter.Write("{");
					formatter.WriteLine();
					formatter.WriteIndent();
					this.WriteStatement(statement, formatter);
					formatter.WriteOutdent();
					formatter.Write("}");
				}
				else
				{
					formatter.Write(";");
				}
			}
	
			public void WritePropertyDeclaration(IPropertyDeclaration value)
			{
				if ((configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
				{
					this.WriteCustomAttributeList(formatter, value);
					formatter.WriteLine ();
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

				if (getMethod != null)
				{
					this.WriteMethodDeclaration(getMethod);

					if (setMethod != null)
					{
						formatter.WriteLine();
					}
				}

				if (setMethod != null)
				{
					this.WriteMethodDeclaration(setMethod);
				}

				//				bool hasSameAttributes = true;
				//				if ((getMethod != null) && (setMethod != null))
				//				{
				//					hasSameAttributes &= (getMethod.Visibility == setMethod.Visibility);
				//					hasSameAttributes &= (getMethod.IsStatic == setMethod.IsStatic);
				//					hasSameAttributes &= (getMethod.IsFinal == setMethod.IsFinal);
				//					hasSameAttributes &= (getMethod.IsVirtual == setMethod.IsVirtual);
				//					hasSameAttributes &= (getMethod.IsAbstract == setMethod.IsAbstract);
				//					hasSameAttributes &= (getMethod.IsNewSlot == setMethod.IsNewSlot);
				//				}
				//	
				//				if (hasSameAttributes)
				//				{
				//					if (getMethod != null)
				//					{
				//						this.WriteMethodAttributes(formatter, getMethod);
				//					}
				//					else if (setMethod != null)
				//					{
				//						this.WriteMethodAttributes(formatter, setMethod);
				//					}
				//				}
				//	
				//				// PropertyType
				//				this.WriteType(value.PropertyType, formatter);
				//				formatter.Write (" ");
				//	
				//				// Name
				//				string propertyName = value.Name;
				//				if (propertyName == "Item")
				//				{
				//					propertyName = "this";
				//				}
				//	
				//				formatter.WriteDeclaration(propertyName);
				//	
				//				IParameterDeclarationCollection parameters = value.Parameters;
				//				if (parameters.Count > 0)
				//				{
				//					formatter.Write("[");
				//					this.WriteParameterDeclarationList(parameters, formatter, configuration);
				//					formatter.Write("]");
				//				}
				//	
				//				if (value.Initializer != null)
				//				{
				//					formatter.Write(" = ");
				//					this.WriteExpression(value.Initializer, formatter);
				//				}
				//	
				//	
				//				bool hasBody = (((getMethod != null) && (getMethod.Body != null)) || ((setMethod != null) && (setMethod.Body != null)));
				//	
				//				if (hasBody)
				//				{
				//					this.formatter.WriteLine();
				//					this.formatter.Write("{");
				//					this.formatter.WriteLine();
				//					this.formatter.WriteIndent();
				//				}
				//				else
				//				{
				//					this.formatter.Write(" ");
				//					this.formatter.Write("{");
				//				}
				//	
				//				if (getMethod != null)
				//				{
				//					if (!hasBody)
				//					{
				//						formatter.Write(" ");
				//					}
				//	
				//					if (!hasSameAttributes)
				//					{
				//						this.WriteMethodAttributes(formatter, getMethod);
				//					}
				//	
				//					formatter.WriteKeyword("get");
				//					
				//					if (getMethod.Body != null)
				//					{
				//						formatter.WriteLine();
				//						formatter.Write("{");
				//						formatter.WriteLine();
				//						formatter.WriteIndent();
				//						this.WriteStatement(getMethod.Body, formatter);
				//						formatter.WriteOutdent();
				//						formatter.Write("}");
				//						formatter.WriteLine();					
				//					}
				//					else
				//					{
				//						formatter.Write(";");					
				//					}
				//				}
				//	
				//				if (setMethod != null)
				//				{
				//					if (!hasBody)
				//					{
				//						formatter.Write(" ");
				//					}
				//	
				//					if (!hasSameAttributes)
				//					{
				//						this.WriteMethodAttributes (formatter, setMethod);
				//					}
				//	
				//					formatter.WriteKeyword("set");
				//	
				//					if (setMethod.Body != null)
				//					{
				//						formatter.WriteLine();
				//						formatter.Write("{");
				//						formatter.WriteLine();
				//						formatter.WriteIndent();
				//						this.WriteStatement(setMethod.Body, formatter);
				//						formatter.WriteOutdent();
				//						formatter.Write("}");
				//						formatter.WriteLine();
				//					}
				//					else
				//					{
				//						formatter.Write(";");					
				//					}
				//				}
				//	
				//				if (hasBody)
				//				{
				//					formatter.WriteOutdent();
				//					formatter.Write("}");
				//				}
				//				else
				//				{
				//					formatter.Write (" ");
				//					formatter.Write ("}");
				//				}
				//	
				//				this.WriteDeclaringType(formatter, value.DeclaringType as ITypeReference);
			}
	
			public void WriteEventDeclaration(IEventDeclaration value)
			{
				if ((this.configuration["ShowCustomAttributes"] == "true") && (value.Attributes.Count != 0))
				{
					this.WriteCustomAttributeList(this.formatter, value);
					this.formatter.WriteLine();
				}

				ITypeDeclaration declaringType = (value.DeclaringType as ITypeReference).Resolve();
				if (!declaringType.Interface)
				{
					switch (Helper.GetVisibility(value))
					{
						case MethodVisibility.Public:
							formatter.WriteKeyword("public");
							break;

						case MethodVisibility.Private:
							formatter.WriteKeyword("private");
							break;

						case MethodVisibility.PrivateScope:
							formatter.WriteComment("/* private scope */");
							break;

						case MethodVisibility.Family:
							formatter.WriteKeyword("protected");
							break;

						case MethodVisibility.Assembly:
							formatter.WriteKeyword("public");
							formatter.Write(" ");
							formatter.WriteKeyword("private");
							break;

						case MethodVisibility.FamilyOrAssembly:
							formatter.WriteKeyword("public");
							formatter.Write(" ");
							formatter.WriteKeyword("protected");
							break;

						case MethodVisibility.FamilyAndAssembly:
							formatter.WriteKeyword("protected");
							formatter.Write(" ");
							formatter.WriteKeyword("private");
							break;

						default:
							throw new NotSupportedException();
					}

					formatter.Write(": ");
					// formatter.WriteLine();
				}

				if (Helper.IsStatic(value))
				{
					formatter.WriteKeyword("static ");
					formatter.Write(" ");
				}
	
				formatter.WriteKeyword("__event");
				formatter.Write(" ");
				this.WriteType(value.EventType, formatter, false);
				formatter.Write(" ");
				formatter.WriteDeclaration(value.Name, value);
				formatter.Write(";");

				this.WriteDeclaringType(formatter, value.DeclaringType as ITypeReference);
			}

			// [JLC]
			private void WriteType(IType type, IFormatter formatter, string fieldDeclarationName)
			{
				this.WriteType(type, formatter, false, new WriteTypeMiddleCallback(WriteName), fieldDeclarationName);
			}

			private void WriteName(IFormatter formatter, object name)
			{
				formatter.Write( (string)name );
			}

			// [JLC]
			private void WriteType(IType type, IFormatter formatter, bool isExpression)
			{
				this.WriteType(type, formatter, isExpression, null, null);
			}

			// Callback to write between type and array information.
			private delegate void WriteTypeMiddleCallback(IFormatter formatter, object middle);

			private void WriteType(IType type, IFormatter formatter, bool isExpression, WriteTypeMiddleCallback callback, object middle)
			{
				ITypeReference typeReference = type as ITypeReference;
				if (typeReference != null)
				{
					string description = Helper.GetNameWithResolutionScope(typeReference);
					this.WriteTypeReference(formatter, typeReference, description, typeReference);

					// [JLC]
					if (!isExpression)
					{
						formatter.Write(" ");
						formatter.WriteKeyword("__gc");
						formatter.Write("*");
					}

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					return;
				}

				// [JLC]
				IArrayType arrayType = type as IArrayType;
				if (arrayType != null)
				{
					this.WriteType(arrayType.ElementType, formatter, false);

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					formatter.Write(" ");
					formatter.WriteKeyword("__gc");
					formatter.Write(" ");
					formatter.Write("[");
	
					IArrayDimensionCollection dimensions = arrayType.Dimensions;
	
					for (int i = 0; i < dimensions.Count; i++)
					{
						if (i != 0)
						{
							formatter.Write(",");
						}
	
						if ((dimensions[i].LowerBound != 0) && (dimensions[i].UpperBound != -1))
						{
							if ((dimensions[i].LowerBound != -1) || (dimensions[i].UpperBound != -1))
							{
								formatter.Write((dimensions[i].LowerBound != -1) ? dimensions[i].LowerBound.ToString () : ".");
								formatter.Write("..");
								formatter.Write((dimensions[i].UpperBound != -1) ? dimensions[i].UpperBound.ToString () : ".");
							}
						}
					}
	
					formatter.Write("]");

					return;
				}
	
				IPointerType pointerType = type as IPointerType;
				if (pointerType != null)
				{
					this.WriteType(pointerType.ElementType, formatter, false);
					formatter.Write("*");

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					return;
				}
	
				IReferenceType referenceType = type as IReferenceType;
				if (referenceType != null)
				{
					this.WriteType(referenceType.ElementType, formatter, false);
					// [JLC]
					formatter.Write("&");

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					return;
				}
	
				//IPinnedType pinnedType = type as IPinnedType;
				IVariableDeclaration pinnedType = type as IVariableDeclaration;
				if (pinnedType != null && pinnedType.Pinned)
				{
					formatter.WriteKeyword("pinned");
					formatter.Write(" ");
					this.WriteType(pinnedType.VariableType, formatter, false);

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					return;
				}
	
				IOptionalModifier optionalModifier = type as IOptionalModifier;
				if (optionalModifier != null)
				{
					this.WriteType(optionalModifier.ElementType, formatter, false);
					formatter.Write(" ");
					formatter.WriteKeyword("modopt");
					formatter.Write("(");
					this.WriteType(optionalModifier.Modifier, formatter, false);
					formatter.Write(")");

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					return;
				}
	
				IRequiredModifier requiredModifier = type as IRequiredModifier;
				if (requiredModifier != null)
				{
					ITypeReference modifier = requiredModifier.Modifier as ITypeReference;
					if ((modifier != null) && (IsType(modifier, "System.Runtime.CompilerServices", "IsVolatile")))
					{
						formatter.WriteKeyword("volatile");
						formatter.Write(" ");
						this.WriteType(requiredModifier.ElementType, formatter, false);
					}
					else
					{
						this.WriteType(requiredModifier.ElementType, formatter, false);
						formatter.Write(" ");
						formatter.WriteKeyword("modreq");
						formatter.Write("(");
						this.WriteType(requiredModifier.Modifier, formatter, false);
						formatter.Write(")");
					}

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}
	
					return;
				}
	
				IFunctionPointer functionPointer = type as IFunctionPointer;
				if (functionPointer != null)
				{
					this.WriteType(functionPointer.ReturnType.Type, formatter, false);
					formatter.Write(" *(");
					for (int i = 0; i < functionPointer.Parameters.Count; i++)
					{
						if (i != 0)
						{
							formatter.Write(", ");
						}
	
						this.WriteType(functionPointer.Parameters[i].ParameterType, formatter, false);
					}
	
					formatter.Write(")");

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					return;
				}
	
				IGenericParameter genericParameter = type as IGenericParameter;
				if (genericParameter != null)
				{
					formatter.Write(genericParameter.Name);

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					return;
				}
	
				IGenericArgument genericArgument = type as IGenericArgument;
				if (genericArgument != null)
				{
					this.WriteType(genericArgument.Resolve(), formatter, false);

					if (callback != null)
					{
						formatter.Write(" ");
						callback(formatter, middle);
					}

					return;
				}
	
				throw new NotSupportedException ();
			}
/*
			// JLC
			private void WriteArrayTypePostfix(IArrayType arrayType)
			{
				formatter.WriteKeyword("__gc");
				formatter.Write(" ");
				formatter.Write("[");
	
				IArrayDimensionCollection dimensions = arrayType.Dimensions;
	
				for (int i = 0; i < dimensions.Count; i++)
				{
					if (i != 0)
					{
						formatter.Write(",");
					}
	
					if ((dimensions[i].LowerBound != 0) && (dimensions[i].UpperBound != -1))
					{
						if ((dimensions[i].LowerBound != -1) || (dimensions[i].UpperBound != -1))
						{
							formatter.Write((dimensions[i].LowerBound != -1) ? dimensions[i].LowerBound.ToString () : ".");
							formatter.Write("..");
							formatter.Write((dimensions[i].UpperBound != -1) ? dimensions[i].UpperBound.ToString () : ".");
						}
					}
				}
	
				formatter.Write("]");
			}
*/	
			private void WriteMethodAttributes(IFormatter formatter, IMethodDeclaration methodDeclaration)
			{
				ITypeDeclaration declaringType = (methodDeclaration.DeclaringType as ITypeReference).Resolve();
				if (!declaringType.Interface)
				{
					if ((!methodDeclaration.SpecialName) || (methodDeclaration.Name != ".cctor"))
					{
						switch (methodDeclaration.Visibility)
						{
							case MethodVisibility.Public:
								formatter.WriteKeyword("public");
								break;
		
							case MethodVisibility.Private:
								formatter.WriteKeyword("private");
								break;
		
							case MethodVisibility.PrivateScope:
								formatter.WriteComment("/* private scope */");
								break;
		
							case MethodVisibility.Family:
								formatter.WriteKeyword("protected");
								break;
		
							case MethodVisibility.Assembly:
								formatter.WriteKeyword("public");
								formatter.Write(" ");
								formatter.WriteKeyword("private");
								break;
		
							case MethodVisibility.FamilyOrAssembly:
								formatter.WriteKeyword("public");
								formatter.Write(" ");
								formatter.WriteKeyword("protected");
								break;
		
							case MethodVisibility.FamilyAndAssembly:
								formatter.WriteKeyword("protected");
								formatter.Write(" ");
								formatter.WriteKeyword("private");
								break;
		
							default:
								throw new NotSupportedException();
						}
	
						// formatter.Write(" ");

						// [JLC]
						formatter.Write(": ");
						// formatter.WriteLine();
					}
				}
				
				if (methodDeclaration.SpecialName && 
					(methodDeclaration.Name.StartsWith("get_") || methodDeclaration.Name.StartsWith("set_")) )
				{
					formatter.WriteKeyword("__property");
					formatter.Write(" ");
				}
									
				if (methodDeclaration.Static)
				{
					formatter.WriteKeyword("static");
					formatter.Write(" ");
				}

				if (!declaringType.Interface)
				{
					if ((!methodDeclaration.NewSlot) && (methodDeclaration.Final))
					{
						formatter.WriteKeyword("sealed");
						formatter.Write(" ");
					}

					if (methodDeclaration.Virtual)
					{
						if (methodDeclaration.Abstract)
						{
							formatter.WriteKeyword("abstract");
							formatter.Write(" ");
						}
						else if ((methodDeclaration.NewSlot) && (!methodDeclaration.Final))
						{
							formatter.WriteKeyword("virtual");
							formatter.Write(" ");
						}
	
						if (!methodDeclaration.NewSlot)
						{
							formatter.WriteKeyword("override");
							formatter.Write(" ");
						}
					}
				}
			}

			private void WriteParameterDeclaration(IParameterDeclaration value, IFormatter formatter, ILanguageWriterConfiguration configuration)
			{
				ArrayList customAttributes = new ArrayList();
				customAttributes.AddRange(value.Attributes);
	
				IType parameterType = value.ParameterType;
				IReferenceType referenceType = parameterType as IReferenceType;
	
				bool outAttribute = false;
				bool paramsAttribute = false;
	
				for (int i = customAttributes.Count - 1; i >= 0; i--)
				{
					ICustomAttribute customAttribute = (ICustomAttribute) customAttributes[i];

					// DefaultParameterValue
					if (IsType(customAttribute.Constructor.DeclaringType, "System.Runtime.InteropServices", "DefaultParameterValueAttribute", "System"))
					{
						customAttributes.RemoveAt(i);
					}

					// ParamArrayAttribute
					if (IsType(customAttribute.Constructor.DeclaringType, "System", "ParamArrayAttribute"))
					{
						paramsAttribute = true;
						customAttributes.RemoveAt(i);
					}
	
					// OutAttribute
					// [JLC]
					if (referenceType != null)
					{
						if (IsType(customAttribute.Constructor.DeclaringType, "System.Runtime.InteropServices", "OutAttribute"))
						{
							outAttribute = true;
							customAttributes.RemoveAt(i);
						}
					}
				}

				if ((configuration != null) && (configuration["ShowCustomAttributes"] == "true") && (customAttributes.Count != 0))
				{
					formatter.Write("[");
	
					for (int i = 0; i < customAttributes.Count; i++)
					{
						if (i != 0)
						{
							formatter.Write(", ");
						}
		
						ICustomAttribute customAttribute = (ICustomAttribute) customAttributes[i];
						this.WriteCustomAttribute(formatter, customAttribute);
					}
	
					formatter.Write("]");
					formatter.Write(" ");
				}
	
				if (paramsAttribute)
				{
					formatter.WriteKeyword("params");
					formatter.Write(" ");
				}
	
				if (outAttribute)
				{
					// [JLC]
					// formatter.WriteKeyword("out");
					formatter.WriteKeyword("[Out]"); // TODO System::Runtime::InteropServices::
					formatter.Write(" ");
					this.WriteType(referenceType.ElementType, formatter, "*" + value.Name);
				}
				else
				{
					if (parameterType != null)
					{
						this.WriteType(parameterType, formatter, value.Name);
					}
					else
					{
						formatter.Write("...");
					}
				}

//				// [JLC]
//				if (outAttribute)
//				{
//					formatter.Write("*");
//				}

//				if ((parameterDeclaration.Name != null) && parameterDeclaration.Name.Length > 0)
//				{
//					formatter.Write(" ");
//					formatter.Write(parameterDeclaration.Name);
//				}
//
//				IArrayType arrayType = parameterDeclaration.ParameterType as IArrayType;
//				if (arrayType != null)
//				{
//					WriteArrayTypePostfix(arrayType);
//				}

				IExpression defaultValue = this.GetDefaultParameterValue(value);
				if (defaultValue != null)
				{
					formatter.Write(" = ");
					this.WriteExpression(defaultValue, formatter);
				}
			}

			private void WriteParameterDeclarationList(IParameterDeclarationCollection parameters, IFormatter formatter, ILanguageWriterConfiguration configuration)
			{
				for (int i = 0; i < parameters.Count; i++)
				{
					IParameterDeclaration parameter = parameters[i];
					IType parameterType = parameter.ParameterType;
					if ((parameterType != null) || ((i + 1) != parameters.Count))
					{
						if (i != 0)
						{
							formatter.Write(", ");
						}

						this.WriteParameterDeclaration(parameter, formatter, configuration);
					}
				}
			}
	
			private void WriteCustomAttribute(IFormatter formatter, ICustomAttribute customAttribute)
			{
				ITypeReference declaringType = customAttribute.Constructor.DeclaringType as ITypeReference;
	
				// [JLC]
				// string name = declaringType.Name;
				string name = declaringType.Name; // Helper.GetNameWithResolutionScope(declaringType).Replace(".", "::").Replace("+", "::");
				if (name.EndsWith("Attribute"))
				{
					name = name.Substring(0, name.Length - 9);
				}
	
				formatter.WriteReference(name, this.GetMethodReferenceDescription(customAttribute.Constructor), customAttribute.Constructor);
	
				IExpressionCollection expression = customAttribute.Arguments;
				if (expression.Count != 0)
				{
					formatter.Write ("(");
					for (int i = 0; i < expression.Count; i++)
					{
						if (i != 0)
						{
							formatter.Write (", ");
						}
	
						this.WriteExpression(expression[i], formatter);
					}
	
					formatter.Write (")");
				}
			}
	
			private void WriteCustomAttributeList(IFormatter formatter, ICustomAttributeProvider provider)
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
	
				if (prefix != null)
				{
					for (int i = 0; i < provider.Attributes.Count; i++)
					{
						formatter.Write("[");
						formatter.WriteKeyword(prefix);
						formatter.Write(" ");
						this.WriteCustomAttribute(formatter, provider.Attributes[i]);
						formatter.Write("]");
						
						if (i != (provider.Attributes.Count - 1))
						{
							formatter.WriteLine();
						}
					}
				}
				else
				{
					formatter.Write("[");
	
					for (int i = 0; i < provider.Attributes.Count; i++)
					{
						if (i != 0)
						{
							formatter.Write(", ");
						}
		
						this.WriteCustomAttribute(formatter, provider.Attributes[i]);
					}
		
					formatter.Write("]");
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
							formatter.Write(", ");
						}
	
						this.WriteType(parameters[i], formatter, false);
					}
	
					formatter.Write(">");
				}
			}
			
			private void WriteGenericParameterConstraintList(IFormatter formatter, ITypeCollection parameters)
			{
				if (parameters.Count > 0)
				{
					for (int i = 0; i < parameters.Count; i++)
					{
						IGenericParameter parameter = parameters[i] as IGenericParameter;
						if ((parameter != null) && (parameter.Constraints.Count > 0))
						{
							bool showConstraints = true;
							if (parameter.Constraints.Count == 1)
							{
								ITypeReference constraint = parameter.Constraints[0] as ITypeReference;
								if (constraint != null)
								{
									showConstraints = !IsType(constraint, "System", "Object");
								}
							}
							
							if (showConstraints)
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
	
									this.WriteGenericParameterConstraint(parameter.Constraints[j], formatter);
								}
							}
						}
	
						if (parameter.Attributes.Count > 0)
						{
							for (int j = 0; j < parameter.Attributes.Count; j++)
							{
								ICustomAttribute customAttribute = parameter.Attributes[j];
	
								if (((customAttribute.Constructor.DeclaringType as ITypeReference).Name == "NewConstraintAttribute") && ((customAttribute.Constructor.DeclaringType as ITypeReference).Namespace == "System.Runtime.CompilerServices"))
								{
									if ((j != 0) || (parameter.Constraints.Count != 0))
									{
										formatter.Write(", ");
									}
	
									formatter.WriteKeyword("new");
									formatter.Write("()");
								}
							}
						}
					}
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
					formatter.WriteKeyword("struct");
					return;
				}

				this.WriteType(value, formatter, false);
			}
	
			#region Expression

			public void WriteExpression(IExpression value)
			{
				this.WriteExpression(value, this.formatter);
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

				if (value is IAddressOutExpression)
				{
					this.WriteAddressOutExpression(value as IAddressOutExpression, formatter);
					return;
				}

				if (value is IAddressReferenceExpression)
				{
					this.WriteAddressReferenceExpression(value as IAddressReferenceExpression, formatter);
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

				if (value is IAssignExpression)
				{
					this.WriteAssignExpression(value as IAssignExpression, formatter);
					return;
				}

				if (value is IGenericDefaultExpression)
				{
					this.WriteGenericDefaultExpression(value as IGenericDefaultExpression, formatter);
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
	
			private void WriteGenericDefaultExpression(IGenericDefaultExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("default");
				formatter.Write("(");
				this.WriteType(expression.GenericArgument, formatter, true);
				formatter.Write(")");
			}
	
			private void WriteTypeOfTypedReferenceExpression(ITypeOfTypedReferenceExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("__reftype");
				formatter.Write("(");
				this.WriteExpression(expression.Expression, formatter);
				formatter.Write(")");
			}

			private void WriteValueOfTypedReferenceExpression(IValueOfTypedReferenceExpression value, IFormatter formatter)
			{
				formatter.WriteKeyword("__refvalue");
				formatter.Write("(");
				this.WriteExpression(value.Expression, formatter);
				formatter.Write(", ");
				this.WriteType(value.TargetType, formatter, true);
				formatter.Write(")");
			}

			private void WriteTypedReferenceCreateExpression(ITypedReferenceCreateExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("__makeref");
				formatter.Write("(");
				this.WriteExpression(expression.Expression, formatter);
				formatter.Write(")");
			}
	
			private void WriteMemberInitializerExpression(IMemberInitializerExpression expression, IFormatter formatter)
			{
				this.WriteMemberReference(formatter, expression.Member);
				formatter.Write("=");
				this.WriteExpression(expression.Value, formatter);
			}
	
			private void WriteMemberReference(IFormatter formatter, IMemberReference memberReference)
			{
				IFieldReference fieldReference = memberReference as IFieldReference;
				if (fieldReference != null)
				{
					this.WriteFieldReference(formatter, fieldReference);
				}
	
				IMethodReference methodReference = memberReference as IMethodReference;
				if (methodReference != null)
				{
					this.WriteMethodReference(formatter, methodReference);
				}
	
				IPropertyReference propertyReference = memberReference as IPropertyReference;
				if (propertyReference != null)
				{
					this.WritePropertyReference(formatter, propertyReference);
				}
	
				IEventReference eventReference = memberReference as IEventReference;
				if (eventReference != null)
				{
					this.WriteEventReference(formatter, eventReference);
				}
			}
	
			private void WriteTargetExpression(IFormatter formatter, IExpression expression)
			{
				this.WriteExpression(expression, formatter);
			}
	
			private void WriteTypeOfExpression(ITypeOfExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("__typeof");
				formatter.Write("(");
				this.WriteType(expression.Type, formatter, true);
				formatter.Write(")");
			}

			private void WriteFieldOfExpression(IFieldOfExpression value, IFormatter formatter)
			{
				formatter.WriteKeyword("__fieldof");
				formatter.Write("(");
				this.WriteType(value.Field.DeclaringType, formatter, true);
				formatter.Write(".");
				formatter.WriteReference(value.Field.Name, this.GetFieldReferenceDescription(value.Field), value.Field);
				
				if (value.Type != null)
				{
					formatter.Write(", ");
					this.WriteType(value.Type, formatter, true);
				}

				formatter.Write(")");
			}

			private void WriteMethodOfExpression(IMethodOfExpression value, IFormatter formatter)
			{
				formatter.WriteKeyword("__methodof");
				formatter.Write("(");

				this.WriteType(value.Method.DeclaringType, formatter, true);
				formatter.Write(".");
				formatter.WriteReference(value.Method.Name, this.GetMethodReferenceDescription(value.Method), value.Method);

				if (value.Type != null)
				{
					formatter.Write(", ");
					this.WriteType(value.Type, formatter, true);
				}
				
				formatter.Write(")");
			}
	
			private void WriteArrayElementType(IFormatter formatter, IType type)
			{
				IArrayType arrayType = type as IArrayType;
				if (arrayType != null)
				{
					this.WriteArrayElementType(formatter, arrayType.ElementType);
				}
				else
				{
					this.WriteType(type, formatter, false);	
				}
			}
			
			private void WriteArrayDimension(IFormatter formatter, IType type)
			{
				IArrayType arrayType = type as IArrayType;
				if (arrayType != null)
				{
					this.WriteArrayDimension(formatter, arrayType.ElementType);
	
					formatter.Write("[");
					formatter.Write("]");
				}
			}
	
			private void WriteArrayCreateExpression(IArrayCreateExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("__gc");
				formatter.Write(" ");
				formatter.WriteKeyword("new");
				formatter.Write(" ");
	
				this.WriteArrayElementType(formatter, expression.Type);
				
				formatter.Write("[");
				this.WriteExpressionList(expression.Dimensions, formatter);
				formatter.Write("]");
	
				this.WriteArrayDimension(formatter, expression.Type);
	
				IBlockExpression initializer = expression.Initializer as IBlockExpression;
				if ((initializer != null) && (initializer.Expressions.Count > 0))
				{
					this.WriteExpression(initializer, formatter);
				}
			}

			private void WriteBlockExpression(IBlockExpression value, IFormatter formatter)
			{
				formatter.Write(" {");
				formatter.WriteLine();
				formatter.WriteIndent();
				this.WriteExpressionList(value.Expressions, formatter);
				formatter.WriteOutdent();
				formatter.Write("}");
			}
	
			private void WriteBaseReferenceExpression(IBaseReferenceExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("base");
			}
	
			private void WriteTryCastExpression(ITryCastExpression expression, IFormatter formatter)
			{
				formatter.Write("(");
				this.WriteExpression(expression.Expression, formatter);
				formatter.Write(" ");
				formatter.WriteKeyword("as");
				formatter.Write(" ");
				this.WriteType(expression.TargetType, formatter, true);
				formatter.Write(")");
			}
	
			private void WriteCanCastExpression(ICanCastExpression expression, IFormatter formatter)
			{
				formatter.Write("(");
				this.WriteExpression(expression.Expression, formatter);
				formatter.Write(" ");
				formatter.WriteKeyword("is");
				formatter.Write(" ");
				this.WriteType(expression.TargetType, formatter, true);
				formatter.Write(")");
			}
	
			private void WriteCastExpression(ICastExpression expression, IFormatter formatter)
			{
				// int i = *static_cast<__box System::Int32*>(o);

				formatter.Write("*");
				formatter.WriteKeyword("static_cast");
				
				formatter.Write("<");
				formatter.WriteKeyword("__box");
				formatter.Write(" ");
				this.WriteType(expression.TargetType, formatter, true);
				formatter.Write("*");
				formatter.Write(">");
				
				formatter.Write("(");

				this.WriteExpression(expression.Expression, formatter);
	
				formatter.Write(")");
			}
	
			private void WriteConditionExpression(IConditionExpression expression, IFormatter formatter)
			{
				formatter.Write("(");
				this.WriteExpression(expression.Condition, formatter);
				formatter.Write(" ? ");
				this.WriteExpression(expression.Then, formatter);
				formatter.Write(" : ");
				this.WriteExpression(expression.Else, formatter);
				formatter.Write(")");
			}

			private void WriteNullCoalescingExpression(INullCoalescingExpression value, IFormatter formatter)
			{
				formatter.Write("(");
				this.WriteExpression(value.Condition, formatter);
				formatter.Write(" ?? ");
				this.WriteExpression(value.Expression, formatter);
				formatter.Write(")");
			}
	
			private void WriteDelegateCreateExpression(IDelegateCreateExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("new");
				formatter.Write(" ");
				this.WriteTypeReference(formatter, expression.DelegateType);
				formatter.Write("(");
				this.WriteTargetExpression(formatter, expression.Target);
				formatter.Write(".");
				this.WriteMethodReference(formatter, expression.Method); // TODO Escape = true
				formatter.Write(")");
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

				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
				this.WriteStatement(value.Body);
				formatter.WriteOutdent();
				formatter.Write("}");
			}

			private void WriteTypeReferenceExpression(ITypeReferenceExpression expression, IFormatter formatter)
			{
				this.WriteTypeReference(formatter, expression.Type, true);
			}
	
			private void WriteFieldReferenceExpression(IFieldReferenceExpression expression, IFormatter formatter)
			{
				if (expression.Target != null)
				{
					this.WriteTargetExpression(formatter, expression.Target);
					// [JLC]
					if(expression.Target is ITypeReferenceExpression)
					{
						formatter.Write ("::");
					}
					else
					{
						if (this.methodContext != null)
						{
							if (Helper.IsValueType(this.GetExpressionType(expression.Target) as ITypeReference))
							{
								formatter.Write(".");
							}
							else
							{
								formatter.Write("->");
							}
						}
						else
						{
							formatter.Write(".");
							// formatter.Write(" ");
							// formatter.WriteComment("/* no method context */");
							// formatter.Write(" ");
						}
					}
				}
	
				this.WriteFieldReference(formatter, expression.Field);
			}
	
			private void WriteArgumentReferenceExpression(IArgumentReferenceExpression expression, IFormatter formatter)
			{
				// TODO Escape name?
				// TODO Should there be a Resovle() mechanism
	
				IFormatter textFormatter = new TextFormatter();
				this.WriteParameterDeclaration(expression.Parameter.Resolve(), textFormatter, null);
				textFormatter.Write("; // Parameter");
	
				if (expression.Parameter.Name != null)
				{
					formatter.WriteReference(expression.Parameter.Name, textFormatter.ToString(), null);
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
	
			private void WriteVariableReference(IVariableReference variableReference, IFormatter formatter)
			{
				IVariableDeclaration variableDeclaration = variableReference.Resolve();

				IFormatter textFormatter = new TextFormatter();
				this.WriteVariableDeclaration(variableDeclaration, textFormatter);
				textFormatter.Write(" // Local Variable");

				formatter.WriteReference(variableDeclaration.Name, textFormatter.ToString(), null);
			}
	
			private void WritePropertyIndexerExpression(IPropertyIndexerExpression expression, IFormatter formatter)
			{
				if (expression.Target.Property.Name == "Item")
				{
					this.WriteTargetExpression(formatter, expression.Target.Target);
				}
				else
				{
					this.WriteTargetExpression(formatter, expression.Target);
				}
				
				formatter.Write("[");
	
				bool first = true;
	
				foreach (IExpression indexExpression in expression.Indices)
				{
					if (first)
					{
						first = false;
					}
					else
					{
						formatter.Write(", ");
					}
	
					this.WriteExpression(indexExpression, formatter);
				}
	
				formatter.Write("]");
			}
	
			private void WriteArrayIndexerExpression(IArrayIndexerExpression expression, IFormatter formatter)
			{
				this.WriteTargetExpression(formatter, expression.Target);
				formatter.Write("[");
	
				for (int i = 0; i < expression.Indices.Count; i++)
				{
					if (i != 0)
					{
						formatter.Write(", ");
					}
	
					this.WriteExpression(expression.Indices[i], formatter);
				}
	
				formatter.Write("]");
			}
	
			private void WriteMethodInvokeExpression(IMethodInvokeExpression expression, IFormatter formatter)
			{
				IMethodReference methodReference = null;

				IMethodReferenceExpression methodReferenceExpression = expression.Method as IMethodReferenceExpression;
				if (methodReferenceExpression != null)
				{
					bool valueTypeConstructor = false;
	
					methodReference = methodReferenceExpression.Method;
					ITypeReference declaringType = methodReference.DeclaringType as ITypeReference;
					if (declaringType != null)
					{
						if ((methodReference.Name == ".ctor") && (methodReference.Resolve().SpecialName) && (Helper.IsValueType(declaringType)))
						{
							valueTypeConstructor = true;
							
							this.WriteExpression(methodReferenceExpression.Target, formatter);
							formatter.Write(" = ");
							if (!Helper.IsValueType(methodReference.DeclaringType as ITypeReference))
							{
								formatter.WriteKeyword("__gc");
								formatter.Write(" ");
								formatter.WriteKeyword("new");
								formatter.Write(" ");
								this.WriteType(methodReference.DeclaringType, formatter, true);
							}
						}
					}
	
					if (!valueTypeConstructor)
					{
						this.WriteMethodReferenceExpression(methodReferenceExpression, formatter);
					}
				}
				else
				{
					// FunctionPointer call
					formatter.Write("*");
					this.WriteExpression(expression.Method, formatter);
				}
							
				formatter.Write("(");

				// [JLC]
//				this.WriteExpressionList(expression.Arguments, formatter);

				IMethodDeclaration methodDeclaration = null;
				if (methodReference != null)
				{
					methodDeclaration = methodReference.Resolve();
				}

				// Indent++;
				for (int i = 0; i < expression.Arguments.Count; i++)
				{
					if (i != 0)
					{
						formatter.Write(", ");
					}

					// HACK: Do we need deeper support for methodContext being null?
					if (methodDeclaration != null && this.methodContext != null)
					{
						IType parameterType = methodReference.Parameters[i].ParameterType;
						IType expressionType = this.GetExpressionType(expression.Arguments[i]);

						if (IsType(parameterType, "System", "Object") && Helper.IsValueType(expressionType as ITypeReference))
						{
							formatter.WriteKeyword("__box");
							formatter.WriteKeyword("(");
							this.WriteExpression(expression.Arguments[i], formatter);
							formatter.WriteKeyword(")");
						}
						else
						{
							this.WriteExpression(expression.Arguments[i], formatter);
						}
					}
					else
					{
						this.WriteExpression(expression.Arguments[i], formatter);
					}
				}
				// Indent--;

				formatter.Write(")");
			}
	
			private void WriteMethodReferenceExpression(IMethodReferenceExpression expression, IFormatter formatter)
			{
				// TODO bool escape = true;
	
				if (expression.Target != null)
				{
					// TODO escape = false;
					if (expression.Target is IBinaryExpression)
					{
						formatter.Write("(");
						this.WriteExpression(expression.Target, formatter);
						formatter.Write(")");
					}
					else
					{
						this.WriteTargetExpression(formatter, expression.Target);
					}
	
					// [JLC]
					// formatter.Write(".");
					if (expression.Target is ITypeReferenceExpression)
					{
						formatter.Write("::");
					}
					else
					{
						if (this.methodContext != null)
						{
							if (Helper.IsValueType(this.GetExpressionType(expression.Target) as ITypeReference))
							{
								formatter.Write(".");
							}
							else
							{
								formatter.Write("->");
							}
						}
						else
						{
							formatter.Write(".");
							// formatter.Write(" ");
							// formatter.WriteComment("/* no method context */");
							// formatter.Write(" ");
						}
					}
				}
	
				this.WriteMethodReference(formatter, expression.Method);
			}
	
			private void WriteEventReferenceExpression(IEventReferenceExpression expression, IFormatter formatter)
			{
				// TODO bool escape = true;
	
				if (expression.Target != null)
				{
					// TODO escape = false;
					this.WriteTargetExpression(formatter, expression.Target);

					// [JLC]
					// formatter.Write(".");
					if(expression.Target is ITypeReferenceExpression)
					{
						formatter.Write ("::");
					}
					else
					{
						if (this.methodContext != null)
						{
							if (Helper.IsValueType(this.GetExpressionType(expression.Target) as ITypeReference))
							{
								formatter.Write(".");
							}
							else
							{
								formatter.Write("->");
							}
						}
						else
						{
							formatter.Write(".");
							// formatter.Write(" ");
							// formatter.WriteComment("/* no method context */");
							// formatter.Write(" ");
						}
					}
				}
	
				this.WriteEventReference(formatter, expression.Event);
			}
	
			private void WriteDelegateInvokeExpression(IDelegateInvokeExpression expression, IFormatter formatter)
			{
				if (expression.Target != null)
				{
					this.WriteTargetExpression(formatter, expression.Target);
				}
	
				formatter.Write("(");
				this.WriteExpressionList(expression.Arguments, formatter);
				formatter.Write(")");
			}
	
			private void WriteObjectCreateExpression(IObjectCreateExpression value, IFormatter formatter)
			{
				if (!Helper.IsValueType(value.Type as ITypeReference))
				{
					formatter.WriteKeyword("__gc");
					formatter.Write(" ");
					formatter.WriteKeyword("new");
					formatter.Write(" ");

					if (value.Constructor != null)
					{
						this.WriteTypeReference(formatter, value.Type as ITypeReference, this.GetMethodReferenceDescription(value.Constructor), value.Constructor);
					}
					else
					{
						this.WriteType(value.Type, formatter, true);
					}
				}

				formatter.Write("(");
				this.WriteExpressionList(value.Arguments, formatter);
				formatter.Write(")");

				IBlockExpression initializer = value.Initializer as IBlockExpression;
				if ((initializer != null) && (initializer.Expressions.Count > 0))
				{
					formatter.Write(" ");
					this.WriteExpression(initializer, formatter);
				}
			}
		
			private void WritePropertyReferenceExpression(IPropertyReferenceExpression expression, IFormatter formatter)
			{
				// TODO bool escape = true;
	
				if (expression.Target != null)
				{
					// TODO escape = false;
					this.WriteTargetExpression(formatter, expression.Target);

					// [JLC]
					// formatter.Write(".");
					if(expression.Target is ITypeReferenceExpression)
					{
						formatter.Write ("::");
					}
					else
					{
						if (this.methodContext != null)
						{
							if (Helper.IsValueType(this.GetExpressionType(expression.Target) as ITypeReference))
							{
								formatter.Write(".");
							}
							else
							{
								formatter.Write("->");
							}
						}
						else
						{
							formatter.Write(".");
							// formatter.Write(" ");
							// formatter.WriteComment("/* no method context */");
							// formatter.Write(" ");
						}
					}
				}
	
				this.WritePropertyReference (formatter, expression.Property);
			}
	
			private void WriteThisReferenceExpression(IThisReferenceExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("this");
			}
	
			private void WriteAddressOfExpression(IAddressOfExpression expression, IFormatter formatter)
			{
				formatter.Write("&");
				this.WriteExpression(expression.Expression, formatter);
			}

			private void WriteAddressOutExpression(IAddressOutExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("out");
				formatter.Write(" ");
				this.WriteExpression(expression.Expression, formatter);
			}

			private void WriteAddressReferenceExpression(IAddressReferenceExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("ref");
				formatter.Write(" ");
				this.WriteExpression(expression.Expression, formatter);
			}
	
			private void WriteAddressDereferenceExpression(IAddressDereferenceExpression expression, IFormatter formatter)
			{
				formatter.Write("*(");
				this.WriteExpression(expression.Expression, formatter);
				formatter.Write(")");
			}
	
			private void WriteSizeOfExpression(ISizeOfExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("sizeof");
				formatter.Write("(");
				this.WriteType(expression.Type, formatter, true);
				formatter.Write(")");
			}

	
			private void WriteStackAllocateExpression(IStackAllocateExpression expression, IFormatter formatter)
			{
				formatter.WriteKeyword("stackalloc");
				formatter.Write(" ");
				this.WriteType(expression.Type, formatter, true);
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
						formatter.Write("~");
						this.WriteExpression(expression.Expression, formatter);
						break;
	
					case UnaryOperator.BooleanNot:
						formatter.Write("!");
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
				this.WriteBinaryOperator(formatter, expression.Operator);
				formatter.Write(" ");
				this.WriteExpression(expression.Right, formatter);
				formatter.Write(")");
			}
	
			private void WriteBinaryOperator(IFormatter formatter, BinaryOperator operatorType)
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
						formatter.Write("/");
						break;
	
					case BinaryOperator.Modulus:
						formatter.Write ("%");
						break;
	
					case BinaryOperator.ShiftLeft:
						formatter.Write ("<<");
						break;
	
					case BinaryOperator.ShiftRight:
						formatter.Write (">>");
						break;
	
					case BinaryOperator.IdentityInequality:
						formatter.Write ("!=");
						break;
	
					case BinaryOperator.IdentityEquality:
						formatter.Write ("==");
						break;

					case BinaryOperator.ValueInequality:
						formatter.Write ("!=");
						break;
	
					case BinaryOperator.ValueEquality:
						formatter.Write ("==");
						break;
	
					case BinaryOperator.BitwiseOr:
						formatter.Write ("|");
						break;
	
					case BinaryOperator.BitwiseAnd:
						formatter.Write ("&");
						break;
	
					case BinaryOperator.BitwiseExclusiveOr:
						formatter.Write ("^");
						break;
	
					case BinaryOperator.BooleanOr:
						formatter.Write ("||");
						break;
	
					case BinaryOperator.BooleanAnd:
						formatter.Write ("&&");
						break;
	
					case BinaryOperator.LessThan:
						formatter.Write ("<");
						break;
	
					case BinaryOperator.LessThanOrEqual:
						formatter.Write ("<=");
						break;
	
					case BinaryOperator.GreaterThan:
						formatter.Write (">");
						break;
	
					case BinaryOperator.GreaterThanOrEqual:
						formatter.Write (">=");
						break;
	
					default:
						throw new NotSupportedException (operatorType.ToString ());
				}
			}
	
			private void WriteLiteralExpression(ILiteralExpression value, IFormatter formatter)
			{
				if (value.Value == null)
				{
					// [JLC] formatter.WriteLiteral("null");
					formatter.WriteLiteral("0");
				}
				else if (value.Value is char)
				{
					this.WriteCharLiteral(formatter, (char) value.Value);
				}
				else if (value.Value is string)
				{
					this.WriteStringLiteral(formatter, (string) value.Value);
				}
				else if (value.Value is byte)
				{
					this.WriteNumber((byte) value.Value, formatter);
				}
				else if (value.Value is sbyte)
				{
					this.WriteNumber((sbyte) value.Value, formatter);
				}
				else if (value.Value is short)
				{
					this.WriteNumber((short) value.Value, formatter);
				}
				else if (value.Value is ushort)
				{
					this.WriteNumber((ushort) value.Value, formatter);
				}
				else if (value.Value is int)
				{
					this.WriteNumber((int) value.Value, formatter);
				}
				else if (value.Value is uint)
				{
					this.WriteNumber((uint) value.Value, formatter);
				}
				else if (value.Value is long)
				{
					this.WriteNumber((long) value.Value, formatter);
				}
				else if (value.Value is ulong)
				{
					this.WriteNumber((ulong) value.Value, formatter);
				}
				else if (value.Value is float)
				{
					// TODO
					formatter.WriteLiteral(((float)value.Value).ToString(CultureInfo.InvariantCulture)); // + " /* f */ ");
				}
				else if (value.Value is double)
				{
					// TODO
					formatter.WriteLiteral(((double)value.Value).ToString("R", CultureInfo.InvariantCulture)); // + " /* f */ ");
				}
				else if (value.Value is decimal)
				{
					formatter.WriteLiteral(((decimal)value.Value).ToString(CultureInfo.InvariantCulture));
				}
				else if (value.Value is bool)
				{
					formatter.WriteLiteral(((bool)value.Value) ? "true" : "false");
				}
				else if (value.Value is byte[])
				{
					formatter.Write("{ ");
					
					byte[] bytes = (byte[]) value.Value;
					for (int i = 0; i < bytes.Length; i++)
					{
						if (i != 0)
						{
							formatter.Write(", ");
						}	
	
						formatter.WriteLiteral("0x" + bytes[i].ToString("X2"));	
					}
					
					formatter.Write(" }");
				}
				else
				{
					throw new ArgumentException("expression");
				}
			}

			private void WriteNumber(IConvertible value, IFormatter formatter)
			{
				IFormattable formattable = (IFormattable) value;
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

			private void WriteTypeReference(IFormatter formatter, ITypeReference typeReference)
			{
				WriteTypeReference(formatter, typeReference, false);
			}
	
			private void WriteTypeReference(IFormatter formatter, ITypeReference typeReference, bool isExpression)
			{
				this.WriteType(typeReference, formatter, isExpression);
			}
	
			private void WriteTypeReference(IFormatter formatter, ITypeReference value, string description, object target)
			{
				string name = value.Name;
	
				// TODO mscorlib test
				bool specialName = false;
				if (value.Namespace == "System")
				{
					if (specialTypeNames.Contains(name))
					{
						name = (string)specialTypeNames[name];
						specialName = true;
					}
				}
	
				string genericName = value.Name;
				if (value.GenericArguments.Count != 0)
				{
					genericName += "`" + value.GenericArguments.Count.ToString();
				}
	
				// [JLC] [HACK] Use full name for moment.
				// if ((!specialName) && (this.ambiguousTypeNames != null) && (this.ambiguousTypeNames.Contains(genericName)))
				if (!specialName)
				{
					// [JLC]
					// name = new TypeInformation(value).NameWithResolutionScope.Replace("+", ".");	
					name = value.Name; // LR Helper.GetNameWithResolutionScope(value).Replace(".", "::").Replace("+", "::");
				}

				ITypeReference genericType = value.GenericType;
				if (genericType != null)
				{
					formatter.WriteReference(name, description, genericType);
					this.WriteGenericArgumentList(value.GenericArguments, formatter);
				}
				else
				{
					formatter.WriteReference(name, description, target);
					this.WriteGenericArgumentList(value.GenericArguments, formatter);
				}
			}
	
			private void WriteFieldReference(IFormatter formatter, IFieldReference fieldReference)
			{
				// TODO Escape?
				formatter.WriteReference(fieldReference.Name, this.GetFieldReferenceDescription(fieldReference), fieldReference);
			}
	
			private void WriteMethodReference(IFormatter formatter, IMethodReference methodReference)
			{
				// TODO Escape?

				IMethodReference methodInstanceReference = methodReference as IMethodReference;
				if (methodInstanceReference != null)
				{
					formatter.WriteReference(methodReference.Name, this.GetMethodReferenceDescription(methodReference), methodInstanceReference.GenericMethod);
					this.WriteGenericArgumentList(methodInstanceReference.GenericArguments, formatter);
				}
				else
				{
					formatter.WriteReference(methodReference.Name, this.GetMethodReferenceDescription(methodReference), methodReference);
				}
			}
	
			private void WritePropertyReference(IFormatter formatter, IPropertyReference propertyReference)
			{
				// TODO Escape?
				formatter.WriteReference(propertyReference.Name, this.GetPropertyReferenceDescription(propertyReference), propertyReference);
			}
	
			private void WriteEventReference(IFormatter formatter, IEventReference eventReference)
			{
				// TODO Escape?
				formatter.WriteReference(eventReference.Name, this.GetEventReferenceDescription(eventReference), eventReference);
			}
		
			#endregion
	
			#region Statement
			public void WriteStatement(IStatement value)
			{
				this.WriteStatement(value, this.formatter);
			}

			private void WriteStatement(IStatement value, IFormatter formatter)
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
					this.WriteMethodReturnStatement(value as IMethodReturnStatement, formatter);
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
	
				if (value is ICommentStatement)
				{
					this.WriteCommentStatement(value as ICommentStatement, formatter);
					return;
				}
				
				if (value is ILockStatement)
				{
					this.WriteLockStatement(value as ILockStatement, formatter);
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

				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid statement type '{0}'.", value.GetType().Name), "value");
			}
	
			private void WriteBlockStatement(IBlockStatement statement, IFormatter formatter)
			{
				if (statement.Statements.Count > 0)
				{
					this.WriteStatementList(formatter, statement.Statements);
				}
			}
	
			private void WriteMemoryCopyStatement(IMemoryCopyStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("memcpy");
				formatter.Write("(");
				this.WriteExpression(statement.Source, formatter);
				formatter.Write(", ");
				this.WriteExpression(statement.Destination, formatter);
				formatter.Write(", ");
				this.WriteExpression(statement.Length, formatter);
				formatter.Write(")");
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteMemoryInitializeStatement(IMemoryInitializeStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("meminit");
				formatter.Write("(");
				this.WriteExpression(statement.Offset, formatter);
				formatter.Write(", ");
				this.WriteExpression(statement.Value, formatter);
				formatter.Write(", ");
				this.WriteExpression(statement.Length, formatter);
				formatter.Write(")");
				formatter.Write(";");
				formatter.WriteLine();
			}

			private void WriteDebugBreakStatement(IDebugBreakStatement value, IFormatter formatter)
			{
				formatter.WriteKeyword("debug");
				formatter.Write(";");
				formatter.WriteLine();
			}

			private void WriteLockStatement(ILockStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("lock");	
				formatter.Write(" ");
				formatter.Write("(");
				this.WriteExpression(statement.Expression, formatter);
				formatter.Write(")");
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
	
				if (statement.Body != null)
				{
					this.WriteBlockStatement(statement.Body, formatter);	
				}
	
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
			}
	
			private void WriteForEachStatement(IForEachStatement statement, IFormatter formatter)
			{
				bool oldStatementLineBreak = this.statementLineBreak;
				this.statementLineBreak = false;
	
				formatter.WriteKeyword("foreach");
				formatter.Write(" ");
				formatter.Write("(");
				this.WriteVariableDeclaration(statement.Variable, formatter);
				formatter.Write(" ");
				formatter.WriteKeyword("in");
				formatter.Write(" ");
				this.WriteExpression(statement.Expression, formatter);
				formatter.Write(")");
	
				this.statementLineBreak = oldStatementLineBreak;
	
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
	
				if (statement.Body != null)
				{
					this.WriteBlockStatement(statement.Body, formatter);	
				}
	
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
			}
	
			private void WriteUsingStatement(IUsingStatement statement, IFormatter formatter)
			{
				bool oldStatementLineBreak = this.statementLineBreak;
				this.statementLineBreak = false;
	
				formatter.WriteKeyword("using");
				formatter.Write(" ");
				formatter.Write("(");
				this.WriteExpression(statement.Expression, formatter);
				formatter.Write(")");
	
				this.statementLineBreak = oldStatementLineBreak;
	
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
	
				if (statement.Body != null)
				{
					this.WriteBlockStatement(statement.Body, formatter);	
				}
	
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
			}
	
			private void WriteFixedStatement(IFixedStatement statement, IFormatter formatter)
			{
				bool oldStatementLineBreak = this.statementLineBreak;
				this.statementLineBreak = false;
	
				formatter.WriteKeyword("fixed");
				formatter.Write(" ");
				formatter.Write("(");
				this.WriteVariableDeclaration(statement.Variable, formatter);
				formatter.Write(" ");
				formatter.WriteKeyword("=");
				formatter.Write(" ");
				this.WriteExpression(statement.Expression, formatter);
				formatter.Write(")");
	
				this.statementLineBreak = oldStatementLineBreak;
	
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
	
				if (statement.Body != null)
				{
					this.WriteBlockStatement(statement.Body, formatter);	
				}
	
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
			}
	
			private void WriteStatementList(IFormatter formatter, IStatementCollection statements)
			{
				foreach (IStatement statment in statements)
				{
					this.WriteStatement(statment, formatter);
				}
			}
	
			private void WriteCommentStatement(ICommentStatement statement, IFormatter formatter)
			{
				if (statement.Comment.Text.IndexOf("\n") == -1)
				{
					formatter.WriteComment("// ");
					formatter.WriteComment(statement.Comment.Text);
					formatter.WriteLine();
				}
				else
				{
					formatter.WriteComment("/* ");
					formatter.WriteComment(statement.Comment.Text);
					formatter.WriteComment(" */");
					formatter.WriteLine();
				}
			}
	
			private void WriteMethodReturnStatement(IMethodReturnStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("return");
	
				if (statement.Expression != null)
				{
					formatter.Write(" ");
					this.WriteExpression(statement.Expression, formatter);
				}
	
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteConditionStatement(IConditionStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("if");
				formatter.Write(" ");
				if (statement.Condition is IBinaryExpression)
				{
					this.WriteExpression(statement.Condition, formatter);
				}
				else
				{
					formatter.Write("(");
					this.WriteExpression(statement.Condition, formatter);
					formatter.Write(")");
				}
	
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
				
				if (statement.Then != null)
				{
					this.WriteStatement(statement.Then, formatter);
				}
	
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
	
				if ((statement.Else != null) && (statement.Else.Statements.Count > 0))
				{
					formatter.WriteKeyword("else");
					
					IConditionStatement elseStatement = statement.Else.Statements[0] as IConditionStatement;
					if ((elseStatement != null) && (statement.Else.Statements.Count == 1))
					{
						formatter.Write(" ");
						this.WriteStatement(elseStatement, formatter);
					}
					else
					{
						formatter.WriteLine();
						formatter.Write("{");
						formatter.WriteLine();
						formatter.WriteIndent();
						
						if (statement.Else != null)
						{
							this.WriteStatement(statement.Else, formatter);
						}
		
						formatter.WriteOutdent();
						formatter.Write("}");
						formatter.WriteLine();
					}
				}
			}
	
			private void WriteTryCatchFinallyStatement(ITryCatchFinallyStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("try");
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
				if (statement.Try != null)
				{
					this.WriteStatement(statement.Try, formatter);
				}
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
				
				foreach (ICatchClause catchClause in statement.CatchClauses)
				{
					formatter.WriteKeyword("catch");
	
					ITypeReference catchType = (ITypeReference) catchClause.Variable.VariableType;
					bool hiddenName = (catchClause.Variable.Name.Length == 0);
					bool hiddenType = (IsType(catchType, "System", "Object"));
	
					if ((!hiddenName) || (!hiddenType))
					{
						formatter.Write(" ");
						formatter.Write("(");
						this.WriteType(catchClause.Variable.VariableType, formatter, false);
	
						if (!hiddenName)
						{
							formatter.Write(" ");
							formatter.WriteDeclaration(catchClause.Variable.Name);
						}
	
						formatter.Write(")");
					}
	
					if (catchClause.Condition != null)
					{
						formatter.Write(" ");
						formatter.WriteKeyword("when");
						formatter.Write(" ");
						formatter.Write("(");
						this.WriteExpression(catchClause.Condition, formatter);
						formatter.Write(")");
					}
	
					formatter.WriteLine();
					formatter.Write("{");
					formatter.WriteLine();
					formatter.WriteIndent();
					if (catchClause.Body != null)
					{
						this.WriteStatement(catchClause.Body, formatter);
					}
					formatter.WriteOutdent();
					formatter.Write("}");
					formatter.WriteLine();
				}
	
				if ((statement.Fault != null) && (statement.Fault.Statements.Count > 0))
				{
					formatter.WriteKeyword("fault");
					formatter.WriteLine();
					formatter.Write("{");
					formatter.WriteLine();
					formatter.WriteIndent();
					if (statement.Fault != null)
					{
						this.WriteStatement(statement.Fault, formatter);
					}
					formatter.WriteOutdent();
					formatter.Write("}");
					formatter.WriteLine();
				}
	
				if ((statement.Finally != null) && (statement.Finally.Statements.Count > 0))
				{
					formatter.WriteKeyword("finally");
					formatter.WriteLine();
					formatter.Write("{");
					formatter.WriteLine();
					formatter.WriteIndent();
					if (statement.Finally != null)
					{
						this.WriteStatement(statement.Finally, formatter);
					}
					formatter.WriteOutdent();
					formatter.Write("}");
					formatter.WriteLine();
				}
			}

			private void WriteAssignExpression(IAssignExpression statement, IFormatter formatter)
			{
				IBinaryExpression binaryExpression = statement.Expression as IBinaryExpression;
				if (binaryExpression != null)
				{
					if (statement.Target.Equals(binaryExpression.Left))
					{
						string operatorText = string.Empty;
						
						switch (binaryExpression.Operator)
						{
							case BinaryOperator.Add:
								operatorText = "+=";
								break;
			
							case BinaryOperator.Subtract:
								operatorText = "-=";
								break;
			
							case BinaryOperator.Multiply:
								operatorText = "*=";
								break;
			
							case BinaryOperator.Divide:
								operatorText = "/=";
								break;
			
							case BinaryOperator.BitwiseOr:
								operatorText = "|=";
								break;
			
							case BinaryOperator.BitwiseAnd:
								operatorText = "&=";
								break;
			
							case BinaryOperator.BitwiseExclusiveOr:
								operatorText = "^=";
								break;
						}
	
						if (operatorText.Length != 0)
						{
							this.WriteExpression(statement.Target, formatter);
							formatter.Write(" ");
							formatter.Write(operatorText);
							formatter.Write(" ");
							this.WriteExpression(binaryExpression.Right, formatter);
	
							return;	
						}
					}
				}
	
				this.WriteExpression(statement.Target, formatter);
				formatter.Write (" = ");

//				ExpressionInformation targetInformation = new ExpressionInformation(statement.Target);
//				ExpressionInformation expressionInformation = new ExpressionInformation(statement.Expression);

				// HACK: Do we need deeper support for methodContext being null?
				if (this.methodContext != null)
				{
					IType targetType = this.GetExpressionType(statement.Target);
					IType expressionType = this.GetExpressionType(statement.Expression);

					if (IsType(targetType, "System", "Object") && Helper.IsValueType(expressionType as ITypeReference))
					{
						formatter.WriteKeyword("__box");
						formatter.WriteKeyword("(");
						this.WriteExpression(statement.Expression, formatter);
						formatter.WriteKeyword(")");
					}
					else
					{
						this.WriteExpression(statement.Expression, formatter);
					}
				}
				else
				{
					this.WriteExpression(statement.Expression, formatter);
				}
	
			}
	
			private void WriteExpressionStatement(IExpressionStatement statement, IFormatter formatter)
			{
				this.WriteExpression(statement.Expression, formatter);

				if (this.statementLineBreak)
				{
					formatter.Write(";");
					formatter.WriteLine();
				}
			}
	
			private void WriteForStatement(IForStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("for");
				formatter.Write (" ");
				formatter.Write ("(");
				
				bool oldStatementLineBreak = this.statementLineBreak;
				this.statementLineBreak = false;
	
				if (statement.Initializer != null)
				{
					this.WriteStatement(statement.Initializer, formatter);
				}
	
				formatter.Write ("; ");
				if (statement.Condition != null)
				{
					this.WriteExpression(statement.Condition, formatter);
				}
	
				formatter.Write ("; ");
				if (statement.Increment != null)
				{
					this.WriteStatement(statement.Increment, formatter);
				}
	
				this.statementLineBreak = oldStatementLineBreak;
				formatter.Write(")");
	
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
				if (statement.Body != null)
				{
					this.WriteStatement(statement.Body, formatter);
				}
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
			}
	
			private void WriteWhileStatement(IWhileStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("while");
				formatter.Write(" ");
				formatter.Write("(");
				if (statement.Condition != null)
				{
					this.WriteExpression(statement.Condition, formatter);
				}
				else
				{
					formatter.WriteLiteral("true");
				}
	
				formatter.Write (")");
	
				formatter.WriteLine ();
				formatter.Write ("{");
				formatter.WriteLine ();
				formatter.WriteIndent ();
				if (statement.Body != null)
				{
					this.WriteStatement(statement.Body, formatter);
				}
				formatter.WriteOutdent ();
				formatter.Write ("}");
				formatter.WriteLine ();
			}
	
			private void WriteDoStatement(IDoStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("do");
	
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
				if (statement.Body != null)
				{
					this.WriteStatement(statement.Body, formatter);
				}
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
	
				formatter.WriteKeyword("while");
				formatter.Write(" ");
				formatter.Write("(");
	
				if (statement.Condition != null)
				{
					this.WriteExpression(statement.Condition, formatter);
				}
				else
				{
					formatter.WriteLiteral("true");
				}
	
				formatter.Write(")");
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteThrowExceptionStatement(IThrowExceptionStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("throw");
				if (statement.Expression != null)
				{
					formatter.Write(" ");
					this.WriteExpression(statement.Expression, formatter);
				}
	
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteBreakStatement(IBreakStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("break");	
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteContinueStatement(IContinueStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("continue");	
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteVariableDeclarationExpression(IVariableDeclarationExpression expression, IFormatter formatter)
			{
				this.WriteVariableDeclaration(expression.Variable, formatter);
			}
			
			private void WriteVariableDeclaration(IVariableDeclaration variableDeclaration, IFormatter formatter)
			{
				// formatter.Write(" /* WriteVariableDeclaration: " + variableDeclaration.Name + " */ ");

				this.WriteType(variableDeclaration.VariableType, formatter, variableDeclaration.Name);

				// this.WriteType(variableDeclaration.VariableType, formatter, false);
				// formatter.Write(" ");
				// formatter.WriteDeclaration(variableDeclaration.Name); // TODO Escape = true
				
				// this.WriteType(variableDeclaration.VariableType, formatter, variableDeclaration.Name);
			}
	
			private void WriteAttachEventStatement(IAttachEventStatement statement, IFormatter formatter)
			{
				this.WriteEventReferenceExpression(statement.Event, formatter);
				formatter.Write(" += ");
				this.WriteExpression(statement.Listener, formatter);
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteRemoveEventStatement(IRemoveEventStatement statement, IFormatter formatter)
			{
				this.WriteEventReferenceExpression(statement.Event, formatter);
				formatter.Write(" -= ");
				this.WriteExpression(statement.Listener, formatter);
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteSwitchStatement(ISwitchStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("switch");
				formatter.Write(" (");
				this.WriteExpression(statement.Expression, formatter);
				formatter.Write(")");
				formatter.WriteLine();
				formatter.Write("{");
				formatter.WriteLine();
				formatter.WriteIndent();
				foreach (ISwitchCase switchCase in statement.Cases)
				{
					IConditionCase conditionCase = switchCase as IConditionCase;
					if (conditionCase != null)
					{
						this.WriteSwitchCaseCondition(conditionCase.Condition, formatter);

						formatter.Write("{");
						formatter.WriteLine();
						formatter.WriteIndent();
						if (conditionCase.Body != null)
						{
							this.WriteStatement(conditionCase.Body, formatter);
						}
						formatter.WriteOutdent();
						formatter.Write("}");
						formatter.WriteLine();
					}

					IDefaultCase defaultCase = switchCase as IDefaultCase;
					if (defaultCase != null)
					{
						formatter.WriteKeyword("default");
						formatter.Write(":");
						formatter.WriteLine();
						formatter.Write("{");
						formatter.WriteLine();
						formatter.WriteIndent();
						if (defaultCase.Body != null)
						{
							this.WriteStatement(defaultCase.Body, formatter);
						}
						formatter.WriteOutdent();
						formatter.Write("}");
						formatter.WriteLine();
					}
				}
	
				formatter.WriteOutdent();
				formatter.Write("}");
				formatter.WriteLine();
			}

			private void WriteSwitchCaseCondition(IExpression condition, IFormatter formatter)
			{
				IBinaryExpression binaryExpression = condition as IBinaryExpression;
				if ((binaryExpression != null) && (binaryExpression.Operator == BinaryOperator.BooleanOr))
				{
					this.WriteSwitchCaseCondition(binaryExpression.Left, formatter);
					this.WriteSwitchCaseCondition(binaryExpression.Right, formatter);
				}
				else
				{
					formatter.WriteKeyword("case");
					formatter.Write(" ");
					this.WriteExpression(condition, formatter);
					formatter.Write(":");
					formatter.WriteLine();
				}
			}
	
			private void WriteGotoStatement(IGotoStatement statement, IFormatter formatter)
			{
				formatter.WriteKeyword("goto ");
				formatter.Write(statement.Name);
				formatter.Write(";");
				formatter.WriteLine();
			}
	
			private void WriteLabeledStatement(ILabeledStatement statement, IFormatter formatter)
			{
				formatter.WriteOutdent();
				formatter.WriteDeclaration(statement.Name);
				formatter.Write(":");
				formatter.WriteLine();
				formatter.WriteIndent();
				if (statement.Statement != null)
				{
					this.WriteStatement(statement.Statement, formatter);
				}
			}
			#endregion
	
			private void WriteDeclaringType(IFormatter formatter, ITypeReference typeReference)
			{
				formatter.WriteProperty("Declaring Type", Helper.GetNameWithResolutionScope(typeReference));
				this.WriteDeclaringAssembly(Helper.GetAssemblyReference(typeReference), formatter);
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
				
				this.WriteType(fieldReference.FieldType, formatter, true);
				formatter.Write(" ");
				formatter.Write(this.GetTypeReferenceDescription(fieldReference.DeclaringType as ITypeReference));
				formatter.Write(".");
				formatter.WriteDeclaration(fieldReference.Name);
				formatter.Write(";");
				
				return formatter.ToString();
			}
	
			private string GetMethodReferenceDescription(IMethodReference methodReference)
			{
				IFormatter formatter = new TextFormatter();
	
				if (this.IsConstructor(methodReference))
				{
					formatter.Write(this.GetTypeReferenceDescription(methodReference.DeclaringType as ITypeReference));
					formatter.Write(".");
					formatter.Write(Helper.GetName(methodReference.DeclaringType as ITypeReference));
				}
				else
				{
					// TODO custom attributes [return: ...]
					this.WriteType(methodReference.ReturnType.Type, formatter, true);
					formatter.Write(" ");
					formatter.Write(Helper.GetNameWithResolutionScope(methodReference.DeclaringType as ITypeReference));
					formatter.Write(".");
					formatter.Write(methodReference.Name);
				}

				this.WriteGenericArgumentList(methodReference.GenericArguments, formatter);
	
				formatter.Write("(");
	
				this.WriteParameterDeclarationList(methodReference.Parameters, formatter, null);
	
				if (methodReference.CallingConvention == MethodCallingConvention.VariableArguments)
				{
					formatter.WriteKeyword(", __arglist");
				}
	
				formatter.Write(")");
				formatter.Write(";");
				
				return formatter.ToString();
			}
	
			private string GetPropertyReferenceDescription(IPropertyReference propertyReference)
			{
				IFormatter formatter = new TextFormatter();
	
				this.WriteType(propertyReference.PropertyType, formatter, true);
				formatter.Write(" ");
	
				// Name
				string propertyName = propertyReference.Name;
				if (propertyName == "Item")
				{
					propertyName = "this";
				}
	
				formatter.Write(this.GetTypeReferenceDescription(propertyReference.DeclaringType as ITypeReference));
				formatter.Write(".");
				formatter.WriteDeclaration(propertyName);
	
				// Parameters
				IParameterDeclarationCollection parameters = propertyReference.Parameters;
				if (parameters.Count > 0)
				{
					formatter.Write("[");
					this.WriteParameterDeclarationList(parameters, formatter, null);
					formatter.Write("]");
				}
	
				formatter.Write(" ");
				formatter.Write("{ ... }");
				
				return formatter.ToString();
			}
	
			private string GetEventReferenceDescription(IEventReference eventReference)
			{
				IFormatter formatter = new TextFormatter();
	
				formatter.WriteKeyword("event");
				formatter.Write(" ");
				this.WriteType(eventReference.EventType, formatter, true);
				formatter.Write(" ");
				formatter.Write(this.GetTypeReferenceDescription(eventReference.DeclaringType as ITypeReference));
				formatter.Write(".");
				formatter.WriteDeclaration(eventReference.Name);
				formatter.Write(";");
				
				return formatter.ToString();
			}
	
			private void WriteCharLiteral(IFormatter formatter, char ch)
			{
				string text = new string(new char[] { ch });
				text = EscapeStringLiteral(text);
				formatter.WriteLiteral("\'" + text + "\'");
			}
	
			private void WriteStringLiteral(IFormatter formatter, string text)
			{
				text = EscapeStringLiteral(text);

//	[JLC] No support for verbatum (@) strings.
//				int index = text.IndexOf("\\");
//				if (index != -1)
//				{
//					index = text.Replace("\\\\", "").IndexOf("\\");
//					if (index == -1)
//					{
//						text = text.Replace("\\\\", "\\");
//						formatter.WriteLiteral("@\"" + text + "\"");
//						return;
//					}
//				}
	
				formatter.WriteLiteral("S\"" + text + "\"");				
			}
	
			private static string EscapeStringLiteral(string text)
			{
				using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
				{
					foreach (char character in text)
					{
						ushort value = (ushort) character;
						if (value > 0x00ff)
						{
							writer.Write("\\u" + value.ToString("x4"));					
						}
						else
						{
							switch (character)
							{
								case '\0':
									writer.Write("\\0");
									break;
		
								case '\a':
									writer.Write("\\a");
									break;
		
								case '\b':
									writer.Write("\\b");
									break;
		
								case '\t':
									writer.Write("\\t");
									break;
		
								case '\n':
									writer.Write("\\n");
									break;
		
								case '\v':
									writer.Write("\\v");
									break;
		
								case '\f':
									writer.Write("\\f");
									break;
		
								case '\r':
									writer.Write("\\r");
									break;
									
								case '\"':
									writer.Write("\\\"");
									break;
			
								case '\'':
									writer.Write("\\\'");
									break;
			
								case '\\':
									writer.Write("\\\\");
									break;
				
								default:
									writer.Write(character);
									break;
							}
						}
					}
					return writer.ToString();
				}
			}
	
			private bool IsConstructor(IMethodReference value)
			{
				return ((value.Name == ".ctor") || (value.Name == ".cctor"));
			}

			private bool IsEnumerationElement(IFieldReference value)
			{
				IType fieldType = value.FieldType;
				IType declaringType = value.DeclaringType;
				if (fieldType.Equals(declaringType))
				{
					ITypeReference typeReference = fieldType as ITypeReference;
					if (typeReference != null)
					{
						return Helper.IsEnumeration(typeReference.Resolve());
					}
				}

				return false;
			}

			private void WriteDeclaration(string value, IFormatter formatter)
			{
				formatter.WriteDeclaration(value);
			}

			private IType GetExpressionType(IExpression expression)
			{
				ITypeReference typeReference = this.methodContext as ITypeReference;
				if (typeReference != null)
				{
					ITypeDeclaration typeDeclaration = typeReference.Resolve();
					if (typeDeclaration != null)
					{
						return GetExpressionType(expression, typeDeclaration);
					}
				}
				
				return null;
			}

			private static bool IsType(IType value, string namespaceName, string name)
			{
				return (IsType(value, namespaceName, name, "mscorlib") || IsType(value, namespaceName, name, "sscorlib"));
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
				ICustomAttribute customAttribute = this.GetCustomAttribute(value, namespaceName, name, "mscorlib");

				if (customAttribute == null)
				{
					customAttribute = this.GetCustomAttribute(value, namespaceName, name, "sscorlib");
				}

				return customAttribute;
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


			public static bool IsSystemType(IType value)
			{
				ITypeReference typeReference = value as ITypeReference;
				if (typeReference != null)
				{
					return (typeReference.Namespace == "System");
				}
	
				IRequiredModifier requiredModifier = value as IRequiredModifier;
				if (requiredModifier != null)
				{
					return IsSystemType(requiredModifier.ElementType);
				}
	
				IOptionalModifier optionalModifier = value as IOptionalModifier;
				if (optionalModifier != null)
				{
					return IsSystemType(optionalModifier.ElementType);
				}
	
				return false;
			}

			public static IType GetExpressionType(IExpression value, ITypeDeclaration declaringType)
			{
				IArgumentReferenceExpression argumentReferenceExpression = value as IArgumentReferenceExpression;
				if (argumentReferenceExpression != null)
				{
					IParameterDeclaration paramterDeclaration = argumentReferenceExpression.Parameter.Resolve();
					return paramterDeclaration.ParameterType;
				}
	
				IFieldReferenceExpression fieldReferenceExpression = value as IFieldReferenceExpression;
				if (fieldReferenceExpression != null)
				{
					return fieldReferenceExpression.Field.FieldType;
				}
	
				IPropertyReferenceExpression propertyReferenceExpression = value as IPropertyReferenceExpression;
				if (propertyReferenceExpression != null)
				{
					return propertyReferenceExpression.Property.PropertyType;
				}
	
				IMethodInvokeExpression methodInvokeExpression = value as IMethodInvokeExpression;
				if (methodInvokeExpression != null)
				{
					return GetExpressionType(methodInvokeExpression.Method, declaringType);
				}
	
				IMethodReferenceExpression methodReferenceExpression = value as IMethodReferenceExpression;
				if (methodReferenceExpression != null)
				{
					return methodReferenceExpression.Method.ReturnType.Type;
				}
	
				ITypeReferenceExpression typeReferenceExpression = value as ITypeReferenceExpression;
				if (typeReferenceExpression != null)
				{
					return typeReferenceExpression.Type;
				}
	
				IThisReferenceExpression thisReferenceExpression = value as IThisReferenceExpression;
				if (thisReferenceExpression != null)
				{
					return declaringType;
				}
	
				IBaseReferenceExpression baseReferenceExpression = value as IBaseReferenceExpression;
				if (baseReferenceExpression != null)
				{
					return declaringType.BaseType;
				}
	
				IVariableDeclaration variableDecaration = null;
	
				IVariableReferenceExpression variableReferenceExpression = value as IVariableReferenceExpression;
				if (variableReferenceExpression != null)
				{
					variableDecaration = variableReferenceExpression.Variable.Resolve();
				}
	
				IVariableDeclarationExpression variableDeclarationExpression = value as IVariableDeclarationExpression;
				if (variableDeclarationExpression != null)
				{
					variableDecaration = variableDeclarationExpression.Variable;
				}
	
				if (variableDecaration != null)
				{
					IType variableType = variableDecaration.VariableType;
	
					if (variableDecaration.Pinned)
					{
						IReferenceType referenceType = variableType as IReferenceType;
						if (referenceType != null)
						{
							IPointerType pointerType = new PointerType();
							pointerType.ElementType = referenceType.ElementType;
							return pointerType;
						}
	
						if (IsType(variableType, "System", "String"))
						{
							IPointerType pointerType = new PointerType();
							pointerType.ElementType = GetType(declaringType, "System", "Char");
							return pointerType;
						}
					}
	
					return variableType;
				}
	
				ILiteralExpression literalExpression = value as ILiteralExpression;
				if (literalExpression != null)
				{
					if (literalExpression.Value == null)
					{
						return GetType(declaringType, "System", "Object");
					}
	
					switch (literalExpression.Value.GetType().FullName)
					{
						case "System.Boolean":
							return GetType(declaringType, "System", "Boolean");
	
						case "System.SByte":
							return GetType(declaringType, "System", "SByte");
	
						case "System.Byte":
							return GetType(declaringType, "System", "Byte");
	
						case "System.Int16":
							return GetType(declaringType, "System", "Int16");
	
						case "System.UInt16":
							return GetType(declaringType, "System", "UInt16");
	
						case "System.Int32":
							return GetType(declaringType, "System", "Int32");
	
						case "System.UInt32":
							return GetType(declaringType, "System", "UInt32");
	
						case "System.Int64":
							return GetType(declaringType, "System", "Int64");
	
						case "System.UInt64":
							return GetType(declaringType, "System", "UInt64");
	
						case "System.Single":
							return GetType(declaringType, "System", "Single");
	
						case "System.Double":
							return GetType(declaringType, "System", "Double");
	
						case "System.Decimal":
							return GetType(declaringType, "System", "Decimal");
	
						case "System.String":
							return GetType(declaringType, "System", "String");
	
						case "System.Char":
							return GetType(declaringType, "System", "Char");
					}
	
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Invalid literal type '{0}'.", literalExpression.Value.GetType().FullName));
				}
	
				IUnaryExpression unaryExpression = value as IUnaryExpression;
				if (unaryExpression != null)
				{
					switch (unaryExpression.Operator)
					{
						case UnaryOperator.BitwiseNot:
						case UnaryOperator.BooleanNot:
						case UnaryOperator.Negate:
						case UnaryOperator.PostIncrement:
						case UnaryOperator.PostDecrement:
						case UnaryOperator.PreIncrement:
						case UnaryOperator.PreDecrement:
							return GetExpressionType(unaryExpression.Expression, declaringType);
					}
				}
	
				IBinaryExpression binaryExpression = value as IBinaryExpression;
				if (binaryExpression != null)
				{
					switch (binaryExpression.Operator)
					{
						// Boolean expressions
						case BinaryOperator.GreaterThan:
						case BinaryOperator.GreaterThanOrEqual:
						case BinaryOperator.IdentityEquality:
						case BinaryOperator.IdentityInequality:
						case BinaryOperator.ValueEquality:
						case BinaryOperator.ValueInequality:
						case BinaryOperator.LessThan:
						case BinaryOperator.LessThanOrEqual:
						case BinaryOperator.BooleanAnd:
						case BinaryOperator.BooleanOr:
							return GetType(declaringType, "System", "Boolean");
	
						case BinaryOperator.Add:
						case BinaryOperator.Subtract:
						case BinaryOperator.Multiply:
						case BinaryOperator.Divide:
						case BinaryOperator.Modulus:
						case BinaryOperator.BitwiseAnd:
						case BinaryOperator.BitwiseOr:
						case BinaryOperator.BitwiseExclusiveOr:
							{
								//			sbyte	byte	short	ushort	int		uint	long	ulong	float	double	decimal
								//	sbyte	int		int		int		int		int		long	long	-		float	double	decimal	
								//	byte	int		int		int		int		int		uint	long	ulong	float	double	decimal	
								//	short	int		int		int		int		int		long	long	-		float	double	decimal	
								//	ushort	int		int		int		int 	int		uint	long	ulong	float	double	decimal	
								//	int		int		int		int		int		int		long	long	-		float	double	decimal	
								//	uint	long	uint	long	uint	long	uint	long	ulong	float	double	decimal	
								//	long	long	long	long	long	long	long	long	-		float	double	decimal	
								//	ulong	-		ulong	-		ulong	-		ulong	-		ulong	float	double	decimal	
								//	float	float	float	float	float	float	float	float	float	float	double	-
								//	double	double	double	double	double	double	double	double	double	double	double	-
								//	decimal	decimal	decimal	decimal	decimal	decimal	decimal	decimal	decimal	-		-		decimal
								
								IType type1 = GetExpressionType(binaryExpression.Left, declaringType);
								IType type2 = GetExpressionType(binaryExpression.Right, declaringType);
	
								if (IsSystemType(type1) || IsSystemType(type2))
								{
									if ((IsType(type1, "System", "Decimal")) || (IsType(type2, "System", "Decimal")))
									{
										return GetType(declaringType, "System", "Decimal");
									}
		
									if ((IsType(type1, "System", "Double")) || (IsType(type2, "System", "Double")))
									{
										return GetType(declaringType, "System", "Double");
									}
		
									if ((IsType(type1, "System", "Single")) || (IsType(type2, "System", "Single")))
									{
										return GetType(declaringType, "System", "Single");
									}
		
									if ((IsType(type1, "System", "UInt64")) || (IsType(type2, "System", "UInt64")))
									{
										return GetType(declaringType, "System", "UInt64");
									}
		
									if ((IsType(type1, "System", "Int64")) || (IsType(type2, "System", "Int64")))
									{
										return GetType(declaringType, "System", "Int64");
									}
		
									if (IsType(type1, "System", "UInt32"))
									{
										if ((IsType(type2, "System", "SByte")) || (IsType(type2, "System", "Int16")) || (IsType(type2, "System", "Int32")) || (IsType(type2, "System", "Int64")))
										{
											return GetType(declaringType, "System", "Int64");
										}
										else
										{
											return type1;
										}
									}
		
									if (IsType(type2, "System", "UInt32"))
									{
										if ((IsType(type1, "System", "SByte")) || (IsType(type1, "System", "Int16")) || (IsType(type1, "System", "Int32")) || (IsType(type1, "System", "Int64")))
										{
											return GetType(declaringType, "System", "Int64");
										}
										else
										{
											return type2;
										}
									}
		
									if ((IsType(type1, "System", "SByte")) || (IsType(type1, "System", "Byte")) || (IsType(type1, "System", "Int16")) || (IsType(type1, "System", "UInt16")) || (IsType(type1, "System", "Int32")))
									{
										if ((IsType(type2, "System", "SByte")) || (IsType(type2, "System", "Byte")) || (IsType(type2, "System", "Int16")) || (IsType(type2, "System", "UInt16")) || (IsType(type2, "System", "Int32")))
										{
											return GetType(declaringType, "System", "Int32");
										}
									}
								}
	
								// Enumeration
								switch (binaryExpression.Operator)
								{
									case BinaryOperator.BitwiseAnd:
									case BinaryOperator.BitwiseOr:
									case BinaryOperator.BitwiseExclusiveOr:
										return type1;
								}
	
								return type1;
							}
	
						case BinaryOperator.ShiftLeft:
						case BinaryOperator.ShiftRight:
							{
								//			sbyte	byte	short	ushort	int		uint	long	ulong
								//	sbyte	int		int		int		int		int		-		-		-
								//	byte	int		int		int		int		int		-		-		-
								//	short	int		int		int		int		int		-		-		-
								//	ushort	int		int		int		int 	int		-		-		-
								//	int		int		int		int		int		int		-		-		-
								//	uint	uint	uint	uint	uint	uint	-		-		-
								//	long	long	long	long	long	long	-		-		-
								//	ulong	ulong	ulong	ulong	ulong	-		-		-		-
	
								IType type = GetExpressionType(binaryExpression.Left, declaringType);
								if ((IsType(type, "System", "SByte")) || (IsType(type, "System", "Byte")) || (IsType(type, "System", "Int16")) || (IsType(type, "System", "UInt16")) || (IsType(type, "System", "Int32")))
								{
									return GetType(declaringType, "System", "Int32");
								}
	
								return type;
							}
					}
	
					throw new NotSupportedException(binaryExpression.Operator.ToString());
				}
	
				IObjectCreateExpression objectCreateExpression = value as IObjectCreateExpression;
				if (objectCreateExpression != null)
				{
					return objectCreateExpression.Constructor.DeclaringType;
				}
	
				IArrayIndexerExpression arrayIndexerExpression = value as IArrayIndexerExpression;
				if (arrayIndexerExpression != null)
				{
					IType targetType = GetExpressionType(arrayIndexerExpression.Target, declaringType);
	
					IReferenceType referenceType = targetType as IReferenceType;
					if (referenceType != null)
					{
						// 	public void Foo(out Bar[] bar, int index);
						//	{
						//		bar[index] = ...; // target type should be Bar
						//	}
						targetType = referenceType.ElementType;
					}
	
					IArrayType arrayType = targetType as IArrayType;
					if (arrayType != null)
					{
						return arrayType.ElementType;
					}
	
					IPointerType pointerType = targetType as IPointerType;
					if (pointerType != null)
					{
						return pointerType.ElementType;
					}
	
					return targetType;
				}
	
				IPropertyIndexerExpression propertyIndexerExpression = value as IPropertyIndexerExpression;
				if (propertyIndexerExpression != null)
				{
					IType targetType = GetExpressionType(propertyIndexerExpression.Target, declaringType);
					return targetType;
				}
	
				IConditionExpression conditionExpression = value as IConditionExpression;
				if (conditionExpression != null)
				{
					// Assert: TrueExpression and FalseExpression evaluate to the same type.
					if (conditionExpression.Then != null)
					{
						return GetExpressionType(conditionExpression.Then, declaringType);
					}
	
					if (conditionExpression.Else != null)
					{
						return GetExpressionType(conditionExpression.Else, declaringType);
					}
	
					throw new NotSupportedException();
				}
	
				INullCoalescingExpression nullCoalescingExpression = value as INullCoalescingExpression;
				if (nullCoalescingExpression != null)
				{
					return GetExpressionType(nullCoalescingExpression.Expression, declaringType);
				}
	
				IAddressDereferenceExpression addressDereferenceExpression = value as IAddressDereferenceExpression;
				if (addressDereferenceExpression != null)
				{
					IType type = GetExpressionType(addressDereferenceExpression.Expression, declaringType);
					IPointerType pointerType = type as IPointerType;
					if (pointerType != null)
					{
						return pointerType.ElementType;
					}
	
					return type;
				}
	
				IAddressOfExpression addressOfExpression = value as IAddressOfExpression;
				if (addressOfExpression != null)
				{
					IPointerType pointerType = new PointerType();
					pointerType.ElementType = GetExpressionType(addressOfExpression.Expression, declaringType);
					return pointerType;
				}
	
				IAddressReferenceExpression addressReferenceExpression = value as IAddressReferenceExpression;
				if (addressReferenceExpression != null)
				{
					IReferenceType referenceType = new ReferenceType();
					referenceType.ElementType = GetExpressionType(addressReferenceExpression.Expression, declaringType);
					return referenceType;
				}
	
				IAddressOutExpression addressOutExpression = value as IAddressOutExpression;
				if (addressOutExpression != null)
				{
					IReferenceType referenceType = new ReferenceType();
					referenceType.ElementType = GetExpressionType(addressOutExpression.Expression, declaringType);
					return referenceType;
				}
	
				ITypeOfExpression typeOfExpression = value as ITypeOfExpression;
				if (typeOfExpression != null)
				{
					return GetType(declaringType, "System", "Type");
				}
	
				ISizeOfExpression sizeOfExpression = value as ISizeOfExpression;
				if (sizeOfExpression != null)
				{
					return GetType(declaringType, "System", "Int32");
				}
	
				ICanCastExpression canCastExpression = value as ICanCastExpression;
				if (canCastExpression != null)
				{
					return GetType(declaringType, "System", "Boolean");
				}
	
				ICastExpression castExpression = value as ICastExpression;
				if (castExpression != null)
				{
					return castExpression.TargetType;
				}
	
				ITryCastExpression tryCastExpression = value as ITryCastExpression;
				if (tryCastExpression != null)
				{
					return tryCastExpression.TargetType;
				}
	
				IArrayCreateExpression arrayCreateExpression = value as IArrayCreateExpression;
				if (arrayCreateExpression != null)
				{
					IArrayType arrayType = new ArrayType();
					arrayType.ElementType = arrayCreateExpression.Type;
					// TODO Dimensions
					return arrayType;
				}
	
				IDelegateCreateExpression delegateCreateExpression = value as IDelegateCreateExpression;
				if (delegateCreateExpression != null)
				{
					return delegateCreateExpression.DelegateType;
				}
	
				IStackAllocateExpression stackAllocateExpression = value as IStackAllocateExpression;
				if (stackAllocateExpression != null)
				{
					return stackAllocateExpression.Type;
				}
	
				IAssignExpression assignExpression = value as IAssignExpression;
				if (assignExpression != null)
				{
					return GetExpressionType(assignExpression.Expression, declaringType);
				}
	
				ITypeOfTypedReferenceExpression typeOfTypedReferenceExpression = value as ITypeOfTypedReferenceExpression;
				if (typeOfTypedReferenceExpression != null)
				{
					return GetType(declaringType, "System", "Type");
				}
	
				IValueOfTypedReferenceExpression valueOfTypedReferenceExpression = value as IValueOfTypedReferenceExpression;
				if (valueOfTypedReferenceExpression != null)
				{
					return GetExpressionType(valueOfTypedReferenceExpression.Expression, declaringType);
				}
	
				ITypedReferenceCreateExpression typedReferenceCreateExpression = value as ITypedReferenceCreateExpression;
				if (typedReferenceCreateExpression != null)
				{
					return GetType(declaringType, "System", "TypedReference");
				}
	
				ISnippetExpression snippetExpression = value as ISnippetExpression;
				if (snippetExpression != null)
				{
					return GetType(declaringType, "System", "Object");
				}
	
				IArgumentListExpression argumentListExpression = value as IArgumentListExpression;
				if (argumentListExpression != null)
				{
					return GetType(declaringType, "System", "RuntimeArgumentHandle");
				}
	
				IGenericDefaultExpression genericDefaultExpression = value as IGenericDefaultExpression;
				if (genericDefaultExpression != null)
				{
					return genericDefaultExpression.GenericArgument;
				}
	
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Unkown expression type '{0}'.", value.GetType().FullName));
			}

			public static ITypeReference GetType(ITypeDeclaration context, string namespaceName, string name)
			{
				object owner = context.Owner;
				while (owner is ITypeDeclaration)
				{
					ITypeDeclaration declaringType = (ITypeDeclaration) owner;
					owner = declaringType.Owner;
				}
				
				IModule module = owner as IModule;
				if (module != null)
				{
					IAssemblyReference assemblyReference = module.Assembly;
					if ((assemblyReference.Name != "mscorlib") && (assemblyReference.Name != "sscorlib"))
					{
						foreach (IAssemblyReference current in module.AssemblyReferences)
						{
							if ((current.Name == "mscorlib") || (current.Name == "sscorlib"))
							{
								assemblyReference = current;
								break;
							}
						}
					}
	
					ITypeReference typeReference = new TypeReference();
					typeReference.Owner = assemblyReference;
					typeReference.Namespace = namespaceName;
					typeReference.Name = name;
					return typeReference;
				}
	
				return null;
			}

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
		
				public void WriteReference(string text, string toolTip, object reference)
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
	}
}


// TODO
// Unsafe method signatures.
// Method parameters with keyword name.
// Filtered out event backing fields.
