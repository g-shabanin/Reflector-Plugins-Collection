using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using LiveSequence.Common;
using LiveSequence.Common.Domain;
using LiveSequence.Tree;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace LiveSequence.Engine
{
    internal class CecilAssemblyParser : IAssemblyParser
    {
        private readonly List<String> methodGraph = new List<string>();
        private readonly List<String> processedAssemblies = new List<string>();

        #region IAssemblyParser Members

        public string AssemblyFileName { get; set; }

        public CecilAssemblyData AssemblyData { get; set; }

        public DTreeNode<DTreeItem> Initialize(string assemblyFileName)
        {
            AssemblyFileName = assemblyFileName;

            AssemblyDefinition asmDefinition = GetAssemblyDefinition(AssemblyFileName);

            // build the data for the main assembly
            BuildAssemblyData(asmDefinition);

            if (Settings.IncludeAssemblyReferences)
            {
                // iterates over all the referenced assembly recursively and build the data
                BuildReferencedAssemblyData(asmDefinition);
            }

            CallEvent("Updating Assembly Stats");
            // update Stats
            AssemblyData.UpdateStats(asmDefinition);

            return AssemblyData.AssemblyTree;
        }

        public List<TypeDefinition> GetAssemblyTypes(string assemblyFileName)
        {
            return GetAssemblyTypes(GetAssemblyDefinition(assemblyFileName));
        }

        public List<TypeDefinition> GetAssemblyTypes(AssemblyDefinition asmDef)
        {
            var types = new List<TypeDefinition>();
            foreach (TypeDefinition t in asmDef.MainModule.Types)
            {
                if (Rules.IsValidType(t.Name))
                {
                    types.Add(t);
                }
            }

            return types;
        }

        public List<MethodDefinition> GetTypeMethods(TypeDefinition tDef)
        {
            var methods = new List<MethodDefinition>();

            foreach (MethodDefinition c in tDef.Constructors)
            {
                methods.Add(c);
            }

            foreach (MethodDefinition m in tDef.Methods)
            {
                if (m != null && Rules.IsValidMethod(m.Name))
                {
                    methods.Add(m);
                }
            }

            return methods;
        }

        public void CleanUp()
        {
            AssemblyData = new CecilAssemblyData();
        }

        public SequenceData GetSequenceData(string methodName, string typeName, string nameSpace, string assemblyName)
        {
            // here match the methodName in the methodcalllist structure
            // and populate the SequenceData field.
            var data = new SequenceData(typeName + methodName);
            data.AddObject(typeName);

            SelectMethod(methodName, typeName, nameSpace, assemblyName, data);

            return data;
        }

        public event EventHandler<ProgressEventArgs> OnProgressChanged;

        public AssemblyStats GetAssemblyStats()
        {
            return AssemblyData.AssemblyStats;
        }

        public List<SequenceData> GetSequenceData()
        {
            var sequenceDataList = new List<SequenceData>();

            foreach (TypeDefinition typeDefinition in this.AssemblyData.AssemblyDefinition.MainModule.Types)
            {
                string typeName = string.IsNullOrEmpty(typeDefinition.Namespace) ? typeDefinition.Name : typeDefinition.Namespace + "." + typeDefinition.Name;
                sequenceDataList.AddRange(this.GetSequenceData(typeName));
            }

            return sequenceDataList;
        }

        public List<SequenceData> GetSequenceData(string typeName)
        {
            var seqData = new List<SequenceData>();

            TypeDefinition typeDef = AssemblyData.AssemblyDefinition.MainModule.Types[typeName];
            foreach (MethodDefinition mDef in GetTypeMethods(typeDef))
            {
              seqData.Add(GetSequenceData(CecilAssemblyHelper.CreateNormalizeMethodDefinition(mDef).ToString(), typeDef.Name,
                                            mDef.DeclaringType.Namespace, AssemblyData.AssemblyDefinition.Name.Name));
            }

            return seqData;
        }

        public DTreeNode<DependencyGraphData> GetDependencyGraphData(TreeSelectionData selectedData)
        {
            return AssemblyData.GetAssemblyGraph(selectedData);
        }

        #endregion

        private void BuildReferencedAssemblyData(AssemblyDefinition asmDef)
        {
            // check the assembly references
            foreach (AssemblyNameReference asmRef in asmDef.MainModule.AssemblyReferences)
            {
                if (Rules.IsValidAssembly(asmRef.Name) && !AlreadyProcessed(asmRef.Name))
                {
                    Logger.Current.Debug(">> Referenced Assemblies:" + asmRef.FullName);
                    // see if there is a file with asmRef.dll name
                    // construct path
                    // get the directory from the assemblyfilename
                    string referencedAssemblyPath = Path.Combine(Path.GetDirectoryName(AssemblyFileName),
                                                                 asmRef.Name + ".dll");

                    if (File.Exists(referencedAssemblyPath))
                    {
                        Logger.Current.Debug(">> Referenced Assembly Exists");
                        AssemblyDefinition referencedAssemblyDef = GetAssemblyDefinition(referencedAssemblyPath);

                        // build the data
                        BuildAssemblyData(referencedAssemblyDef);

                        // recursive call
                        BuildReferencedAssemblyData(referencedAssemblyDef);
                    }
                    else
                    {
                        Logger.Current.Debug(">> Unable to find referenced assembly. Ask user to select");
                    }

                    // add to processed list
                    processedAssemblies.Add(asmRef.Name);
                }
            }
        }

        /// <summary>
        /// Maintains a list of already processed assemblies.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private bool AlreadyProcessed(IEquatable<string> assemblyName)
        {
            foreach (string processedAssembly in processedAssemblies)
            {
                if (assemblyName.Equals(processedAssembly))
                {
                    return true;
                }
            }

            return false;
        }

        private static AssemblyDefinition GetAssemblyDefinition(string assemblyFileName)
        {
            return AssemblyFactory.GetAssembly(assemblyFileName);
        }

        private void BuildAssemblyData(AssemblyDefinition asmDef)
        {
            var namespaces = new List<string>();

            AssemblyData.AddNewAssembly(asmDef);
            CallEvent("Assembly:" + asmDef.Name.Name);
            foreach (TypeDefinition tDef in GetAssemblyTypes(asmDef))
            {
                TypeDefinition tDef1 = tDef;
                if (!namespaces.Exists(
                         s => s.Equals(tDef1.Namespace)
                         ))
                {
                    AssemblyData.AddNewNamespace(tDef.Namespace);
                    namespaces.Add(tDef.Namespace);
                }

                AssemblyData.AddNewType(tDef);
                CallEvent("Type:" + tDef.FullName);

                foreach (MethodDefinition mDef in GetTypeMethods(tDef))
                {
                    AssemblyData.AddNewMethod(mDef); // add before recursion
                    CallEvent("Method:" + mDef.Name);
                    ParseMethodBody(tDef.Name, mDef,
                                    mDef.DeclaringType.FullName + "." + CecilAssemblyHelper.CreateNormalizeMethodDefinition(mDef));

                    // clear methodGraph
                    methodGraph.Clear();
                }
            }
        }

        private void ParseMethodBody(string typeName, MethodDefinition mDef, string startMethod)
        {
            if (mDef.HasBody)
            {
                for (int i = 0; i < mDef.Body.Instructions.Count; i++) //  (Instruction ins in mDef.Body.Instructions)
                {
                    Instruction ins = mDef.Body.Instructions[i];
                    try
                    {
                        if (ins.OpCode.Name == "call" || ins.OpCode.Name == "callvirt")
                        {
                            MethodDefinition childMethodDef = AssemblyData.AddNewCall(typeName, mDef, ins.Operand, startMethod);

                            if (childMethodDef != null)
                            {
                                string currentMethod = mDef.DeclaringType.Name + "." +
                                                       CecilAssemblyHelper.CreateNormalizeMethodDefinition(mDef);
                                // add current method to the methodGraph...to detect infinite loops
                                methodGraph.Add(currentMethod);

                                string childMethod = childMethodDef.DeclaringType.Name + "." +
                                                     CecilAssemblyHelper.CreateNormalizeMethodDefinition(childMethodDef);

                                bool methodExists = methodGraph.Exists(
                                    methodName => methodName.Equals(childMethod)
                                    );

                                // check for infinite loop
                                if (methodExists)
                                {
                                    Logger.Current.Debug(
                                        string.Format(
                                            "Infinite Loop Detected: StartMethod:{0}, CurrentMethod:{1}, ChildMethod:{2}",
                                            startMethod, currentMethod, childMethod));
                                }
                                else
                                {
                                    ParseMethodBody(childMethodDef.DeclaringType.Name, childMethodDef, startMethod);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Found a problem:" + ex);
                        Logger.Current.Debug(ex.ToString());
                    }
                }
            }
        }

        private void SelectMethod(string methodName, string typeName, string nameSpace, string assemblyName,
                                  SequenceData data)
        {
            foreach (MethodCallInfo mInfo in AssemblyData.MethodCallList)
            {
                if (!mInfo.StartMethod.Equals(nameSpace + "." + typeName + "." + methodName)) 
                    continue;
                
                data.AddObject(mInfo.MethodCallType);
                data.AddMessage(mInfo);
                Logger.Current.Debug(">> Found method:" + mInfo);

                // if targetAssembly is different from assemblyName then recursively call this method
                var targetAssembly = Helper.RemoveExtension(Helper.RemoveExtension(mInfo.TargetMethodAssembly, ".dll"), ".exe");
                if (!targetAssembly.Equals(assemblyName))
                {
                    SelectMethod(mInfo.MethodCallName, mInfo.MethodCallType, mInfo.MethodCallNamespace,
                                 mInfo.TargetMethodAssembly, data);
                }
               
            }
        }

        private void CallEvent(string msg)
        {
            if (OnProgressChanged != null)
            {
                OnProgressChanged(null, new ProgressEventArgs(msg));
            }
        }

        public CecilAssemblyParser()
        {
            AssemblyFileName = string.Empty;
            AssemblyData = new CecilAssemblyData();
        }
    }
}