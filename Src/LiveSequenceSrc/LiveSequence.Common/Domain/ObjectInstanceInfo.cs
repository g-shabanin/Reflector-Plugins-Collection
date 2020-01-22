namespace LiveSequence.Common.Domain
{
    public class ObjectInstanceInfo
    {
        public string TypeName { get; set; }

        public string Key { get; set; }

        public ObjectInstanceInfo(string typeName, string key)
        {
            this.TypeName = typeName;
            this.Key = key;
        }

        public override string ToString()
        {
            string sb = string.Format("ObjectInstanceInfo :: Adding TypeName: {0}, Key: {1}",
                                      this.TypeName, this.Key);
            return sb;
        }
	
    }
}