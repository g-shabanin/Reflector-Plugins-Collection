namespace Reflector.Sequence
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Windows.Forms;
  using LiveSequence.Common;
  using LiveSequence.Common.Context;
  using LiveSequence.Common.Domain;
  using LiveSequence.Common.Graphics;
  using LiveSequence.Common.Presentation;
  using Reflector;
  using Reflector.CodeModel;

  /// <summary>
  /// Code behind class for the DiagramViewerContainer user control.
  /// </summary>
  public partial class DiagramViewerContainer : UserControl
  {
    /// <summary>
    /// Contains a reference to Reflector's assembly browser.
    /// </summary>
    private readonly IAssemblyBrowser assemblyBrowser;

    /// <summary>
    /// Contains a reference to Reflector's assembly manager.
    /// </summary>
    private readonly IAssemblyManager assemblyManager;

    /// <summary>
    /// Contains a reference to the InstructionDataPopulator object that is used for populating the sequence's call stack.
    /// </summary>
    private readonly InstructionDataPopulator populator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramViewerContainer"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public DiagramViewerContainer(IServiceProvider serviceProvider)
    {
      InitializeComponent();

      this.assemblyBrowser = serviceProvider.GetService(typeof(IAssemblyBrowser)) as IAssemblyBrowser;
      this.assemblyManager = serviceProvider.GetService(typeof(IAssemblyManager)) as IAssemblyManager;
      this.populator = new InstructionDataPopulator(this.assemblyManager);
      this.elementHost.Child = new DiagramViewer();
    }

    /// <summary>
    /// Raises the <see cref="E:ParentChanged"/> event.
    /// </summary>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected override void OnParentChanged(EventArgs e)
    {
      base.OnParentChanged(e);

      if (Parent != null)
      {
        this.assemblyBrowser.ActiveItemChanged += this.OnAssemblyBrowserActiveItemChanged;
        this.assemblyManager.AssemblyLoaded += this.OnAssemblyManagerAssemblyLoadedOrUnloaded;
        this.assemblyManager.AssemblyUnloaded += this.OnAssemblyManagerAssemblyLoadedOrUnloaded;
        this.Translate();
      }
      else
      {
        this.assemblyBrowser.ActiveItemChanged -= this.OnAssemblyBrowserActiveItemChanged;
        this.assemblyManager.AssemblyLoaded -= this.OnAssemblyManagerAssemblyLoadedOrUnloaded;
        this.assemblyManager.AssemblyUnloaded -= this.OnAssemblyManagerAssemblyLoadedOrUnloaded;
      }
    }

    /// <summary>
    /// Called when the assembly browser active item changed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnAssemblyBrowserActiveItemChanged(object sender, EventArgs e)
    {
      if (Parent != null)
      {
        this.Translate();
      }
    }

    /// <summary>
    /// Called when the assembly manager loaded or unloaded an assembly.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnAssemblyManagerAssemblyLoadedOrUnloaded(object sender, EventArgs e)
    {
      if (Parent != null)
      {
        this.populator.RefreshDerivedTypeInformation();
      }
    }

    /// <summary>
    /// Translates this instance.
    /// </summary>
    private void Translate()
    {
      this.populator.CleanUp();
      var method = this.assemblyBrowser.ActiveItem as IMethodDeclaration;
      this.TranslateMethodDeclaration(method);

      //if (method == null)
      //{
      //  // Try the class diagram
      //  this.populator.RefreshDerivedTypeInformation();

      //  var type = this.assemblyBrowser.ActiveItem as ITypeDeclaration;
      //  this.TranslateTypeDeclaration(type);

      //  var space = this.assemblyBrowser.ActiveItem as INamespace;
      //  this.TranslateNamespaceDeclaration(space);
      //}
    }

    /// <summary>
    /// Translates the namespace declaration.
    /// </summary>
    /// <param name="typeNamespace">The type namespace.</param>
    private void TranslateNamespaceDeclaration(INamespace typeNamespace)
    {
      if (typeNamespace != null)
      {
        IList<ClassModelData> modelData = this.populator.BuildModelFromNamespace(typeNamespace);
        ClassDiagramRenderer renderer = new ClassDiagramRenderer();
        renderer.Export(modelData);
      }
    }

    /// <summary>
    /// Translates the type declaration.
    /// </summary>
    /// <param name="type">The type declaration.</param>
    private void TranslateTypeDeclaration(ITypeDeclaration type)
    {
      if (type != null)
      {
        ClassModelData data = this.populator.BuildModelFromType(type);
        ClassDiagramRenderer renderer = new ClassDiagramRenderer();
        renderer.Export(data);
      }
    }

    /// <summary>
    /// Translates the method declaration.
    /// </summary>
    /// <param name="method">The method.</param>
    private void TranslateMethodDeclaration(IMethodDeclaration method)
    {
      if (method != null)
      {
        var body = method.Body as IMethodBody;
        if (body != null)
        {
          SequenceData data = this.populator.BuildGraphFromMethod(method);
          IRenderer renderEngine = new WPFRenderer();
          renderEngine.Export(data);
        }
      }
    }

    /// <summary>
    /// Handles the Click event of the saveToXPSToolStripMenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnSaveAsMenuItemClick(object sender, EventArgs e)
    {
      if (this.saveAsDialog.ShowDialog() == DialogResult.OK)
      {
        try
        {
          using (ContextHelper.CreateSaveScope(this.elementHost.Child as DiagramViewer, this.saveAsDialog.FileName))
          {
            // nothing to do here
            Logger.Current.Info(string.Format(CultureInfo.InvariantCulture, "File saved to {0}", this.saveAsDialog.FileName));
          }
        }
        catch (IOException ex)
        {
          // log error
          Logger.Current.Error(ex.Message, ex);
        }
      }
    }

    /// <summary>
    /// Handles the Click event of the menuItemPrint control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnPrintMenuItemClick(object sender, EventArgs e)
    {
      System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
      if (printDialog.ShowDialog().Equals(false))
      {
        return;
      }

      using (ContextHelper.CreatePrintScope(this.elementHost.Child as DiagramViewer, printDialog))
      {
        // nothing to do here
        Logger.Current.Info(string.Format(CultureInfo.InvariantCulture, "Diagram printed to {0}", printDialog.PrintQueue.FullName));
      }
    }

    /// <summary>
    /// Handles the Click event of the resetFilterToolStripMenuItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnResetFilterMenuItemClick(object sender, EventArgs e)
    {
      this.Translate();
    }
  }
}
