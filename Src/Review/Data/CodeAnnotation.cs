using System;
using System.Collections;

namespace Reflector.Review.Data
{
    [Serializable]
    internal enum CodeAnnotationStatus
    {
        Active,
        Resolved,
        Closed
    }

    [Serializable]
    internal enum CodeAnnotationResolution
    {
        Investigated,
        Fixed,
        WontFix,
        ByDesign,
        NoRepro
    }

    [Serializable]
    internal sealed class CodeAnnotation
    {
        private CodeIdentifier identifier;
        public CodeIdentifier Identifier
        {
            get { return this.identifier; }
            set 
            { 
                this.identifier = value; 
            }
        }

        public DateTime CreatedDate
        {
            get
            {
                if (!this.HasChanges)
                    throw new InvalidOperationException("no changes");
                return ((CodeChange)this.Changes[0]).ChangedDate;
            }
        }

        public string CreatedBy
        {
            get 
            {
                if (!this.HasChanges)
                    throw new InvalidOperationException("no changes");
                return ((CodeChange)this.Changes[0]).ChangedBy;
            }
        }

        public DateTime ChangedDate
        {
            get
            {
                if (!this.HasChanges)
                    throw new InvalidOperationException("no changes");
                return ((CodeChange)this.Changes[this.Changes.Count - 1]).ChangedDate;
            }
        }

        public string ChangedBy
        {
            get
            {
                if (!this.HasChanges)
                    throw new InvalidOperationException("no changes");
                return ((CodeChange)this.Changes[this.Changes.Count - 1]).ChangedBy;
            }
        }

        public CodeAnnotationStatus Status
        {
            get
            {
                if (!this.HasChanges)
                    throw new InvalidOperationException("no changes");
                return ((CodeChange)this.Changes[this.Changes.Count - 1]).Status;
            }
        }

        public CodeAnnotationResolution Resolution
        {
            get
            {
                if (!this.HasChanges)
                    throw new InvalidOperationException("no changes");
                return ((CodeChange)this.Changes[this.Changes.Count - 1]).Resolution;
            }
        }

        private readonly ArrayList changes = new ArrayList(1);
        public IList Changes
        {
            get { return this.changes; }
        }

        public bool HasChanges
        {
            get { return this.changes.Count > 0; }
        }

        public void Update()
        {
            this.changes.Sort();
        }

        public void CommitChange(CodeChange change)
        {
            if (change == null)
                throw new ArgumentNullException("change");

            if (this.Changes.Count > 0)
            {
                // update changed fields
                CodeChange last = (CodeChange)this.Changes[this.Changes.Count - 1];
                change.UpdateModifiedFields(last);
            }

            this.Changes.Add(change);
        }
    }
}
