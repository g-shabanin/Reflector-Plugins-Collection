using LiveSequence.Common.Domain;

namespace LiveSequence.Tree
{
    class DTreeItem : ITreeItem
    {
        public string Text { get; set; }

        public SelectionType SelectionType { get; set; }

        public override string ToString()
        {
            return this.Text;
        }

        public DTreeItem(string text, SelectionType sType)
        {
            this.Text = text;
            this.SelectionType = sType;
        }

    }
}
