using System;
using System.Collections.Generic;
using LiveSequence.Common;
using LiveSequence.Common.Domain;
using LiveSequence.Tree;
using Mono.Cecil;

namespace LiveSequence.Engine
{
    internal class CecilAssemblyData
    {
        private readonly DTreeNode<DTreeItem> _assemblyTree = new DTreeNode<DTreeItem>();

        private readonly Dictionary<string, DTreeNode<DependencyGraphData>> graphData =
            new Dictionary<string, DTreeNode<DependencyGraphData>>();

        private readonly DTreeNode<DTreeItem> _rootNode = new DTreeNode<DTreeItem>();
        private readonly List<DependencyTypeData> typeDependData = new List<DependencyTypeData>();
        private readonly Dictionary<string, TypeData> typeFieldData = new Dictionary<string, TypeData>();
        private DTreeNode<DTreeItem> currentAssemblyNode = new DTreeNode<DTreeItem>();
        private DTreeNode<DTreeItem> currentNamespaceNode = new DTreeNode<DTreeItem>();
        private DTreeNode<DTreeItem> currentTypeNode = new DTreeNode<DTreeItem>();

        public List<MethodCallInfo> MethodCallList = new List<MethodCallInfo>();

        public CecilAssemblyData()
        {
            _rootNode = _assemblyTree.Nodes.Add(new DTreeItem("...", SelectionType.NONE));
            // add a default entry into the graph HT
            graphData.Add("", null);
            AssemblyStats = new AssemblyStats();
        }

        public DTreeNode<DTreeItem> AssemblyTree
        {
            get { return _assemblyTree; }
        }

        public AssemblyStats AssemblyStats { get; set; }

        public AssemblyDefinition AssemblyDefinition { get; set; }

        internal MethodDefinition AddNewCall(string typeName, MethodDefinition mDef, object callDef, string startMethod)
        {
            Logger.Current.Debug("AssemblyData :: AddNewCall");

            // check whether the call and the current type is valid
            if (Rules.IsValidCall(callDef, typeName, new CallRules()))
            {
                if (mDef != null)
                {
                    var mInfo = new MethodCallInfo
                                    {
                                        StartMethod = startMethod,
                                        TypeName = typeName,
                                        MethodName = CecilAssemblyHelper.CreateNormalizeMethodDefinition(mDef).ToString()
                                    };

                    mDef = callDef as MethodDefinition;

                    NormalizeMethodDefinition norm;

                    if (mDef == null)
                    {
                        var callRef = callDef as MethodReference;
                        // method reference, just add the callee, we'll search for the referenced assembly later
                        norm = CecilAssemblyHelper.CreateNormalizeMethodDefinition(callRef);
                    }
                    else
                    {
                      norm = CecilAssemblyHelper.CreateNormalizeMethodDefinition(mDef);
                    }

                    mInfo.MethodCallType = norm.DeclaringTypeFullName;
                    mInfo.MethodCallNamespace = norm.DeclaringTypeNamespace;
                    mInfo.MethodCallName = norm.ToString();
                    mInfo.TargetMethodAssembly = norm.AssemblyName;

                    // check whether the calling type is in the ignored list
                    if (Rules.IsValidType(mInfo.MethodCallType))
                    {
                        MethodCallList.Add(mInfo);

                        Logger.Current.Debug(">> Adding Method:" + mInfo);
                    }
                    else
                    {
                        Logger.Current.Debug(">> MethodCallType: " + mInfo.MethodCallType + " in ignored list");
                    }
                }
            }
            else
            {
                mDef = null;
            }

            return mDef;
        }

        internal void AddNewAssembly(AssemblyDefinition asmDef)
        {
            currentAssemblyNode = _rootNode.Nodes.Add(new DTreeItem(asmDef.Name.Name, SelectionType.ASSEMBLY));

            DTreeNode<DependencyGraphData> asmRootNode = GetDependencyGraphNode(
                new DTreeItem(asmDef.Name.Name, SelectionType.ASSEMBLY), GraphDataArrow.NONE, "");

            // build graph data for assembly
            // reqd. Assembly References
            foreach (AssemblyNameReference asmRef in asmDef.MainModule.AssemblyReferences)
            {
                asmRootNode.Nodes.Add(GetDependencyGraphNode(
                                          new DTreeItem(asmRef.Name, SelectionType.ASSEMBLY), GraphDataArrow.TO,
                                          "references"));
            }

            graphData.Add(asmDef.Name.Name, asmRootNode);
        }

        private static DTreeNode<DependencyGraphData> GetDependencyGraphNode(ITreeItem item, GraphDataArrow graphDataArrow,
                                                                      string edgeText)
        {
            var data = new DependencyGraphData
                           {
                               SelectedType = item.SelectionType,
                               Title = item.Text,
                               Arrow = graphDataArrow,
                               EdgeText = edgeText
                           };

            return new DTreeNode<DependencyGraphData>(data);
        }

        internal void AddNewType(TypeDefinition tDef)
        {
            Logger.Current.Debug("CecilAssemblyDate :: AddNewType");
            Logger.Current.Debug(">> Adding new type:" + tDef.Name + " at Level:" + currentAssemblyNode.Depth);

            // get the correct namespace node
            foreach (var nsNode in currentAssemblyNode.Nodes)
            {
                if (currentAssemblyNode.Nodes.Count > 0)
                {
                    if (nsNode.Value.Text.Equals(tDef.Namespace))
                    {
                        SelectionType t = SelectionType.TYPE;
                        if (tDef.IsInterface)
                        {
                            t = SelectionType.INTERFACE;
                        }

                        // add base class
                        if (tDef.BaseType != null && !tDef.BaseType.FullName.Equals("System.Object"))
                        {
                            typeDependData.Add(new DependencyTypeData(tDef.FullName, tDef.BaseType.FullName,
                                                                      SelectionType.ABSTRACT));
                        }

                        // add dependencies to the typeDependencies table
                        foreach (TypeReference tRef in tDef.Interfaces)
                        {
                            typeDependData.Add(new DependencyTypeData(tDef.FullName, tRef.FullName,
                                                                      SelectionType.INTERFACE));
                        }

                        currentTypeNode = nsNode.Nodes.Add(new DTreeItem(tDef.Name, t));

                        // create the typedata object
                        //TypeData tData = new TypeData(this.currentAssemblyNode.Value.Text,
                        //    nsNode.Value.Text, tDef.FullName, tDef.Methods.Count);
                        var tData = CecilAssemblyHelper.CreateTypeData(tDef, nsNode.Value.Text);

                        try
                        {
                            if (!typeFieldData.ContainsKey(tData.ToString()))
                            {
                                typeFieldData.Add(tData.ToString(), tData);
                            }
                        }
                        catch // we will get exceptions for types already added
                        {
                        }
                    }
                }
            }
        }

        internal void AddNewMethod(MethodDefinition mDef)
        {
          currentTypeNode.Nodes.Add(new DTreeItem(CecilAssemblyHelper.CreateNormalizeMethodDefinition(mDef).ToString(), SelectionType.METHOD));
        }

        internal void UpdateStats(AssemblyDefinition asmDefinition)
        {
            Logger.Current.Debug("CecilAssemblyData::UpdateStats");

            AssemblyDefinition = asmDefinition;

            AssemblyStats.AssemblyName = asmDefinition.Name.Name;
            if (asmDefinition.EntryPoint == null)
            {
                AssemblyStats.EntryPoint = "No Entry Point";
            }
            else
            {
              var norm = CecilAssemblyHelper.CreateNormalizeMethodDefinition(asmDefinition.EntryPoint);
                AssemblyStats.EntryPoint = string.Format("{0}.{1}", norm.DeclaringTypeFullName, norm);
            }

            AssemblyStats.TotalTypes = asmDefinition.MainModule.Types.Count;
            AssemblyStats.TotalAsmReferences = asmDefinition.MainModule.AssemblyReferences.Count;

            // update stats from the tree
            // total assemblies
            Logger.Current.Debug(">> Total Assemblies:" + AssemblyTree.Nodes[0].Nodes.Count);
            AssemblyStats.TotalAssemblies = AssemblyTree.Nodes[0].Nodes.Count;

            Logger.Current.Debug(">> Total Namespaces in each Assembly");
            foreach (var assemblyNode in AssemblyTree.Nodes[0].Nodes)
            {
                Logger.Current.Debug(">> >> Assembly:" + assemblyNode.Value + " > Namespaces:" +
                                     assemblyNode.Nodes.Count);

                Logger.Current.Debug(">> >> Total types in each Namespace");

                foreach (var namespaceNode in assemblyNode.Nodes)
                {
                    if (namespaceNode.Value.Text.Length > 0)
                    {
                        Logger.Current.Debug(">> >> >> Namespace:" + namespaceNode.Value + " > Types:" +
                                             namespaceNode.Nodes.Count);
                        AssemblyStats.NamespaceList.Add(new NamespaceData(assemblyNode.Value.Text,
                                                                          namespaceNode.Value.Text,
                                                                          namespaceNode.Nodes.Count));


                        DTreeNode<DependencyGraphData> nsRootNode =
                            GetDependencyGraphNode(namespaceNode.Value, GraphDataArrow.NONE, "");

                        // add the assembly dependency
                        nsRootNode.Nodes.Add(
                            GetDependencyGraphNode(assemblyNode.Value, GraphDataArrow.TO, "in"));


                        foreach (var typeNode in namespaceNode.Nodes)
                        {
                            Logger.Current.Debug(">> >> >> Type:" + typeNode.Value + " > Methods:" +
                                                 typeNode.Nodes.Count);
                            //TypeData typeData = new TypeData(assemblyNode.Value.Text, namespaceNode.Value.Text, typeNode.Value.Text, typeNode.Nodes.Count);
                            TypeData typeData = typeFieldData[namespaceNode.Value.Text + "." + typeNode.Value.Text];
                            AssemblyStats.TypeList.Add(typeData);

                            // add type to namespace graph
                            nsRootNode.Nodes.Add(
                                GetDependencyGraphNode(typeNode.Value, GraphDataArrow.FROM, "in"));

                            DTreeNode<DependencyGraphData> typeRootNode =
                                GetDependencyGraphNode(typeNode.Value, GraphDataArrow.NONE, "");

                            typeRootNode.Nodes.Add(
                                GetDependencyGraphNode(namespaceNode.Value, GraphDataArrow.TO, "in"));

                            // add dependencies for abstract/interface types
                            AddDependentNodes(typeRootNode, typeData);

                            // add field/properties
                            AddFieldNodes(typeRootNode, typeData);

                            foreach (var methodNode in typeNode.Nodes)
                            {
                                Logger.Current.Debug(">> >> >> Method:" + methodNode.Value + " > Methods:" +
                                                     methodNode.Nodes.Count);
                                AssemblyStats.MethodList.Add(new MethodData(assemblyNode.Value.Text,
                                                                            namespaceNode.Value.Text,
                                                                            typeNode.Value.Text, methodNode.Value.Text));

                                // add method to type graph
                                typeRootNode.Nodes.Add(
                                    GetDependencyGraphNode(methodNode.Value, GraphDataArrow.FROM, "in"));
                            }

                            // update type graph in HT
                            graphData.Add(assemblyNode.Value + "." + namespaceNode.Value + "." + typeNode.Value,
                                          typeRootNode);
                        }

                        graphData.Add(assemblyNode.Value + "." + namespaceNode.Value, nsRootNode);
                    }
                }
            }
        }

        private static void AddFieldNodes(DTreeNode<DependencyGraphData> rootNode, TypeData tData)
        {
            foreach (String field in tData.Fields)
            {
                rootNode.Nodes.Add(
                    GetDependencyGraphNode(new DTreeItem(field, SelectionType.FIELD), GraphDataArrow.TO, "contains"));
            }

            foreach (String props in tData.Properties)
            {
                rootNode.Nodes.Add(
                    GetDependencyGraphNode(new DTreeItem(props, SelectionType.PROPERTY), GraphDataArrow.TO, "contains"));
            }
        }

        private void AddDependentNodes(DTreeNode<DependencyGraphData> rootNode, TypeData tData)
        {
            foreach (DependencyTypeData data in typeDependData)
            {
                if (data.TypeName.Equals(tData.ToString()))
                {
                    // add data.ImplTypeName to graph as dependency
                    rootNode.Nodes.Add(
                        GetDependencyGraphNode(new DTreeItem(data.ImplTypeName, data.SelectionType), GraphDataArrow.TO,
                                               "implements"));
                }

                if (data.ImplTypeName.Equals(tData.ToString()))
                {
                    // add data.TypeName to graph as dependent
                    rootNode.Nodes.Add(
                        GetDependencyGraphNode(new DTreeItem(data.TypeName, SelectionType.TYPE), GraphDataArrow.FROM,
                                               "implements"));
                }
            }
        }

        internal void AddNewNamespace(string namespaceText)
        {
            currentNamespaceNode = currentAssemblyNode.Nodes.Add(new DTreeItem(namespaceText, SelectionType.NAMESPACE));
        }

        internal DTreeNode<DependencyGraphData> GetAssemblyGraph(TreeSelectionData selectedData)
        {
            string keyname = string.Empty;

            switch (selectedData.SelectionType)
            {
                case SelectionType.ASSEMBLY:
                    keyname = selectedData.AssemblyName;
                    break;
                case SelectionType.NAMESPACE:
                    keyname = selectedData.AssemblyName + "." + selectedData.NameSpace;
                    break;
                case SelectionType.TYPE:
                    keyname = selectedData.AssemblyName + "." + selectedData.NameSpace + "." + selectedData.TypeName;
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(keyname))
            {
                try
                {
                    return graphData[keyname];
                }
                catch (Exception ex)
                {
                    Logger.Current.Debug(">> Exception in getting keyname for graph Data:" + ex);
                }
            }


            return null;
        }

        internal static void AddNewField(FieldDefinition fDef)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}