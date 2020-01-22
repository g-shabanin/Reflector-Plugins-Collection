using LiveSequence.Common.Domain;
using LiveSequence.Tree;

namespace LiveSequence.Engine
{
    class TreeSelectionData
    {
        public SelectionType SelectionType { get; set; }

        public string AssemblyName { get; set; }

        public string NameSpace { get; set; }

        public string TypeName { get; set; }

        public string MethodName { get; set; }

        public TreeSelectionData(TreeViewController<DTreeItem> tvController, SelectionType selectionType)
        {
            MethodName = string.Empty;
            TypeName = string.Empty;
            NameSpace = string.Empty;
            AssemblyName = string.Empty;
            if (tvController.SelectedNode != null)
            {
                this.SelectionType = selectionType;

                switch (selectionType)
                {
                    case SelectionType.ASSEMBLY:
                        this.AssemblyName = tvController.SelectedNode.Value.Text;
                        break;
                    case SelectionType.NAMESPACE:
                        this.NameSpace = tvController.SelectedNode.Value.Text;
                        this.AssemblyName = tvController.SelectedNode.Parent.Value.Text;
                        break;
                    case SelectionType.TYPE:
                        this.TypeName = tvController.SelectedNode.Value.Text;
                        this.NameSpace = tvController.SelectedNode.Parent.Value.Text;
                        this.AssemblyName = tvController.SelectedNode.Parent.Parent.Value.Text;
                        break;
                    case SelectionType.METHOD:
                        this.MethodName = tvController.SelectedNode.Value.Text;
                        this.TypeName = tvController.SelectedNode.Parent.Value.Text;
                        this.NameSpace = tvController.SelectedNode.Parent.Parent.Value.Text;
                        this.AssemblyName = tvController.SelectedNode.Parent.Parent.Parent.Value.Text;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
