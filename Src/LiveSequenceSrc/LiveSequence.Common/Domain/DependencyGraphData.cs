namespace LiveSequence.Common.Domain
{
    public enum GraphDataArrow
    {
        TO,
        FROM,
        NONE
    }

    public class DependencyGraphData
    {
        private GraphDataArrow _graphArrow = GraphDataArrow.NONE;
        
        public GraphDataArrow Arrow
        {
            get { return _graphArrow; }
            set { _graphArrow = value; }
        }

        private SelectionType _selectedType;

        public SelectionType SelectedType
        {
            get { return _selectedType; }
            set { _selectedType = value; }
        }

        private string _edgeText;

        public string EdgeText
        {
            get { return _edgeText; }
            set { _edgeText = value; }
        }

        private string _title;

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public override string ToString()
        {
            return this.Title;
        }
    }
}