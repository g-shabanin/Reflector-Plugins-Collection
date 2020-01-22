using System;
using System.Collections.Generic;
using System.ComponentModel;
using LiveSequence.Common.Domain;
using LiveSequence.Common.Presentation;
using LiveSequence.Engine;
using LiveSequence.Tree;

namespace LiveSequence
{
    internal class MainFormController
    {
        public IMainFormView View { get; set; }

        public IAssemblyParser AssemblyParser { get; set; }

        public BackgroundWorker Worker { get; set; }

        public IRenderer Renderer { get; set; }

        internal void Initialize(IMainFormView view)
        {
            View = view;

            Renderer = new PicRenderer();

            // Replace it by DI
            AssemblyParser = new CecilAssemblyParser();

            WireEvents();
        }

        private void AssemblyParser_OnProgressChanged(object sender, ProgressEventArgs e)
        {
            if (Worker != null)
            {
                Worker.ReportProgress(0, e.Message);
            }
        }

        internal void ProcessAssembly()
        {
            if (View.AssemblyFileName.Length == 0)
            {
                throw new ArgumentNullException("Assembly file name not available");
            }

            DTreeNode<DTreeItem> assemblyTree = AssemblyParser.Initialize(View.AssemblyFileName);

            View.AssemblyTree = assemblyTree;
        }

        internal void CleanUp()
        {
            AssemblyParser.CleanUp();
        }

        internal SequenceData GetSequenceData(string methodName, string typeName, string nameSpace, string assemblyName)
        {
            return AssemblyParser.GetSequenceData(methodName, typeName, nameSpace, assemblyName);
        }

        internal void WireEvents()
        {
            if (AssemblyParser != null)
            {
                AssemblyParser.OnProgressChanged += AssemblyParser_OnProgressChanged;
            }

            if (Worker != null)
            {
                Worker.WorkerReportsProgress = true;
                Worker.WorkerSupportsCancellation = true;
                Worker.DoWork += Worker_DoWork;
                Worker.ProgressChanged += Worker_ProgressChanged;
                Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            View.WorkerProgressChanged(e);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //this.View.UpdateAssemblyStats();
            View.WorkerCompleted();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcessAssembly();
        }

        internal AssemblyStats GetAssemblyStats()
        {
            return AssemblyParser.GetAssemblyStats();
        }

        internal void RunWorker()
        {
            Worker.RunWorkerAsync();
        }

        internal List<SequenceData> GetSequenceData(string typeName)
        {
            return AssemblyParser.GetSequenceData(typeName);
        }

        internal DTreeNode<DependencyGraphData> GetDependencyGraphData(TreeSelectionData selectedData)
        {
            return AssemblyParser.GetDependencyGraphData(selectedData);
        }

        internal List<SequenceData> GetSequenceData()
        {
            return this.AssemblyParser.GetSequenceData();
        }

        public MainFormController()
        {
            Worker = new BackgroundWorker();
        }
    }
}