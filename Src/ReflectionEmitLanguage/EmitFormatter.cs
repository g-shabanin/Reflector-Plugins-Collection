using System;
using Reflector.CodeModel;

namespace Reflector.ReflectionEmitLanguage
{
    internal sealed class EmitFormatter : IFormatter
    {
        private IFormatter formatter;
        public EmitFormatter(IFormatter formatter)
        {
            this.formatter = formatter;
        }

        public IFormatter Formatter
        {
            get
            {
                return this.formatter;
            }
        }

        public void Write(string value)
        {
            this.formatter.Write(value);
        }

        public void Write(string format, params object[] args)
        {
            string value = String.Format(format, args);
            this.Write(value);
        }

        public void WriteComment(string value)
        {
            string comment = String.Format("// {0}", value);
            this.formatter.WriteComment(comment);
            this.formatter.WriteLine();
        }
        public void WriteComment(string format, params object[] args)
        {
            string comment = String.Format(format, args);
            this.WriteComment(comment);
        }

        public void WriteDeclaration(string value)
        {
            this.formatter.WriteDeclaration(value);
        }

        public void WriteDeclaration(string format, params object[] args)
        {
            string value = String.Format(format, args);
            this.WriteDeclaration(value);
        }

        public void WriteDeclaration(string value, object target)
        {
            this.WriteDeclaration(value);
        }

        public void WriteIndent()
        {
            this.formatter.WriteIndent();
        }

        public void WriteKeyword(string value)
        {
            this.formatter.WriteKeyword(value);
        }

        public void WriteLine()
        {
            this.formatter.WriteLine();
        }

        public void WriteLiteral(string value)
        {
            string literal = String.Format("\"{0}\"", value);
            this.formatter.WriteLiteral(literal);
        }
        public void WriteLiteral(string format, params object[] args)
        {
            string value = String.Format(format, args);
            this.WriteLiteral(value);
        }

        public void WriteOutdent()
        {
            this.formatter.WriteOutdent();
        }

        public void WriteProperty(string name, string value)
        {
            this.formatter.WriteProperty(name, value);
        }

        public void WriteReference(string value, string description, object target)
        {
            this.formatter.WriteReference(value, description, target);
        }

        public void WriteEndStatement()
        {
            this.formatter.Write(";");
            this.formatter.WriteLine();
        }

        public void WriteMethodInvocation(
            string instanceName,
            string methodName,
            bool endStatement,
            params string[] args
            )
        {
            this.Write("{0}.{1}(", instanceName, methodName);
            if (args.Length > 3)
            {
                this.formatter.WriteLine();
                this.formatter.WriteIndent();
                for (int i = 0; i < args.Length; ++i)
                {
                    if (i != 0)
                        this.formatter.Write(",");
                    this.formatter.Write(args[i]);
                    this.formatter.WriteLine();
                }
                this.formatter.Write(")");
                if (endStatement)
                    this.formatter.Write(";");
                this.formatter.WriteOutdent();
                if (endStatement)
                    this.formatter.WriteLine();
            }
            else
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    if (i != 0)
                        this.formatter.Write(",");
                    this.formatter.Write(args[i]);
                }
                this.formatter.Write(")");
                if (endStatement)
                    this.WriteEndStatement();
            }
        }

        public void WriteVariableDeclaration(
            string variableType,
            string variableName
            )
        {
            this.Write("{0} {1}", variableType, variableName);
        }

        public void WriteVariableDeclaration(
            string variableType,
            string variableName,
            object target
            )
        {
            this.Write("{0} ", variableType);
            this.WriteReference(variableName, "", target);
        }

        public void WriteEqual()
        {
            this.formatter.Write(" = ");
        }

        public void WriteTypeOf(Reflector.CodeModel.IType type)
        {
            string typeName = null;
            Reflector.CodeModel.ITypeReference typeReference = type as Reflector.CodeModel.ITypeReference;
            if (typeReference != null)
            {
                typeName = Helper.GetNameWithResolutionScope(typeReference);
            }
            else
                typeName = type.ToString();

            this.formatter.WriteKeyword("typeof");
            this.Write("(");
            this.WriteReference(typeName, "", type);
            this.Write(")");
        }

        public void WriteAssignType(string instance, string property, Reflector.CodeModel.IType type)
        {
            this.Write("{0}.{1}", instance, property);
            this.WriteEqual();
            this.WriteKeyword("new");
            this.Write(" ");
            this.Write("TypeTypeReference(");
            this.WriteTypeOf(type);
            this.Write(")");
            this.WriteEndStatement();
        }

        public void WriteVisibility(string instance, Reflector.CodeModel.MethodVisibility visibility)
        {
            if ((visibility & Reflector.CodeModel.MethodVisibility.Assembly) != 0)
            {
                this.Write(
                    "{0}.Attributes |= System.CodeDom.MethodVisibility.Assembly",
                    instance);
                this.WriteEndStatement();
            }
            if ((visibility & Reflector.CodeModel.MethodVisibility.Family) != 0)
            {
                this.Write(
                    "{0}.Attributes |= System.CodeDom.MethodVisibility.Family",
                    instance);
                this.WriteEndStatement();
            }
            if ((visibility & Reflector.CodeModel.MethodVisibility.Private) != 0)
            {
                this.Write(
                    "{0}.Attributes |= System.CodeDom.MethodVisibility.Private",
                    instance);
                this.WriteEndStatement();
            }
        }
    }
}
