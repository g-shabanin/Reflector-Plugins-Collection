using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using LiveSequence.Common;
using LiveSequence.Common.Domain;
using LiveSequence.Common.Presentation;
using LiveSequence.Tree;

namespace LiveSequence
{
    internal partial class ConsoleView : Form, IMainFormView
    {
        private readonly string _assemblyName;

        public ConsoleView(MainFormController controller, CommandLineArguments args)
        {
            this.InitializeComponent();
            this.Visible = false;

            this.Controller = controller;
            this._assemblyName = args.AssemblyName;
            this.TypeName = args.TypeName;
            this.DestinationPath = args.DestinationPath;

            // Override config settings.
            Settings.OutputType = args.OutputType;
            Settings.IncludeAssemblyReferences = args.IncludeReferenceAssemblies;

            this.Controller.Initialize(this);
        }

        public MainFormController Controller { get; set; }

        public string TypeName { get; set; }

        public List<SequenceData> SequenceDataGroup { get; private set; }

        public string DestinationPath { get; private set; }

        #region IMainFormView Members

        public DTreeNode<DTreeItem> AssemblyTree { get; set; }

        public string AssemblyFileName
        {
            get { return _assemblyName; }
        }

        public void WorkerProgressChanged(ProgressChangedEventArgs e)
        {
            Console.WriteLine("Processing " + e.UserState);
        }

        public void WorkerCompleted()
        {
            Console.WriteLine("Ready");

            if (string.IsNullOrEmpty(this.TypeName))
            {
                // get data for all types...
                this.SequenceDataGroup = this.Controller.GetSequenceData();
            }
            else
            {
                // now ask the controller to get the sequence data for assembly & typename
                this.SequenceDataGroup = this.Controller.GetSequenceData(this.TypeName);
            }

            // End the work...
            this.Close();
        }

        #endregion

        internal void StartProgress()
        {
            Controller.RunWorker();
        }

        private void ConsoleFormOnLoad(object sender, EventArgs e)
        {
            this.StartProgress();
        }

        private void ConsoleFormOnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                // Do not write everything when the close reason is Windows or TaskManager.
                return;
            }

            Logger.Current.Debug("Process Sequence Data...");
            foreach (SequenceData data in this.SequenceDataGroup)
            {
                IRenderer engine = this.Controller.Renderer;
                if (string.Compare(Settings.OutputType, "XPS", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    engine = new XpsRenderer(true, this.DestinationPath);
                }
                else if (string.Compare(Settings.OutputType, "PNG", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    engine = new PngRenderer(true, this.DestinationPath);
                }

                try
                {
                    engine.Export(data);
                }
                catch (Exception ex)
                {
                    Logger.Current.Error(ex.Message, ex);
                    Environment.FailFast("An exception occured while processing the sequence data. It has been logged to the logfile.");
                }
            }
        }
    }
}