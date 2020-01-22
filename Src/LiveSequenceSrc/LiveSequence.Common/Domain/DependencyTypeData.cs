namespace LiveSequence.Common.Domain
{
    public class DependencyTypeData
    {
        public string TypeName { get; set; }

        public string ImplTypeName { get; set; }

        public SelectionType SelectionType { get; set; }


        public DependencyTypeData(string typeName, string implTypeName, SelectionType sType)
        {
            this.TypeName = typeName;
            this.ImplTypeName = implTypeName;
            this.SelectionType = sType;
        }
    }
}