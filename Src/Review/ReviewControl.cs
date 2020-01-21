namespace Reflector.Review
{
	using System;
	using System.Drawing;
	using System.Diagnostics;
	using System.IO;
	using System.Windows.Forms;
	using System.Xml;
	using Reflector.CodeModel;
	using Reflector.Review.Data;

	internal sealed class ReviewControl : Control
    {
		private CommandBarManager commandBarManager;
		private CommandBar toolBar;
		private HistoryRichTextBox historyTextBox;
		private Splitter splitter;
		private CodeAnnotationListView codeAnnotationListView;

        private CommandBarButton newReviewButton;
        private CommandBarButton loadReviewButton;
        private CommandBarButton saveReviewButton;
        private CommandBarButton mergeReviewsButton;

        private CommandBarButton previousAnnotationButton;
        private CommandBarButton nextAnnotationButton;

        private CommandBarButton editAnnotationButton;
        private CommandBarButton saveAnnotationButton;
        private CommandBarButton deleteAnnotationButton;

        private CommandBarComboBox statusComboBox;
        private CommandBarComboBox resolutionComboBox;

        private CodeReview review;
        private bool isCurrentAnnotation = false;
        private int currentAnnotationIndex = 0;

        private CodeAnnotation editedAnnotation = null;
        private CodeChange currentChange = null;

		private IAssemblyBrowser assemblyBrowser;
		private IWindowManager windowManager;
		private IAssemblyManager assemblyManager;
		private IAssemblyCache assemblyCache;
		private IConfigurationManager configurationManager;

        private Color readColor = Color.LightYellow;
        private Color editColor = Color.White;

		private const string dialogFilter = "Review files (*.review)|*.review|XML files (*.xml)|*.xml|All files (*.*)|*.*";
		private string currentFileName = string.Empty;
		
		public ReviewControl(IServiceProvider serviceProvider)
        {
			this.TabStop = false;

			this.assemblyBrowser = (IAssemblyBrowser) serviceProvider.GetService(typeof(IAssemblyBrowser));
			this.windowManager = (IWindowManager) serviceProvider.GetService(typeof(IWindowManager));
			this.assemblyManager = (IAssemblyManager) serviceProvider.GetService(typeof(IAssemblyManager));
			this.assemblyCache = (IAssemblyCache) serviceProvider.GetService(typeof(IAssemblyCache));
			this.configurationManager = (IConfigurationManager)serviceProvider.GetService(typeof(IConfigurationManager));

			this.LoadConfiguration();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			// Send keys to local CommandBarManager before they bubble to the main window.
			if ((this.commandBarManager != null) && (this.commandBarManager.PreProcessMessage(ref msg)))
			{
				return true;
			}

			if (base.ProcessCmdKey(ref msg, keyData))
			{
				return true;
			}

 			return false;
		}

		public void Activate()
		{
			this.historyTextBox.Focus();
		}

        public bool Editing
        {
            get 
            { 
                return !this.historyTextBox.ReadOnly; 
            }
            set 
            { 
                this.historyTextBox.ReadOnly = !value;
				if (this.historyTextBox.ReadOnly)
				{
					this.historyTextBox.BackColor = this.readColor;
				}
				else
				{
					this.historyTextBox.BackColor = this.editColor;
				}
            }
        }

        public bool IsCurrentAnnotation
        {
            get { return this.isCurrentAnnotation; }
        }

        public int CurrentAnnotationIndex
        {
            get 
			{ 
				return this.currentAnnotationIndex; 
			}
            
			set
            {
				if (this.currentAnnotationIndex != value)
				{
					this.OnBeforeAnnotationChange(EventArgs.Empty);
				}

                this.currentAnnotationIndex = value;

                if (this.review.Annotations.Count > 0)
                {
					if (this.currentAnnotationIndex + 1 > this.review.Annotations.Count)
					{
						this.currentAnnotationIndex = 0;
					}

					if (this.currentAnnotationIndex < 0)
					{
						this.currentAnnotationIndex = this.review.Annotations.Count - 1;
					}
                }

				CodeAnnotation currentAnnotation = this.CurrentAnnotation;
				if (currentAnnotation != null)
				{
					this.codeAnnotationListView.ActiveAnnotation = currentAnnotation;
				}
            }
        }

		private void UpdateIdentifierList()
		{
			this.codeAnnotationListView.ClearAnnotations();

			foreach (CodeAnnotation annotation in this.review.Annotations)
			{
				this.codeAnnotationListView.AddAnnotation(annotation);
			}
		}

        public CodeAnnotation CurrentAnnotation
        {
            get 
            {
                if (this.IsCurrentAnnotation)
				{
					return (CodeAnnotation) this.review.Annotations[this.CurrentAnnotationIndex];
				}

				return null;
            }
        }

        private CodeChange CurrentChange
        {
            get { return this.currentChange; }
        }

        public event EventHandler BeforeAnnotationChange;
        private void OnBeforeAnnotationChange(EventArgs e)
        {
            EventHandler eh = this.BeforeAnnotationChange;
            if (eh != null)
                eh(this, e);
        }

        private void ReviewControl_BeforeAnnotationChange(object sender, EventArgs e)
        {
            // propose to save work before moving to next item
            if (this.Editing)
            {
                if (MessageBox.Show("Do you want to save the annotation changes?", "Review", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    this.SaveCurrentAnnotation();
                }

                this.currentChange = null;
                this.Editing = false;
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

			if (this.Controls.Count == 0)
			{
				if (this.CurrentFileName.Length > 0)
				{
					this.review = this.LoadReview(this.CurrentFileName);
				}

				if (this.review == null)
				{
					this.CurrentFileName = string.Empty;
					this.review = new CodeReview();
				}

				this.commandBarManager = new CommandBarManager();
				this.toolBar = new CommandBar(this.commandBarManager, CommandBarStyle.ToolBar);

				this.newReviewButton = this.toolBar.Items.AddButton(CommandBarImages.New, "New Review", new EventHandler(this.NewReviewButton_Click));
				this.loadReviewButton = this.toolBar.Items.AddButton(CommandBarImages.Open, "Load Review", new EventHandler(this.LoadReviewButton_Click));
				this.saveReviewButton = this.toolBar.Items.AddButton(CommandBarImages.Save, "Save Review As...", new EventHandler(this.SaveReviewButton_Click));
				this.mergeReviewsButton = this.toolBar.Items.AddButton(CommandBarImages.Parent, "Merge Reviews", new EventHandler(this.MergeReviewsButton_Click));

				this.toolBar.Items.AddSeparator();

				this.previousAnnotationButton = this.toolBar.Items.AddButton(CommandBarImages.Back, "Previous", new EventHandler(this.PreviousAnnotationButtonClick), Keys.Control | Keys.Up);
				this.nextAnnotationButton = this.toolBar.Items.AddButton(CommandBarImages.Forward, "Next", new EventHandler(this.NextAnnotationButtonClick), Keys.Control | Keys.Down);

				this.toolBar.Items.AddSeparator();

				this.editAnnotationButton = this.toolBar.Items.AddButton(CommandBarImages.Edit, "Edit Annotation", new EventHandler(this.EditAnnotationButtonClick), Keys.Control | Keys.W);
				this.saveAnnotationButton = this.toolBar.Items.AddButton(CommandBarImages.Copy, "Save/Commit Annotation", new EventHandler(this.SaveAnnotationButtonClick), Keys.Control | Keys.S);
				this.deleteAnnotationButton = this.toolBar.Items.AddButton(CommandBarImages.Delete, "Cancel/Delete Annotation", new EventHandler(this.DeleteAnnotationButton_Click));

				this.toolBar.Items.AddSeparator();

				this.statusComboBox = this.toolBar.Items.AddComboBox("Status", new ComboBox());
				this.statusComboBox.ComboBox.Items.AddRange(Enum.GetNames(typeof(CodeAnnotationStatus)));
				this.statusComboBox.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
				this.resolutionComboBox = this.toolBar.Items.AddComboBox("Resolution", new ComboBox());
				this.resolutionComboBox.ComboBox.Items.AddRange(Enum.GetNames(typeof(CodeAnnotationResolution)));
				this.resolutionComboBox.ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

				this.historyTextBox = new HistoryRichTextBox();
				this.historyTextBox.Dock = DockStyle.Fill;
				this.historyTextBox.AutoSize = true;
				this.historyTextBox.ReadOnly = true;
				this.historyTextBox.DetectUrls = true;
				this.historyTextBox.BackColor = this.readColor;
				this.Controls.Add(this.historyTextBox);

				this.splitter = new Splitter();
				this.splitter.Dock = DockStyle.Bottom;
				this.Controls.Add(this.splitter);

				this.codeAnnotationListView = new CodeAnnotationListView(this.assemblyManager);
				this.codeAnnotationListView.Dock = DockStyle.Bottom;
				this.codeAnnotationListView.Height = 150;
				this.Controls.Add(this.codeAnnotationListView);

				this.commandBarManager.CommandBars.Add(this.toolBar);
				this.Controls.Add(this.commandBarManager);
				this.Dock = DockStyle.Fill;
				this.BeforeAnnotationChange += new EventHandler(ReviewControl_BeforeAnnotationChange);

				this.UpdateButtonsState();
			}

			if (this.Parent != null)
			{
				this.assemblyBrowser.ActiveItemChanged += new EventHandler(this.AssemblyBrowser_ActiveItemChanged);
				this.codeAnnotationListView.ActiveAnnotationChanged += new EventHandler(this.CodeAnnotationListView_ActiveAnnotationChanged);

				this.UpdateIdentifierList();
				this.UpdateAnnotation();
			}
			else
			{
				this.assemblyBrowser.ActiveItemChanged -= new EventHandler(this.AssemblyBrowser_ActiveItemChanged);
				this.codeAnnotationListView.ActiveAnnotationChanged -= new EventHandler(this.CodeAnnotationListView_ActiveAnnotationChanged);

				if (this.Editing)
				{
					this.SaveCurrentAnnotation();
				}
			}
        }

        #region Next/Previous
        public void NextAnnotationButtonClick(object sender, EventArgs e)
        {
			if (this.newReviewButton.Enabled)
			{
				this.EnsureVisible();

				if (!this.isCurrentAnnotation)
				{
					this.isCurrentAnnotation = true;
				}
				else
				{
					this.CurrentAnnotationIndex++;
				}

				this.MoveToCurrentAnnotation();
				this.historyTextBox.Focus();

				this.RenderAnnotation();
				this.UpdateButtonsState();
			}
        }

        public void PreviousAnnotationButtonClick(object sender, EventArgs e)
        {
			if (this.previousAnnotationButton.Enabled)
			{
				this.EnsureVisible();

				if (!this.isCurrentAnnotation)
				{
					this.isCurrentAnnotation = true;
				}
				else
				{
					this.CurrentAnnotationIndex--;
				}

				this.MoveToCurrentAnnotation();
				this.historyTextBox.Focus();

				this.RenderAnnotation();
				this.UpdateButtonsState();
			}
        }

        private void MoveToCurrentAnnotation()
        {
            object item = this.CurrentAnnotation.Identifier.Resolve(this.assemblyManager, this.assemblyCache);
			if (item != null)
			{
				this.assemblyBrowser.ActiveItem = item;
			}
        }
        #endregion

        #region Load/Save/Clear review
        private void LoadReviewButton_Click(object sender, EventArgs e)
        {
			OpenFileDialog dialog = this.CreateOpenDialog();
			dialog.Multiselect = false;
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				CodeReview loadedReview = this.LoadReview(dialog.FileName);
				if (loadedReview != null)
				{
					this.CurrentFileName = dialog.FileName;
					this.review = loadedReview;

					this.windowManager.StatusBar.Text = string.Format("Loaded '{0}'.", this.CurrentFileName);
					this.UpdateAnnotation();
					this.UpdateButtonsState();
				}
			}
        }

		private void MergeReviewsButton_Click(object sender, EventArgs e)
        {
			CodeReview[] loadedReviews = this.LoadReviews();
			if (loadedReviews != null)
			{
				this.review.Merge(loadedReviews);

				this.UpdateIdentifierList();
				this.windowManager.StatusBar.Text = "Loaded Reviews.";
				this.UpdateAnnotation();
				this.UpdateButtonsState();
			}
        }

		private void SaveReviewButton_Click(object sender, EventArgs e)
        {
			this.SaveReview();
        }

		private void NewReviewButton_Click(object sender, EventArgs e)
        {
			if (this.review.Annotations.Count > 0)
			{
				if (MessageBox.Show("Do you want to save the changes?", "Review", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
				{
					if (!this.SaveReview())
					{
						return;
					}
				}
			}

			this.CurrentFileName = string.Empty;
			this.review = new CodeReview();

			this.codeAnnotationListView.Items.Clear();
			this.isCurrentAnnotation = false;
			this.CurrentAnnotationIndex = 0;
			this.windowManager.StatusBar.Text = string.Empty;
			this.RenderAnnotation();
			this.UpdateButtonsState();
		}

		private bool SaveReview()
		{
			if (this.SaveAsReview(this.review))
			{
				this.windowManager.StatusBar.Text = String.Format("Saved '{0}'.", this.CurrentFileName);
				this.RenderAnnotation();
				this.UpdateButtonsState();
				return true;
			}

			return false;
		}

        #endregion

        private void EnsureVisible()
        {
            if (this.Parent != null) return;

            foreach (IWindow window in this.windowManager.Windows)
            {
                if (window.Content == this)
                {
                    window.Visible = true;
                    break;
                }
            }
        }

        #region Edit/Save/Delete Annotation
        public void EditAnnotationButtonClick(object sender, EventArgs e)
        {
			if (this.editAnnotationButton.Enabled)
			{
				this.EnsureVisible();
				this.EditCurrentAnnotation();
				this.UpdateButtonsState();
			}
        }

        public void SaveAnnotationButtonClick(object sender, EventArgs e)
        {
			if (this.saveAnnotationButton.Enabled)
			{
				this.EnsureVisible();
				this.SaveCurrentAnnotation();
				this.RenderAnnotation();
				this.UpdateButtonsState();
			}
        }

        private void DeleteAnnotationButton_Click(object sender, EventArgs e)
        {
            this.DeleteCurrentAnnontation();
            this.UpdateAnnotation();
        }

        private void SaveCurrentAnnotation()
        {
            Debug.Assert(this.Editing);
            Debug.Assert(this.currentChange != null);
            Debug.Assert(this.editedAnnotation != null);
            this.Editing = false;
            this.currentChange.ChangedBy = Environment.UserName;
            this.currentChange.ChangedDate = DateTime.Now;
            this.currentChange.Comment = this.historyTextBox.Text;
            this.currentChange.Status = (CodeAnnotationStatus)Enum.Parse(typeof(CodeAnnotationStatus), this.statusComboBox.Value);
            this.currentChange.Resolution = (CodeAnnotationResolution)Enum.Parse(typeof(CodeAnnotationResolution), this.resolutionComboBox.Value);

            this.editedAnnotation.CommitChange(this.currentChange);
			if (!this.review.Annotations.Contains(this.editedAnnotation))
			{
				this.review.Annotations.Add(this.editedAnnotation);
				this.codeAnnotationListView.AddAnnotation(this.editedAnnotation);
			}
            this.editedAnnotation = null;
            this.currentChange = null;

            // save file
            this.SaveReview(this.review);
            this.windowManager.StatusBar.Text = "saved " + this.CurrentFileName;

            // move to review element
            this.UpdateAnnotation();
        }

        private void EditCurrentAnnotation()
        {
            Debug.Assert(!this.Editing);
            Debug.Assert(this.assemblyBrowser.ActiveItem != null);

            // new annotation?
            this.editedAnnotation = (this.CurrentAnnotation == null) ? new CodeAnnotation() : this.CurrentAnnotation;

            Debug.Assert(this.assemblyBrowser.ActiveItem != null);
            this.editedAnnotation.Identifier = new CodeIdentifier(this.assemblyBrowser.ActiveItem);
            this.currentChange = new CodeChange();
            this.historyTextBox.ResetText();
            if (!this.IsCurrentAnnotation)
            {
                this.statusComboBox.Value = CodeAnnotationStatus.Active.ToString();
                this.resolutionComboBox.Value = CodeAnnotationResolution.Investigated.ToString();
            }

            this.historyTextBox.Select();
            this.historyTextBox.Select(0, 0);
            this.Editing = true;
            this.windowManager.StatusBar.Text = string.Format("Editing {0}", this.assemblyBrowser.ActiveItem);
        }

        private void DeleteCurrentAnnontation()
        {
            if (this.Editing)
            {
                this.currentChange = null;
                this.historyTextBox.ReadOnly = true;
                this.Editing = false;
            }
            else
            {
				this.codeAnnotationListView.RemoveAnnotation(this.CurrentAnnotation);

                this.review.Annotations.RemoveAt(this.CurrentAnnotationIndex);
                this.isCurrentAnnotation = false;
                this.statusComboBox.Value = null;
                this.resolutionComboBox.Value = null;
                this.CurrentAnnotationIndex++;
                
				// save file
                this.SaveReview(this.review);
                this.windowManager.StatusBar.Text = "Saved to review file.";
            }
        }
        #endregion

		private void CodeAnnotationListView_ActiveAnnotationChanged(object sender, EventArgs e)
		{
			CodeAnnotation annotation = this.codeAnnotationListView.ActiveAnnotation;
			if (annotation != null)
			{
				object item = annotation.Identifier.Resolve(this.assemblyManager, this.assemblyCache);
				if (item != null)
				{
					bool listHasFocus = this.codeAnnotationListView.Focused;
					bool textHasFocus = this.historyTextBox.Focused;

					this.assemblyBrowser.ActiveItem = item;

					if (listHasFocus)
					{
						this.codeAnnotationListView.Focus();
					}

					else if (textHasFocus)
					{
						this.historyTextBox.Focus();
					}
				}
			}
		}

        private void AssemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
        {
			if (this.Parent != null)
			{
				if (!this.Editing)
				{
					this.UpdateAnnotation();
				}
			}
        }

        private void UpdateButtonsState()
        {
            if (this.assemblyBrowser.ActiveItem == null)
            {
				foreach (CommandBarItem item in this.toolBar.Items)
				{
					CommandBarControl control = item as CommandBarControl;
					if (control != null)
					{
						control.Enabled = false;
					}
				}
            }
            else
            {
                this.nextAnnotationButton.Enabled =
                this.previousAnnotationButton.Enabled =
                    this.review.Annotations.Count > 0
                    && !this.Editing;

                this.editAnnotationButton.Enabled = !this.Editing;
                this.saveAnnotationButton.Enabled = this.Editing;
                this.deleteAnnotationButton.Enabled = this.Editing || this.CurrentAnnotation != null;

                this.statusComboBox.Enabled = this.Editing;
                this.resolutionComboBox.Enabled = this.Editing;
                if (!this.Editing && this.IsCurrentAnnotation)
                {
                    this.statusComboBox.Value = null;
                    this.resolutionComboBox.Value = null;
                }

                this.newReviewButton.Enabled = !this.Editing;
                this.loadReviewButton.Enabled = !this.Editing;
                this.saveReviewButton.Enabled = !this.Editing;
                this.mergeReviewsButton.Enabled = !this.Editing;
            }
        }

        private void UpdateAnnotation()
        {            
            if (this.assemblyBrowser.ActiveItem != null)
			{
				int index = this.review.FindAnnotation(this.assemblyBrowser.ActiveItem);
				if (index < 0)
				{
					this.isCurrentAnnotation = false;
					this.codeAnnotationListView.ActiveAnnotation = null;
				}
				else
				{
					this.isCurrentAnnotation = true;
					this.CurrentAnnotationIndex = index;
				}

				this.RenderAnnotation();
				this.UpdateButtonsState();
			}
			else
			{
				this.codeAnnotationListView.ActiveAnnotation = null;
			}
        }

        private void RenderAnnotation()
        {
            RichTextFormatter formatter = new RichTextFormatter();
            if (this.CurrentFileName.Length > 0)
            {
				formatter.WriteBold("File: ");
                formatter.Write(this.CurrentFileName);
                formatter.WriteLine();
                formatter.WriteLine();
            }

			if (!this.IsCurrentAnnotation)
			{
				this.windowManager.StatusBar.Text = string.Empty;
                this.statusComboBox.Value = null;
                this.resolutionComboBox.Value = null;
                formatter.Write("Press Ctrl+W to add an annotation.");
                formatter.WriteLine();
                this.historyTextBox.Rtf = formatter.ToString();
                return;
			}

            this.windowManager.StatusBar.Text = string.Format("Annotation of {0}", this.CurrentAnnotation.Identifier.Identifier);
            this.statusComboBox.Value = this.CurrentAnnotation.Status.ToString();
            this.resolutionComboBox.Value = this.CurrentAnnotation.Resolution.ToString();

            for (int i = this.CurrentAnnotation.Changes.Count - 1; i >= 0; i--)
            {
                CodeChange change = (CodeChange)this.CurrentAnnotation.Changes[i];

                // formatter.WriteBold(string.Format("{0}: edited by {1}, {2} - {3}", change.ChangedDate.ToString("g"), change.ChangedBy, change.Status, change.Resolution));
				formatter.WriteBold(change.ChangedDate.ToString("g"));
				formatter.Write(": ");
				formatter.Write("edited by ");
				formatter.WriteBold(change.ChangedBy);
				formatter.Write(", ");
				formatter.Write(change.Status.ToString());
				formatter.Write(", ");
				formatter.Write(change.Resolution.ToString());
                formatter.WriteLine();

                if (change.Comment != null && change.Comment.Length > 0)
                {
                    formatter.Write(change.Comment);
                    formatter.WriteLine();
                }

                foreach (CodeChangedField changedField in change.ChangedFields)
                {
                    formatter.Write(String.Format("{0} = '{1}'", changedField.Name, changedField.Value));
                    formatter.WriteLine();
                }
                formatter.WriteLine();
            }

            this.historyTextBox.Rtf = formatter.ToString();
        }

		public string CurrentFileName
		{
			get
			{
				return this.currentFileName;
			}

			set
			{
				this.currentFileName = value;

				IConfiguration configuration = this.configurationManager["Reflector.Review"];
				configuration.ClearProperty("ReviewFileName");
				configuration.SetProperty("ReviewFileName", this.currentFileName, string.Empty);
			}
		}

		private void LoadConfiguration()
		{
			IConfiguration configuration = this.configurationManager["Reflector.Review"];
			if (configuration.HasProperty("ReviewFileName"))
			{
				this.currentFileName = configuration.GetProperty("ReviewFileName");
			}
		}

		private OpenFileDialog CreateOpenDialog()
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.DefaultExt = ".review";
			dialog.InitialDirectory = Environment.CurrentDirectory;
			dialog.Title = "Open Reviews";
			dialog.Multiselect = true;
			dialog.Filter = dialogFilter;
			return dialog;
		}

		private CodeReview LoadReview(string file)
		{
			try
			{
				using (Stream stream = File.OpenRead(file))
				{
					CodeReview codeReview = new CodeReview();
					codeReview.Load(new XmlTextReader(stream));
					return codeReview;
				}
			}
			catch (IOException exception)
			{
				MessageBox.Show(exception.Message, "Review", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return null;
			}
		}

		private CodeReview[] LoadReviews()
		{
			OpenFileDialog dialog = this.CreateOpenDialog();
			dialog.Multiselect = true;
			if ((dialog.ShowDialog() != DialogResult.OK) || (dialog.FileNames == null) || (dialog.FileNames.Length == 0))
			{
				return null;
			}

			try
			{
				CodeReview[] reviews = new CodeReview[dialog.FileNames.Length];
				for (int i = 0; i < dialog.FileNames.Length; ++i)
				{
					string reviewFile = dialog.FileNames[i];

					using (Stream stream = File.OpenRead(reviewFile))
					{
						CodeReview codeReview = new CodeReview();
						codeReview.Load(new XmlTextReader(stream));
						reviews[i] = codeReview;
					}
				}

				return reviews;
			}
			catch (IOException exception)
			{
				MessageBox.Show(exception.Message, "Review", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}
		}

		public bool SaveAsReview(CodeReview review)
		{
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Title = "Save Review";
			dialog.InitialDirectory = Environment.CurrentDirectory;
			dialog.DefaultExt = ".review";
			dialog.FileName = "api.review";
			dialog.Filter = dialogFilter;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				this.CurrentFileName = dialog.FileName;
				this.SaveReview(review);
				return true;
			}

			return false;
		}

		public void SaveReview(CodeReview review)
		{
			if (this.CurrentFileName.Length == 0)
			{
				this.SaveAsReview(review);
			}
			else
			{
				string fileName = this.CurrentFileName;
				try
				{
					using (StreamWriter writer = new StreamWriter(fileName))
					{
						XmlTextWriter xmlWriter = new XmlTextWriter(writer);
						xmlWriter.Formatting = Formatting.Indented;
						review.Save(xmlWriter);
					}
				}
				catch (IOException exception)
				{
					MessageBox.Show(exception.Message, "Review", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
	}
}
