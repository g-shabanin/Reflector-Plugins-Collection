namespace LiveSequence.Common.Domain
{
    /// <summary>
    /// Normalize the method definition into the following standard format
    /// MethodName(Parameters) : Return Type
    /// </summary>
    public class NormalizeMethodDefinition
    {
        public NormalizeMethodDefinition(string fullTypeName, string nameSpace, string methodName,
                                         string parameterList, string returnType,
                                         string assemblyName)
        {
            DeclaringTypeFullName = fullTypeName;
            DeclaringTypeNamespace = nameSpace;
            Name = methodName;
            ParameterList = parameterList;
            ReturnType = returnType;
            AssemblyName = assemblyName;
        }

        public string DeclaringTypeFullName { get; set; }

        public string DeclaringTypeNamespace { get; set; }

        public string Name { get; set; }

        public string ParameterList { get; set; }

        public string ReturnType { get; set; }

        public string AssemblyName { get; set; }

        public override string ToString()
        {
            return string.Format("{0}({1}) : {2}", Name, ParameterList, ReturnType);
        }
    }
}