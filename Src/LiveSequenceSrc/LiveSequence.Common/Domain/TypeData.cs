using System.Collections.Generic;

namespace LiveSequence.Common.Domain
{
    public class TypeData
    {
        public TypeData(string assemblyName, string namespaceText, string typeName, int totalMethods, IEnumerable<string> fields, IEnumerable<string> properties)
        {
            AssemblyName = assemblyName;
            NamespaceText = namespaceText;
            TypeName = typeName;
            TotalMethods = totalMethods;
            Fields = new List<string>(fields);
            Properties = new List<string>(properties);
        }

        public string AssemblyName { get; set; }

        public string NamespaceText { get; set; }

        public string TypeName { get; set; }

        public int TotalMethods { get; set; }

        public List<string> Fields { get; set; }

        public List<string> Properties { get; set; }

        public override string ToString()
        {
            return NamespaceText + "." + TypeName;
        }
    }
}