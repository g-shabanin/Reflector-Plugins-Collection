namespace LiveSequence.Common.Graphics
{
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Windows.Media;
  using System.Windows.Media.Animation;
  using System.Windows.Threading;
  using LiveSequence.Common.Domain;

  /// <summary>
  /// Main container for the Sequence diagram.
  /// </summary>
  public sealed class Diagram : FrameworkElement
  {
    /// <summary>
    /// List of rows in the diagram. Each row contains groups, and each group contains nodes.
    /// </summary>
    private List<DiagramRow> rows = new List<DiagramRow>();

    /// <summary>
    /// Populates the rows with nodes.
    /// </summary>
    private DiagramController logic;

    /// <summary>
    /// Size of the diagram. Used to layout all of the nodes before the
    /// control gets an actual size.
    /// </summary>
    private Size totalSize = new Size(0, 0);

    /// <summary>
    /// Zoom level of the diagram.
    /// </summary>
    private double scale = 1.0;

    /// <summary>
    /// Bounding area of the selected node, the selected node is the 
    /// non-primary node that is selected, and will become the primary node.
    /// </summary>
    private Rect selectedNodeBounds = Rect.Empty;

    /// <summary>
    /// Flag if currently populating or not. Necessary since diagram populate 
    /// contains several parts and animations, request to update the diagram
    /// are ignored when this flag is set.
    /// </summary>
    private bool populating;

    /// <summary>
    /// The person that has been added to the diagram.
    /// </summary>
    private ObjectInfo newObjectInfo;

    /// <summary>
    /// Timer used with the repopulating animation.
    /// </summary>
    private DispatcherTimer animationTimer = new DispatcherTimer();

    /// <summary>
    /// Initializes a new instance of the <see cref="Diagram"/> class.
    /// </summary>
    public Diagram()
    {
      // Init the diagram logic, which handles all of the layout logic.
      this.logic = new DiagramController();
      this.logic.NodeClickHandler = new RoutedEventHandler(this.OnNodeClick);

      // Can have an empty object collection when in design tools such as Blend.
      if (this.logic.DiagramModel != null)
      {
        this.logic.DiagramModel.ContentChanged += new EventHandler<ContentChangedEventArgs>(this.OnDiagramModelContentChanged);
        this.logic.DiagramModel.CurrentChanged += new EventHandler(this.OnDiagramModelCurrentChanged);
      }
    }

    /// <summary>
    /// Occurs when [diagram updated].
    /// </summary>
    public event EventHandler DiagramUpdated;

    /// <summary>
    /// Occurs when [diagram populated].
    /// </summary>
    public event EventHandler DiagramPopulated;

    /// <summary>
    /// Gets or sets the zoom level of the diagram.
    /// </summary>
    public double Scale
    {
      get
      {
        return this.scale;
      }

      set
      {
        if (this.scale != value)
        {
          this.scale = value;
          this.LayoutTransform = new ScaleTransform(this.scale, this.scale);
        }
      }
    }

    /// <summary>
    /// Gets the bounding area (relative to the diagram) of the primary node.
    /// </summary>
    public Rect PrimaryNodeBounds
    {
      get
      {
        return this.logic.GetNodeBounds(this.logic.DiagramModel.Current);
      }
    }

    /// <summary>
    /// Gets the bounding area (relative to the diagram) of the selected node.
    /// The selected node is the non-primary node that was previously selected
    /// to be the primary node.
    /// </summary>
    public Rect SelectedNodeBounds
    {
      get
      {
        return this.selectedNodeBounds;
      }
    }

    /// <summary>
    /// Gets the number of nodes in the diagram.
    /// </summary>
    public int NodeCount
    {
      get
      {
        return this.logic.ObjectInfoLookup.Count;
      }
    }

    /// <summary>
    /// Gets the number of visual child elements within this element.
    /// </summary>
    /// <value></value>
    /// <returns>The number of visual child elements for this element.</returns>
    protected override int VisualChildrenCount
    {
      // Return the number of rows.
      get
      {
        return this.rows.Count;
      }
    }

    /// <summary>
    /// Draw the connector lines at a lower level (OnRender) instead
    /// of creating visual tree objects.
    /// </summary>
    /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
    protected override void OnRender(DrawingContext drawingContext)
    {
      // Draw connectors so message information appears.
      foreach (DiagramConnector connector in this.logic.Connections)
      {
        connector.Draw(drawingContext);
      }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
    protected override void OnInitialized(EventArgs e)
    {
      this.UpdateDiagram();
      base.OnInitialized(e);
    }

    /// <summary>
    /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)"/>, and returns a child at the specified index from a collection of child elements.
    /// </summary>
    /// <param name="index">The zero-based index of the requested child element in the collection.</param>
    /// <returns>
    /// The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.
    /// </returns>
    protected override Visual GetVisualChild(int index)
    {
      // Return the requested row.
      return this.rows[index];
    }

    /// <summary>
    /// When overridden in a derived class, measures the size in layout required for child elements and determines a size for the <see cref="T:System.Windows.FrameworkElement"/>-derived class.
    /// </summary>
    /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
    /// <returns>
    /// The size that this element determines it needs during layout, based on its calculations of child element sizes.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
      // Let each row determine how large they want to be.
      Size size = new Size(double.PositiveInfinity, double.PositiveInfinity);
      foreach (DiagramRow row in this.rows)
      {
        row.Measure(size);
      }

      // Return the total size of the diagram.
      return this.ArrangeRows(false);
    }

    /// <summary>
    /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement"/> derived class.
    /// </summary>
    /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
    /// <returns>The actual size used.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
      // Arrange the rows in the diagram, return the total size.
      return this.ArrangeRows(true);
    }

    /// <summary>
    /// Arrange the rows in the diagram, return the total size.
    /// </summary>
    /// <param name="arrange">if set to <c>true</c> [arrange].</param>
    /// <returns>The total size of the diagram</returns>
    private Size ArrangeRows(bool arrange)
    {
      // Location of the row.
      double pos = 0;

      // Bounding area of the row.
      Rect bounds = new Rect();

      // Total size of the diagram.
      Size size = new Size(0, 0);

      foreach (DiagramRow row in this.rows)
      {
        // Row location, center the row horizontaly.
        bounds.Y = pos;
        bounds.X = (this.totalSize.Width == 0) ? 0 :
            bounds.X = (this.totalSize.Width - row.DesiredSize.Width) / 2;

        // Row Size.
        bounds.Width = row.DesiredSize.Width;
        bounds.Height = row.DesiredSize.Height;

        // Arrange the row, save the location.
        if (arrange)
        {
          row.Arrange(bounds);
          row.Location = bounds.TopLeft;
        }

        // Update the size of the diagram.
        size.Width = Math.Max(size.Width, bounds.Width);
        size.Height = pos + row.DesiredSize.Height;

        pos += bounds.Height;
      }

      // Determine final size based on the connector's ultimate right.
      if (this.logic.Connections.Count > 0)
      {
          double ultimateRight = 0;
          foreach (DiagramConnector connector in this.logic.Connections)
          {
              DiagramCallConnector callConnector = connector as DiagramCallConnector;
              if (callConnector != null)
              {
                  ultimateRight = Math.Max(callConnector.UltimateRight, ultimateRight);
              }
          }

          // Ultimate width is the maximum of the found ultimateRight for all connectors and the current size.Width.
          size.Width = Math.Max(size.Width, ultimateRight + 20);
      }

      // Store the size, this is necessary so the diagram
      // can be laid out without a valid Width property.
      this.totalSize = size;
      return size;
    }

    /// <summary>
    /// Reset all of the data associated with the diagram.
    /// </summary>
    private void Clear()
    {
      foreach (DiagramRow row in this.rows)
      {
        row.Clear();
        this.RemoveVisualChild(row);
      }

      this.rows.Clear();
      this.logic.Clear();
    }

    /// <summary>
    /// Populate the diagram. Update the diagram and hide all non-primary nodes.
    /// Then pause, and finish the populate by fading in the new nodes.
    /// </summary>
    private void Populate()
    {
      // Set flag to ignore future updates until complete.
      this.populating = true;

      // Update the nodes in the diagram.
      this.UpdateDiagram();

      // Pause before displaying the new nodes.
      this.animationTimer.Interval = TimeSpan.FromMilliseconds(Constants.AnimationPauseDuration);
      this.animationTimer.Tick += new EventHandler(this.OnAnimationTimer);
      this.animationTimer.IsEnabled = true;

      // Let other controls know the diagram has been repopulated.
      this.OnDiagramPopulated();
    }

    /// <summary>
    /// The animation pause timer is complete, finish populating the diagram.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnAnimationTimer(object sender, EventArgs e)
    {
      // Turn off the timer.
      this.animationTimer.IsEnabled = false;

      // Fade each node into view.
      foreach (DiagramConnectorNode connector in this.logic.ObjectInfoLookup.Values)
      {
        if (connector.Node.Visibility != Visibility.Visible)
        {
          connector.Node.Visibility = Visibility.Visible;
          connector.Node.BeginAnimation(
            Diagram.OpacityProperty,
            new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(Constants.NodeFadeInDuration)));
        }
      }

      // Redraw connector lines.
      this.InvalidateVisual();

      this.populating = false;
    }

    /// <summary>
    /// Reset the diagram with the nodes. This is accomplished by creating a series of rows.
    /// Each row contains a series of groups, and each group contains the nodes. The elements 
    /// are not laid out at this time. Also creates the connections between the nodes.
    /// </summary>
    private void UpdateDiagram()
    {
      // Necessary for Blend.
      if (this.logic.DiagramModel == null)
      {
        return;
      }

      // First reset everything.
      this.Clear();

      // Check the kind of model.
      if (this.logic.DiagramModel.Count > 0 && this.logic.DiagramModel[0] is ExtendedObjectInfo)
      {
        UpdateClassDiagram();
      }
      else
      {
        UpdateSequenceDiagram();
      }

      // Raise event so others know the diagram was updated.
      this.OnDiagramUpdated();

      // Animate the new object info (optional, might not be any new objects).
      this.AnimateNewObjectInfo();
    }

    private void UpdateClassDiagram()
    {
      // create the primary row with the object info.
      DiagramRow objectInfoRow = this.logic.CreateClassModelPrimaryRow(this.logic.DiagramModel.Current);
      this.AddRow(objectInfoRow);

      DiagramRow descendantRow = this.logic.CreateClassModelDescendantRow();
      this.AddRow(descendantRow);

      // TODO: create additional rows for descendant items...
      // TODO: create 'final' rows with non connected items
      this.logic.CreateClassModelConnections();

      // currently here to loop through groups....
      foreach (DiagramGroup group in objectInfoRow.Groups)
      {
        // ????....
      }
    }

    private void UpdateSequenceDiagram()
    {
      // create the primary row with the object info.
      DiagramRow objectInfoRow = this.logic.CreateSequencePrimaryRow();
      this.AddRow(objectInfoRow);

      // create a starter message row with empty MessageInfo
      DiagramRow messageStarterRow = this.logic.CreateSequenceMessageRow(null);
      this.AddRow(messageStarterRow);

      // create the messages
      foreach (MessageInfo messageInfo in this.logic.Messages)
      {
        DiagramRow messageInfoRow = this.logic.CreateSequenceMessageRow(messageInfo);
        this.AddRow(messageInfoRow);
      }

      // add a finalizer row
      DiagramRow messageFinalizerRow = this.logic.CreateSequenceMessageRow(null);
      this.AddRow(messageFinalizerRow);
}

    /// <summary>
    /// Add a row to the visual tree.
    /// </summary>
    /// <param name="row">The diagram row.</param>
    private void AddRow(DiagramRow row)
    {
      if (row != null && row.NodeCount > 0)
      {
        this.AddVisualChild(row);
        this.rows.Add(row);
      }
    }

    /// <summary>
    /// A node was clicked, make that node the primary node.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void OnNodeClick(object sender, RoutedEventArgs e)
    {
      // Get the node that was clicked.
      DiagramNode node = sender as DiagramNode;
      if (node != null)
      {
        // Make it the primary node. This raises the CurrentChanged
        // event, which repopulates the diagram.
        this.logic.DiagramModel.Current = node.ObjectInfo;
      }
    }

    /// <summary>
    /// Called when the current object in the main ObjectInfo collection changes.
    /// This means the diagram should be updated based on the new selected object info.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnDiagramModelCurrentChanged(object sender, EventArgs e)
    {
      // Save the bounds for the current primary object info,
      // this is required later when animating the diagram.
      this.selectedNodeBounds = this.logic.GetNodeBounds(this.logic.DiagramModel.Current);

      // Repopulate the diagram.
      this.Populate();
    }

    /// <summary>
    /// Called when data changed in the main ObjectInfo collection. This can be
    /// a node being filtered out of the collection, updated ObjectInfo details, and
    /// updated relationship data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="OpenSequence.Xps.Data.ContentChangedEventArgs"/> instance containing the event data.</param>
    private void OnDiagramModelContentChanged(object sender, ContentChangedEventArgs e)
    {
      if (this.populating)
      {
        return;
      }

      // Save the object info that is being added to the diagram.
      // This is optional and can be null.
      this.newObjectInfo = e.NewInfo;

      // Redraw the diagram.
      this.UpdateDiagram();
      this.InvalidateMeasure();
      this.InvalidateArrange();
      this.InvalidateVisual();
    }

    /// <summary>
    /// Animate the new person that was added to the diagram.
    /// </summary>
    private void AnimateNewObjectInfo()
    {
      // The new person is optional, can be null.
      if (this.newObjectInfo == null)
      {
        return;
      }

      // Get the UI element to animate.                
      DiagramNode node = this.logic.GetDiagramNode(this.newObjectInfo);
      if (node != null)
      {
        // Create the new person animation.
        DoubleAnimation anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(Constants.NewPersonAnimationDuration));

        // Animate the node.
        ScaleTransform transform = new ScaleTransform();
        transform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        transform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        node.RenderTransform = transform;
      }

      this.newObjectInfo = null;
    }

    /// <summary>
    /// Invokes the DiagramUpdated event.
    /// </summary>
    private void OnDiagramUpdated()
    {
      if (this.DiagramUpdated != null)
      {
        this.DiagramUpdated(this, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Invokes the DiagramPopulated event.
    /// </summary>
    private void OnDiagramPopulated()
    {
      if (this.DiagramPopulated != null)
      {
        this.DiagramPopulated(this, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Static container for private class constants.
    /// </summary>
    private static class Constants
    {
      /// <summary>
      /// Gets the duration to pause before displaying new nodes.
      /// </summary>
      public static double AnimationPauseDuration
      {
        get { return 600; }
      }

      /// <summary>
      /// Gets the duration for nodes to fade in when the diagram is repopulated.
      /// </summary>
      public static double NodeFadeInDuration
      {
        get { return 500; }
      }

      /// <summary>
      /// Gets the duration for the new person animation.
      /// </summary>
      public static double NewPersonAnimationDuration
      {
        get { return 250; }
      }
    }
  }
}
