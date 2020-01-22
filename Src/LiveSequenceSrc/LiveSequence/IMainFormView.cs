using System;
using System.ComponentModel;
using LiveSequence.Tree;

namespace LiveSequence
{
    interface IMainFormView
    {
        DTreeNode<DTreeItem> AssemblyTree { get; set; }

        String AssemblyFileName { get; }

        void WorkerProgressChanged(ProgressChangedEventArgs e);
        void WorkerCompleted();
    }
}
