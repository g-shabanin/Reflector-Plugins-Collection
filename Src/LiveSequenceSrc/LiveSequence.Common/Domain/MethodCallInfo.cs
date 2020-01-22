namespace LiveSequence.Common.Domain
{
    public class MethodCallInfo
    {
        public string StartMethod { get; set; }

        public string TypeName { get; set; }

        public string MethodName { get; set; }

        public string MethodCallName { get; set; }

        public string MethodCallType { get; set; }

        public string MethodCallNamespace { get; set; }

        public string TargetMethodAssembly { get; set; }

        public override string ToString()
        {
            string sb = string.Format("MethodCallInfo :: StartMethod:{0} TypeName: {1}, MethodName: {2}, MethodCallType: {3}, MethodCallName: {4}, TargetMethodAssembly:{5}",
                                      this.StartMethod, this.TypeName, this.MethodName, this.MethodCallType, this.MethodCallName, this.TargetMethodAssembly);

            return sb;
        }
    }
}