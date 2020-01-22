using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LiveSequence.Tree
{

    #region Comparer

    internal class NodeCompare<T> : System.Collections.IComparer
    {
        readonly IComparer<T> m_valueCompare;

        public NodeCompare(IComparer<T> valueCompare)
        {
            m_valueCompare = valueCompare;
        }

        public int Compare(object x, object y)
        {
            DTreeNode<T> xData = (DTreeNode<T>)x;
            DTreeNode<T> yData = (DTreeNode<T>)y;

            return m_valueCompare.Compare(xData.Value, yData.Value);
        }
    }

    #endregion // Comparer

    public interface ITreeNodeMapper
    {
        void UpdateNode(object dataNode, TreeNode treeNode);
        object GetNodeInfo(TreeNode treeNode);
    }

    public class TreeNodeDefaultMapper<T> : ITreeNodeMapper
    {
        public void UpdateNode(object dataNode, TreeNode treeNode)
        {
            DTreeNode<T> dn = (DTreeNode<T>)dataNode;
            treeNode.Tag = dn;
            treeNode.Text = dn.Value.ToString();
        }

        public object GetNodeInfo(TreeNode treeNode)
        {
            return treeNode.Tag;
        }
    }

    public class TreeViewController<T>
    {
        #region Data
        private TreeView mView;
        private DTreeNode<T> mData;
        private ITreeNodeMapper mNodeMapper;

        IComparer<T> mValueCompare;      // supplied by caller: compares node values
        System.Collections.IComparer mNodeCompare;       // helper object: compares Node<T>'s
        #endregion // Data

        #region CTORs

        void Construct(TreeView view, DTreeNode<T> data, ITreeNodeMapper nodeMapper)
        {
            mView = view;
            mData = data;
            mNodeMapper = nodeMapper ?? new TreeNodeDefaultMapper<T>();

            UpdateAllNodes();

            mData.Root.OnNodeChanged += m_dataTree_OnNodeChanged;
            mData.Root.OnValueChanged += m_dataTree_OnValueChanged;
        }

        void m_dataTree_OnValueChanged(object sender, EventArgs e)
        {
            UpdateNode(((DTreeEventArgs<T>)e).Node, false);
        }

        void m_dataTree_OnNodeChanged(object sender, EventArgs e)
        {
            DTreeEventArgs<T> args = e as DTreeEventArgs<T>;

            if (args != null)
            {
                bool recursive = args.Change == ENodeEvent.ChildAdded ||
                                 args.Change == ENodeEvent.ChildOrderChanged ||
                                 args.Change == ENodeEvent.ChildRemoved ||
                                 args.Change == ENodeEvent.ChildsCleared;

                UpdateNode(args.Node, recursive);
            }
        }

        public TreeViewController(TreeView view, DTreeNode<T> data)
        {
            Construct(view, data, null);
        }

        public TreeViewController(TreeView view, DTreeNode<T> data, ITreeNodeMapper nodeMapper)
        {
            Construct(view, data, nodeMapper);
        }

        #endregion // CTORs

        #region Auto-Comparer
        public IComparer<T> AutoSortCompare
        {
            get { return mValueCompare; }
            set
            {
                if (mValueCompare == value) // avoid updating all items
                    return;

                mValueCompare = value;
                mNodeCompare = (mValueCompare != null) ?
                      new NodeCompare<T>(mValueCompare) : null;

                UpdateAllNodes();
            }
        }
        #endregion

        #region Data Node <--> View Node translation
        public DTreeNode<T> GetDataNode(TreeNode viewNode)
        {
            if (viewNode == null)
                return null;
            return mNodeMapper.GetNodeInfo(viewNode) as DTreeNode<T>;
        }

        public TreeNode GetViewNode(DTreeNode<T> dataNode)
        {
            return FindTreeNode(dataNode, mView.Nodes);
        }

        protected TreeNode FindTreeNode(DTreeNode<T> dataNode, TreeNodeCollection where)
        {
            foreach (TreeNode viewNode in where)
            {
                if (viewNode.Tag == dataNode)
                    return viewNode;

                TreeNode found = FindTreeNode(dataNode, viewNode.Nodes);
                if (found != null)
                    return found;
            }
            return null;
        }
        #endregion

        #region SelectedNode
        public DTreeNode<T> SelectedNode
        {
            get { return GetDataNode(mView.SelectedNode); }
            set { mView.SelectedNode = GetViewNode(value); }
        }
        #endregion

        #region Trigger Manual View updates
        public void UpdateAllNodes()
        {
            UpdateNodeList(mData.Nodes, mView.Nodes);
        }

        public void UpdateNode(DTreeNode<T> dataNode, bool recursive)
        {
            UpdateNode(dataNode, GetViewNode(dataNode), recursive);
        }

        public void UpdateNode(TreeNode viewNode, bool recursive)
        {
            UpdateNode(GetDataNode(viewNode), viewNode, recursive);
        }

        internal void UpdateNode(DTreeNode<T> dataNode, TreeNode treeNode, bool recursive)
        {
            if (dataNode == mData) // special handling for "root node changed"
            {
                if (recursive)
                    UpdateAllNodes();
            }
            else
            {
                mNodeMapper.UpdateNode(dataNode, treeNode);

                if (recursive)
                {
                    if (dataNode.HasChildren)
                        UpdateNodeList(dataNode.Nodes, treeNode.Nodes);
                    else
                        treeNode.Nodes.Clear();
                }
            }
        }

        internal void UpdateNodeList(DTreeNodeCollection<T> dataNodes, TreeNodeCollection viewNodes)
        {
            System.Collections.IList list;

            // Sort if required
            if (mNodeCompare != null)
            {
                System.Collections.ArrayList sortedNodes = new System.Collections.ArrayList(dataNodes);
                sortedNodes.Sort(mNodeCompare);
                list = sortedNodes;
            }
            else
                list = dataNodes; // original sort order

            // update existing nodes
            int existing = Math.Min(list.Count, viewNodes.Count);

            for (int i = 0; i < existing; ++i)
                UpdateNode(list[i] as DTreeNode<T>, viewNodes[i], true);

            // add items if necessary
            if (list.Count > viewNodes.Count)
            {
                for (int i = viewNodes.Count; i < list.Count; ++i)
                {
                    TreeNode node = new TreeNode();
                    viewNodes.Add(node);
                    UpdateNode(list[i] as DTreeNode<T>, node, true);
                }
            }
            // ..or else remove items if necessary
            else if (list.Count < viewNodes.Count)
            {
                for (int i = 0; i < viewNodes.Count - list.Count; ++i)
                    viewNodes.RemoveAt(viewNodes.Count - 1);
            }
        }
        #endregion


    }
}
