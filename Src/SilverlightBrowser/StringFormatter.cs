namespace Reflector.SilverlightBrowser
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Reflector.CodeModel;
	using System.Windows.Forms;

	public class Config : ILanguageWriterConfiguration
    {
        public class VisibilityConfiguation : IVisibilityConfiguration
        {
            #region IVisibilityConfiguration Members

            public bool Assembly
            {
                get { return true; }
            }

            public bool Family
            {
                get { return true; }
            }

            public bool FamilyAndAssembly
            {
                get { return true; }
            }

            public bool FamilyOrAssembly
            {
                get { return true; }
            }

            public bool Private
            {
                get { return true; }
            }

            public bool Public
            {
                get { return true; }
            }

            #endregion
        }
        #region ILanguageWriterConfiguration Members

        public IVisibilityConfiguration Visibility
        {
            get 
            {
				return new VisibilityConfiguation();
            }
        }

        public string this[string name]
        {
            get 
            {
                switch (name)
                {
                    case "ShowDocumentation":
                    case "ShowCustomAttributes":
                    case "ShowNamespaceImports":
                    case "ShowNamespaceBody":
                    case "ShowTypeDeclarationBody":
                    case "ShowMethodDeclarationBody":
                        return "true";
                }
                return "false";
            }
        }

        #endregion
    }

    public class StringFormatter : IFormatter
    {
        bool newline = false;
        int indent = 0;

        private void ApplyIndent()
        {
            if (this.newline)
            {
                for (int i = 0; i < this.indent; i++)
                {
                    this.sb.Append("    ");
                }
                this.newline = false;
            }
        }

        StringBuilder sb = new StringBuilder();

        public string Value
        {
            get 
            {
                return sb.ToString(); 
            }
        }


        #region IFormatter Members

        public void Write(string value)
        {
            this.ApplyIndent();
            sb.Append(value);
        }

        public void WriteComment(string value)
        {
            sb.Append(value);
        }

        public void WriteDeclaration(string value, object target)
        {
            this.ApplyIndent();
            sb.Append(value);
        }

        public void WriteDeclaration(string value)
        {
            this.ApplyIndent();
            sb.Append(value);
        }

        public void WriteIndent()
        {
            this.indent++;           
        }

        public void WriteKeyword(string value)
        {
            this.ApplyIndent();
            sb.Append(value);
        }

        public void WriteLine()
        {
            this.sb.Append("\r\n");
            this.newline = true;
        }

        public void WriteLiteral(string value)
        {
            this.ApplyIndent();
            sb.Append(value);
        }

        public void WriteOutdent()
        {

            this.indent--;
        }

        public void WriteProperty(string name, string value)
        {
            this.ApplyIndent();
            /*
			return;
            if(name != "Assembly")
                sb.Append(value);
			*/
        }

        public void WriteReference(string value, string description, object target)
        {
            this.ApplyIndent();
            this.sb.Append(value);
        }

        #endregion

        internal void Clear()
        {
            sb = new StringBuilder();
        }
    }
}
