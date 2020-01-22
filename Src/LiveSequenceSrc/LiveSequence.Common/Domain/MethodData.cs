namespace LiveSequence.Common.Domain
{
    public class MethodData
    {
        public string AssemblyName { get; set; }

        public string NamespaceText { get; set; }

        public string TypeName { get; set; }

        public string MethodName { get; set; }

        public MethodData(string assemblyName, string namespaceText, string typeName, string methodName)
        {
            this.AssemblyName = assemblyName;
            this.NamespaceText = namespaceText;
            this.TypeName = typeName;
            this.MethodName = methodName;
        }

    }
}