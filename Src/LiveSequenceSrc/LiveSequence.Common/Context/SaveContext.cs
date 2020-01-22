namespace LiveSequence.Common.Context
{
  using System;
  using System.IO;
  using System.IO.Packaging;
  using System.Printing;
  using System.Windows;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;
  using System.Windows.Xps;
  using System.Windows.Xps.Packaging;
  using LiveSequence.Common.Graphics;

  /// <summary>
  /// Context class for the Save to XPS functionality.
  /// </summary>
  public sealed class SaveContext : ContextBase
  {
    /// <summary>
    /// Contains a reference to the file name to which the file is saved.
    /// </summary>
    private string xpsFileName;
    
    /// <summary>
    /// Contains a reference to the DiagramViewer that contains the sequence to be saved.
    /// </summary>
    private DiagramViewer viewer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveContext"/> class.
    /// </summary>
    /// <param name="contextParameters">The context parameters.</param>
    internal SaveContext(ContextParameters contextParameters)
    {
      SaveContextParameters scp = contextParameters as SaveContextParameters;
      if (scp == null)
      {
        throw new ArgumentException(Properties.Resources.ArgumentHasInvalidType, "contextParameters");
      }

      this.viewer = scp.Viewer;
      this.xpsFileName = scp.TargetFileName;
    }

    /// <summary>
    /// Gets the diagram control.
    /// </summary>
    /// <value>The diagram control.</value>
    private DiagramViewer DiagramControl
    {
      get
      {
        return this.viewer;
      }
    }

    /// <summary>
    /// Releases the context.
    /// </summary>
    internal override void ReleaseContext()
    {
      // save current diagram transform
      Transform transform = this.DiagramControl.Diagram.LayoutTransform;
      
      // reset current transform (in case it is scaled or rotated)
      this.DiagramControl.Diagram.LayoutTransform = null;

      // Get the desired size of the diagram
      Size size = new Size(this.DiagramControl.Diagram.DesiredSize.Width + 50, this.DiagramControl.Diagram.DesiredSize.Height + 50);
      
      // Measure and arrange elements
      this.DiagramControl.Diagram.Measure(size);
      this.DiagramControl.Diagram.Arrange(new Rect(size));

      // write the data to the XPS file...
      string uniqueName = Path.Combine(Path.GetDirectoryName(this.xpsFileName), Guid.NewGuid().ToString() + Path.GetExtension(this.xpsFileName));
      switch (Path.GetExtension(this.xpsFileName).ToUpperInvariant())
      {
        case ".XPS":
          this.SaveToXps(uniqueName);
          break;
        case ".PNG":
          this.SaveToPng(uniqueName, size);
          break;
        default:
          this.SaveToXps(uniqueName);          // by default save it to xps, even if the extension isn't .XPS.
          break;
      }

      // Restore previously saved layout
      this.DiagramControl.Diagram.LayoutTransform = transform;

      // Reset the scroll
      this.DiagramControl.ResetScrollPosition();

      // copy the xps file
      FileInfo fi = new FileInfo(uniqueName);
      fi.CopyTo(this.xpsFileName, true);
      fi.Delete();
    }

    /// <summary>
    /// Saves to XPS.
    /// </summary>
    /// <param name="uniqueName">Unique name of the file.</param>
    private void SaveToXps(string uniqueName)
    {
      Package package = Package.Open(uniqueName, FileMode.Create);
      XpsDocument xpsDoc = new XpsDocument(package);
      XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
      PrintTicket xpsTicket = new PrintTicket();
      xpsTicket.PageMediaSize = new PageMediaSize(this.DiagramControl.Diagram.DesiredSize.Width + 50, this.DiagramControl.Diagram.DesiredSize.Height + 50);

      // Since Diagram derives from FrameworkElement, the XpsDocument writer knows
      // how to output it's contents. The Diagram is used instead of the DiagramControl
      // so that the diagram is output to fit the content.
      xpsWriter.Write(this.DiagramControl.Diagram, xpsTicket);
      xpsDoc.Close();
      package.Close();
    }

    private void SaveToPng(string uniqueName, Size size)
    {
      // Create a render bitmap and push the surface to it
      RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
        (int)size.Width,
        (int)size.Height,
        96D,
        96D,
        PixelFormats.Pbgra32);
      renderBitmap.Render(this.DiagramControl.Diagram);

      // Create a file stream for saving image
      using (FileStream outStream = new FileStream(uniqueName, FileMode.Create))
      {
        // Use png encoder for our data
        PngBitmapEncoder encoder = new PngBitmapEncoder();

        // push the rendered bitmap to it
        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

        // save the data to the stream
        encoder.Save(outStream);
      }
    }
  }
}
