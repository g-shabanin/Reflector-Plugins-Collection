using System;
using System.Collections;

namespace Reflector.Review.Data
{
    [Serializable]
    internal sealed class CodeChange : IComparable
    {
        private string comment;
        public string Comment
        {
            get { return this.comment; }
            set { this.comment = value; }
        }

        private string changedBy;
        public string ChangedBy
        {
            get { return this.changedBy; }
            set { this.changedBy = value; }
        }

        private DateTime changedDate;
        public DateTime ChangedDate
        {
            get { return this.changedDate; }
            set { this.changedDate = value; }
        }

        private CodeAnnotationStatus status;
        public CodeAnnotationStatus Status
        {
            get { return this.status; }
            set { this.status = value; }
        }

        private CodeAnnotationResolution resolution;
        public CodeAnnotationResolution Resolution
        {
            get { return this.resolution; }
            set { this.resolution = value; }
        }

        private readonly ArrayList changedFields = new ArrayList(1);
        public IList ChangedFields
        {
            get { return this.changedFields; }
        }

        public void UpdateModifiedFields(CodeChange last)
        {
            if (last == null)
                throw new ArgumentNullException("last");

            if (this.Status != last.Status)
                this.ChangedFields.Add(new CodeChangedField("Status", this.Status.ToString()));
            if (this.Resolution != last.Resolution)
                this.ChangedFields.Add(new CodeChangedField("Resolution", this.Resolution.ToString()));
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            CodeChange change = obj as CodeChange;
            if (change == null) return -1;
            return this.ChangedDate.CompareTo(change.ChangedDate);
        }

        #endregion
    }
}
