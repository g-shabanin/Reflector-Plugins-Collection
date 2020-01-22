using System;
using System.Collections.Generic;
using LiveSequence.Common.Domain;
using LiveSequence.Tree;
using Mono.Cecil;

namespace LiveSequence.Engine
{
    internal interface IAssemblyParser
    {
        String AssemblyFileName { get; set; }

        CecilAssemblyData AssemblyData { get; set; }

        List<TypeDefinition> GetAssemblyTypes(AssemblyDefinition asmDef);

        List<TypeDefinition> GetAssemblyTypes(string assemblyFileName);

        List<MethodDefinition> GetTypeMethods(TypeDefinition typeDefinition);

        DTreeNode<DTreeItem> Initialize(string assemblyName);

        void CleanUp();

        SequenceData GetSequenceData(string methodName, string typeName, string nameSpace, string assemblyName);

        event EventHandler<ProgressEventArgs> OnProgressChanged;

        AssemblyStats GetAssemblyStats();

        List<SequenceData> GetSequenceData();

        List<SequenceData> GetSequenceData(string typeName);

        DTreeNode<DependencyGraphData> GetDependencyGraphData(TreeSelectionData selectedData);
    }

    internal class ProgressEventArgs : EventArgs
    {
        private string _message = string.Empty;

        public ProgressEventArgs(string message)
        {
            Message = message;
        }

        internal string Message
        {
            get { return _message; }
            private set { _message = value; }
        }
    }
}