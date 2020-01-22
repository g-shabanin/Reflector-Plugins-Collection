using System;
using System.Windows.Forms;

namespace LiveSequence.Tree
{
    class ImageTreeNodeMapper<T> : ITreeNodeMapper where T : ITreeItem
    {
        public void UpdateNode(object dataNode, TreeNode treeNode)
        {
            DTreeNode<T> dn = (DTreeNode<T>)dataNode;
            treeNode.Tag = dn;
            treeNode.Text = dn.Value.ToString();
            treeNode.ImageIndex = Convert.ToInt32(dn.Value.SelectionType); //treeNode.Level;
            treeNode.SelectedImageIndex = Convert.ToInt32(dn.Value.SelectionType);
        }

        public object GetNodeInfo(TreeNode treeNode)
        {
            return treeNode.Tag;
        }
    }
}
