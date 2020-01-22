using LiveSequence.Common.Domain;

namespace LiveSequence.Tree
{
    interface ITreeItem
    {
        SelectionType SelectionType { get; set;} 
        string Text { get; set; }
    }
}
