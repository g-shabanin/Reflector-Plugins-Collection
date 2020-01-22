using System.Collections.Generic;

namespace LiveSequence.Common.Domain
{
    public class AssemblyStats
    {
        public string AssemblyName { get; set; }

        public string EntryPoint { get; set; }

        public int TotalNameSpaces { get; set; }

        private int _totalTypes;

        public int TotalTypes
        {
            get { return _totalTypes; }
            set { _totalTypes = value - 1; } // ignoring the <module>
        }

        public int TotalAsmReferences { get; set; }

        public int TotalAssemblies { get; set; }

        private List<NamespaceData> _namespaceList = new List<NamespaceData>();

        public List<NamespaceData> NamespaceList
        {
            get { return _namespaceList; }
            set { _namespaceList = value; }
        }

        private List<TypeData> _typeList = new List<TypeData>();

        public List<TypeData> TypeList
        {
            get { return _typeList; }
            set { _typeList = value; }
        }

        private List<MethodData> _methodList = new List<MethodData>();

        public List<MethodData> MethodList
        {
            get { return _methodList; }
            set { _methodList = value; }
        }	
    }
}