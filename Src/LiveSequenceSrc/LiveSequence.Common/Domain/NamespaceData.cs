namespace LiveSequence.Common.Domain
{
    public class NamespaceData
    {
        public string AssemblyName { get; set; }

        public string NamespaceText { get; set; }

        public int TotalTypes { get; set; }

        public NamespaceData(string assemblyName, string namespaceText, int totalTypes)
        {
            this.AssemblyName = assemblyName;
            this.NamespaceText = namespaceText;
            this.TotalTypes = totalTypes;
        }
    }
}