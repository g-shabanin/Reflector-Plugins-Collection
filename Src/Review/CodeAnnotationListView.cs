namespace Reflector.Review
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;
	using Reflector.Review.Data;
	using Reflector.CodeModel;
    using System.Collections;

    internal class CodeAnnotationListView : ListView
    {
		public event EventHandler ActiveAnnotationChanged;

		private IAssemblyManager assemblyManager;
        private ListViewColumnSorter sorter = new ListViewColumnSorter();

		public CodeAnnotationListView(IAssemblyManager assemblyManager)
		{
			this.assemblyManager = assemblyManager;

			ColumnHeader[] columnHeaders = new ColumnHeader[4];
			columnHeaders[0] = new ColumnHeader();
			columnHeaders[0].Text = "Name";
			columnHeaders[0].Width = 500;

            columnHeaders[1] = new ColumnHeader();
            columnHeaders[1].Text = "Status";
            columnHeaders[1].Width = 80;

            columnHeaders[2] = new ColumnHeader();
            columnHeaders[2].Text = "Changed Date";
            columnHeaders[2].Width = 125;

            columnHeaders[3] = new ColumnHeader();
            columnHeaders[3].Text = "Changed By";
            columnHeaders[3].Width = 80;

			this.Columns.AddRange(columnHeaders);
			this.Dock = DockStyle.Fill;
			this.View = View.Details;
			this.MultiSelect = true;
			this.FullRowSelect = true;
			this.HideSelection = false;
            this.AllowColumnReorder = true;
			this.SmallImageList = BrowserResource.ImageList;

            this.ListViewItemSorter = this.sorter;
			// ListView in WinForms in .NET 1.0 has a bug that window handle needs to be created before any list items can be focused.
			IntPtr handle = this.Handle;
		}

		public void ClearAnnotations()
		{
			this.SelectedItems.Clear();
			this.Items.Clear();
		}

		public void AddAnnotation(CodeAnnotation annotation)
		{
            if (annotation == null)
                throw new ArgumentNullException("annotation");

			CodeAnnotationListItem item = new CodeAnnotationListItem();
			this.Items.Add(item);
			item.Annotation = annotation;
		}

		public void RemoveAnnotation(CodeAnnotation annotation)
		{
            if (annotation == null)
                throw new ArgumentNullException("annotation");

			for (int i = this.Items.Count - 1; i >= 0; i--)
			{
				CodeAnnotationListItem item = (CodeAnnotationListItem) this.Items[i];
				if (item.Annotation == annotation)
				{
					this.Items.RemoveAt(i);
				}
			}
		}

		public CodeAnnotation ActiveAnnotation
		{
			set
			{
				this.SelectedItems.Clear();

				for (int i = 0; i < this.Items.Count; i++)
				{
					CodeAnnotationListItem item = (CodeAnnotationListItem)this.Items[i];
					if (value == item.Annotation)
					{
						item.Selected = true;
						item.Focused = true;

						item.EnsureVisible();
					}
				}
			}

			get
			{
				if (this.SelectedItems.Count == 1)
				{
					CodeAnnotationListItem item = (CodeAnnotationListItem) this.SelectedItems[0];
					return item.Annotation;
				}

				return null;
			}
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
			this.OnActiveAnnotationChanged(EventArgs.Empty);
		}

		protected virtual void OnActiveAnnotationChanged(EventArgs e)
		{
            EventHandler eh = this.ActiveAnnotationChanged;
            if (eh!=null)
				eh(this, e);
		}

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            base.OnColumnClick(e);
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.sorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (this.sorter.Order == SortOrder.Ascending)
                    this.sorter.Order = SortOrder.Descending;
                else
                    this.sorter.Order = SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                this.sorter.SortColumn = e.Column;
                this.sorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.Sort();
        }

		private IAssemblyManager AssemblyManager
		{
			get
			{
				return this.assemblyManager;
			}
		}

		private class CodeAnnotationListItem : ListViewItem
		{
            public CodeAnnotation Annotation
            {
                get { return (CodeAnnotation)this.Tag; }
				set
				{

                    this.Tag = value;

                    this.SubItems.Clear();
                    this.SubItems.Add(this.Annotation.Status.ToString());
                    this.SubItems.Add(this.Annotation.ChangedDate.ToString("u"));
                    this.SubItems.Add(this.Annotation.ChangedBy);

                    object target = this.Resolve(null);
                    ITypeReference typeReference = target as ITypeReference;
					if (typeReference != null)
					{
                        this.Text = Helper.GetNameWithResolutionScope(typeReference);
						this.ImageIndex = IconHelper.GetImageIndex(typeReference);
						this.ForeColor = Color.FromArgb(IconHelper.GetColorDeclaringType(typeReference));
						return;
					}

					IMethodReference methodReference = target as IMethodReference;
					if (methodReference != null)
					{
						this.Text = Helper.GetNameWithDeclaringType(methodReference);
						this.ImageIndex = IconHelper.GetImageIndex(methodReference);
						this.ForeColor = Color.FromArgb(IconHelper.GetColorDeclaringType(methodReference));
						return;
					}

					IFieldReference fieldReference = target as IFieldReference;
					if (fieldReference != null)
					{
						this.Text = Helper.GetNameWithDeclaringType(fieldReference);
						this.ImageIndex = IconHelper.GetImageIndex(fieldReference);
						this.ForeColor = Color.FromArgb(IconHelper.GetColorDeclaringType(fieldReference));
						return;
					}

					IEventReference eventReference = target as IEventReference;
					if (eventReference != null)
					{
						this.Text = Helper.GetNameWithDeclaringType(eventReference);
						this.ImageIndex = IconHelper.GetImageIndex(eventReference);
						this.ForeColor = Color.FromArgb(IconHelper.GetColorDeclaringType(eventReference));
						return;
					}

					IPropertyReference propertyReference = target as IPropertyReference;
					if (propertyReference != null)
					{
						this.Text = Helper.GetNameWithDeclaringType(propertyReference);
						this.ImageIndex = IconHelper.GetImageIndex(propertyReference);
						this.ForeColor = Color.FromArgb(IconHelper.GetColorDeclaringType(propertyReference));
						return;
					}

					IAssembly assembly = target as IAssembly;
					if (assembly != null)
					{
						this.Text = assembly.Name;
						this.ImageIndex = BrowserResource.Assembly;
						return;
					}

					IModule module = target as IModule;
					if (module != null)
					{
						this.Text = module.Name;
						this.ImageIndex = BrowserResource.Module;
						return;
					}

					IResource resource = target as IResource;
					if (resource != null)
					{
						this.Text = resource.Name;
						this.ImageIndex = IconHelper.GetImageIndex(resource);
						return;
					}

					this.Text = value.Identifier.Identifier;
					this.ImageIndex = BrowserResource.Error;
				}
			}

			public object Resolve(IAssemblyCache assemblyCache)
			{
				CodeAnnotationListView listView = (CodeAnnotationListView) this.ListView;
				IAssemblyManager assemblyManager = listView.AssemblyManager;

                CodeAnnotation annotation = (CodeAnnotation)this.Tag;
                string identifier = annotation.Identifier.Identifier;
				CodeIdentifier codeIdentifier = new CodeIdentifier(identifier);
				return codeIdentifier.Resolve(assemblyManager, assemblyCache);
			}

			public override string ToString()
			{
				return this.Text;
			}
		}

        /// <summary>
        /// This class is an implementation of the 'IComparer' interface.
        /// </summary>
        private class ListViewColumnSorter : IComparer
        {
            /// <summary>
            /// Specifies the column to be sorted
            /// </summary>
            private int ColumnToSort;
            /// <summary>
            /// Specifies the order in which to sort (i.e. 'Ascending').
            /// </summary>
            private SortOrder OrderOfSort;
            /// <summary>
            /// Case insensitive comparer object
            /// </summary>
            private CaseInsensitiveComparer ObjectCompare;

            /// <summary>
            /// Class constructor.  Initializes various elements
            /// </summary>
            public ListViewColumnSorter()
            {
                // Initialize the column to '0'
                ColumnToSort = 0;

                // Initialize the sort order to 'none'
                OrderOfSort = SortOrder.None;

                // Initialize the CaseInsensitiveComparer object
                ObjectCompare = new CaseInsensitiveComparer();
            }

            /// <summary>
            /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
            /// </summary>
            /// <param name="x">First object to be compared</param>
            /// <param name="y">Second object to be compared</param>
            /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
            public int Compare(object x, object y)
            {
                int compareResult;
                ListViewItem listviewX, listviewY;

                // Cast the objects to be compared to ListViewItem objects
                listviewX = (ListViewItem)x;
                listviewY = (ListViewItem)y;

                // Compare the two items
                compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

                // Calculate correct return value based on object comparison
                if (OrderOfSort == SortOrder.Ascending)
                {
                    // Ascending sort is selected, return normal result of compare operation
                    return compareResult;
                }
                else if (OrderOfSort == SortOrder.Descending)
                {
                    // Descending sort is selected, return negative result of compare operation
                    return (-compareResult);
                }
                else
                {
                    // Return '0' to indicate they are equal
                    return 0;
                }
            }

            /// <summary>
            /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
            /// </summary>
            public int SortColumn
            {
                set
                {
                    ColumnToSort = value;
                }
                get
                {
                    return ColumnToSort;
                }
            }

            /// <summary>
            /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
            /// </summary>
            public SortOrder Order
            {
                set
                {
                    OrderOfSort = value;
                }
                get
                {
                    return OrderOfSort;
                }
            }

        }
	}
}
