#define CODE_ANALYSIS
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "LiveSequence.Common.Graphics.DiagramViewer.#ZoomSliderPanel", Justification = "This message is thrown in error. The ZoomSliderPanel is a container in the viewer's XAML.")]

namespace LiveSequence.Common.Graphics
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Media.Animation;
  using System.Windows.Threading;

  /// <summary>
  /// Interaction logic for DiagramViewer.xaml
  /// </summary>
  public sealed partial class DiagramViewer : UserControl
  {
    /// <summary>
    /// Used when manually scrolling, the start point.
    /// </summary>
    private Point scrollStartPoint;

    /// <summary>
    /// Used when manually scrolling, the start offset.
    /// </summary>
    private Point scrollStartOffset;

    /// <summary>
    /// Stores the top-left offset of the diagram. Used to auto-scroll
    /// the new primary node to the location of the previous selected node.
    /// </summary>
    private Point previousTopLeftOffset;

    /// <summary>
    /// Timer that is used when animating a new diagram.
    /// </summary>
    private DispatcherTimer autoCenterTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramViewer"/> class.
    /// </summary>
    public DiagramViewer()
    {
      InitializeComponent();
      this.InitializeResources();

      // Default zoom level.
      this.Zoom = 1;
    }

    /// <summary>
    /// Gets or sets the zoom level of the diagram.
    /// </summary>
    /// <value>The zoom value.</value>
    public double Zoom
    {
      get
      {
        return this.ZoomSlider.Value;
      }

      set
      {
        if (value >= this.ZoomSlider.Minimum && value <= this.ZoomSlider.Maximum)
        {
          this.Diagram.Scale = value;
          this.ZoomSlider.Value = value;
          this.UpdateScrollSize();
        }
      }
    }

    /// <summary>
    /// Resets the scroll position.
    /// </summary>
    internal void ResetScrollPosition()
    {
      // Just restore the view to center the screen...
      this.AutoScrollToCenter();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
    protected override void OnInitialized(EventArgs e)
    {
      // Timer used for animations.
      this.autoCenterTimer = new DispatcherTimer();

      // Events.
      this.Diagram.Loaded += new RoutedEventHandler(this.Diagram_Loaded);
      this.Diagram.SizeChanged += new SizeChangedEventHandler(this.Diagram_SizeChanged);
      this.Diagram.DiagramUpdated += new EventHandler(this.Diagram_DiagramUpdated);
      this.Diagram.DiagramPopulated += new EventHandler(this.Diagram_DiagramPopulated);

      this.ZoomSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.ZoomSlider_ValueChanged);
      this.ZoomSlider.MouseDoubleClick += new MouseButtonEventHandler(this.ZoomSlider_MouseDoubleClick);

      this.SizeChanged += new SizeChangedEventHandler(this.Diagram_SizeChanged);
      this.ScrollViewer.ScrollChanged += new ScrollChangedEventHandler(this.ScrollViewer_ScrollChanged);

      base.OnInitialized(e);
    }

    /// <summary>
    /// Adjust the time slider for Shift + MouseWheel,
    /// adjust the zoom slider for Ctrl + MouseWheel.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Input.MouseWheelEventArgs"/> that contains the event data.</param>
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
      // Zoom slider.
      if ((Keyboard.Modifiers & ModifierKeys.Control) > 0)
      {
        e.Handled = true;
        this.Zoom += (e.Delta > 0) ? ZoomSlider.LargeChange : -ZoomSlider.LargeChange;
      }

      base.OnPreviewMouseWheel(e);
    }

    /// <summary>
    /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseDown"/> attached routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that one or more mouse buttons were pressed.</param>
    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
      if (ScrollViewer.IsMouseOver && !Diagram.IsMouseOver)
      {
        // Save starting point, used later when determining how much to scroll.
        this.scrollStartPoint = e.GetPosition(this);
        this.scrollStartOffset.X = ScrollViewer.HorizontalOffset;
        this.scrollStartOffset.Y = ScrollViewer.VerticalOffset;

        // Update the cursor if can scroll or not.
        this.Cursor = (ScrollViewer.ExtentWidth > ScrollViewer.ViewportWidth) ||
            (ScrollViewer.ExtentHeight > ScrollViewer.ViewportHeight) ?
            Cursors.ScrollAll : Cursors.Arrow;

        this.CaptureMouse();
      }

      base.OnPreviewMouseDown(e);
    }

    /// <summary>
    /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseMove"/> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Input.MouseEventArgs"/> that contains the event data.</param>
    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
      if (this.IsMouseCaptured)
      {
        // Get the new scroll position.
        Point point = e.GetPosition(this);

        // Determine the new amount to scroll.
        Point delta = new Point(
            (point.X > this.scrollStartPoint.X) ? -(point.X - this.scrollStartPoint.X) : (this.scrollStartPoint.X - point.X),
            (point.Y > this.scrollStartPoint.Y) ? -(point.Y - this.scrollStartPoint.Y) : (this.scrollStartPoint.Y - point.Y));

        // Scroll to the new position.
        ScrollViewer.ScrollToHorizontalOffset(this.scrollStartOffset.X + delta.X);
        ScrollViewer.ScrollToVerticalOffset(this.scrollStartOffset.Y + delta.Y);
      }

      base.OnPreviewMouseMove(e);
    }

    /// <summary>
    /// Invoked when an unhandled <see cref="E:System.Windows.Input.Mouse.PreviewMouseUp"/> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that one or more mouse buttons were released.</param>
    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
      if (this.IsMouseCaptured)
      {
        this.Cursor = Cursors.Arrow;
        this.ReleaseMouseCapture();
      }

      base.OnPreviewMouseUp(e);
    }

    /// <summary>
    /// Initializes the resources.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Windows.Application", Justification = "This control may also be used in non-WPF applications, that have a null-reference on Application.Current. To prevent exceptions, this construct has been used.")]
    private void InitializeResources()
    {
      if (Application.Current == null)
      {
        Application app = new Application();
      }

      if (!Application.Current.Resources.Contains("PrimaryNodeTemplate"))
      {
        Application.Current.Resources.Add("PrimaryNodeTemplate", this.Resources["PrimaryNodeTemplate"]);
      }

      if (!Application.Current.Resources.Contains("MessageNodeTemplate"))
      {
        Application.Current.Resources.Add("MessageNodeTemplate", this.Resources["MessageNodeTemplate"]);
      }

      if (!Application.Current.Resources.Contains("BlockedNodeTemplate"))
      {
        Application.Current.Resources.Add("BlockedNodeTemplate", this.Resources["BlockedNodeTemplate"]);
      }

      if (!Application.Current.Resources.Contains("RoundedNodeTemplate"))
      {
        Application.Current.Resources.Add("RoundedNodeTemplate", this.Resources["RoundedNodeTemplate"]);
      }
    }

    /// <summary>
    /// Handles the Loaded event of the Diagram control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    private void Diagram_Loaded(object sender, RoutedEventArgs e)
    {
      // Initialize the display after the diagram has been loaded,
      // set the scroll size and center the diagram in the display area.
      this.UpdateScrollSize();
      this.AutoScrollToSelected();
    }

    /// <summary>
    /// Handles the SizeChanged event of the Diagram control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
    private void Diagram_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      // Update the scroll size, this is necessary to make sure the
      // user cannot scroll the entire diagram off the display area.
      this.UpdateScrollSize();
    }

    /// <summary>
    /// Handles the DiagramUpdated event of the Diagram control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void Diagram_DiagramUpdated(object sender, EventArgs e)
    {
      // The diagram changed, nothing to do here...
    }

    /// <summary>
    /// Handles the DiagramPopulated event of the Diagram control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void Diagram_DiagramPopulated(object sender, EventArgs e)
    {
      // The diagram was populated. Need to force the diagram to layout 
      // since the diagram values are required to perform animations (need 
      // to know exactly where the primary node is located).

      // Save the current top-left offset before force the layout,
      // this is used later when animating the diagram.
      Point offset = this.GetTopLeftScrollOffset();
      this.previousTopLeftOffset = new Point(
          Grid.ActualWidth - ScrollViewer.HorizontalOffset - offset.X,
          Grid.ActualHeight - ScrollViewer.VerticalOffset - offset.Y);

      // Force the layout.
      this.UpdateLayout();

      // Now auto-scroll so the primary node appears at the previous
      // selected node location.
      this.AutoScrollToSelected();
    }

    /// <summary>
    /// Handles the ValueChanged event of the ZoomSlider control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedPropertyChangedEventArgs&lt;System.Double&gt;"/> instance containing the event data.</param>
    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      // Update the diagram zoom level.
      this.Zoom = e.NewValue;
    }

    /// <summary>
    /// Handles the MouseDoubleClick event of the ZoomSlider control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
    private void ZoomSlider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      // Reset the zoom level.
      ZoomSlider.Value = 1.0;
    }

    /// <summary>
    /// Handles the ScrollChanged event of the ScrollViewer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Controls.ScrollChangedEventArgs"/> instance containing the event data.</param>
    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      //// Need to adjust the scroll position when zoom changes to keep 
      //// the diagram centered. The ScrollChanged event occurs when
      //// zooming since the diagram's extent changes.

      if (e.ExtentWidthChange != 0 &&
          e.ExtentWidthChange != e.ExtentWidth)
      {
        // Keep centered horizontaly.
        double percent = e.ExtentWidthChange / (e.ExtentWidth - e.ExtentWidthChange);
        double middle = e.HorizontalOffset + (e.ViewportWidth / 2);
        ScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset + (middle * percent));
      }

      if (e.ExtentHeightChange != 0 &&
          e.ExtentHeightChange != e.ExtentHeight)
      {
        // Keep centered verically.
        double percent = e.ExtentHeightChange / (e.ExtentHeight - e.ExtentHeightChange);
        double middle = e.VerticalOffset + (e.ViewportHeight / 2);
        ScrollViewer.ScrollToVerticalOffset(e.VerticalOffset + (middle * percent));
      }
    }

    /// <summary>
    /// Return the offset that positions the diagram in the top-left
    /// corner, takes into account the zoom level.
    /// </summary>
    /// <returns>The Point that identifies the offset.</returns>
    private Point GetTopLeftScrollOffset()
    {
      // Offset that is returned.
      Point offset = new Point();

      // Empty offset if the diagram is empty.
      if (Diagram.ActualWidth == 0 || Diagram.ActualHeight == 0)
      {
        return offset;
      }

      // Get the size of the diagram.
      Size diagramSize = new Size(
          Diagram.ActualWidth * this.Zoom,
          Diagram.ActualHeight * this.Zoom);

      // Calcualte the offset that positions the diagram in the top-left corner.
      offset.X = this.ActualWidth + diagramSize.Width - (Constants.PanMargin / 2);
      offset.Y = this.ActualHeight + diagramSize.Height - (Constants.PanMargin / 2);

      return offset;
    }

    /// <summary>
    /// Update the scroll area so the diagram can be scrolled from edge to edge.
    /// </summary>
    private void UpdateScrollSize()
    {
      // Nothing to do if the diagram is empty.
      if (this.ActualWidth == 0 || this.ActualHeight == 0)
      {
        return;
      }

      Size diagramSize = new Size(
          Diagram.ActualWidth * this.Zoom,
          Diagram.ActualHeight * this.Zoom);

      // The grid contains the diagram, set the size of the grid so it's
      // large enough to allow the diagram to scroll from edge to edge.
      Grid.Width = Math.Max(0, (this.ActualWidth * 2) + diagramSize.Width - Constants.PanMargin);
      Grid.Height = Math.Max(0, (this.ActualHeight * 2) + diagramSize.Height - Constants.PanMargin);
    }

    /// <summary>
    /// Scroll the diagram so the primary node appears at the location of the selected node.
    /// </summary>
    private void AutoScrollToSelected()
    {
      // Don't scroll if the diagram is empty.
      if (Diagram.ActualWidth == 0 || Diagram.ActualHeight == 0)
      {
        return;
      }

      // This is the offset that will be scrolled. First get the offset 
      // that positions the diagram in the top-left corner.
      Point offset = this.GetTopLeftScrollOffset();

      // Get the location of the node that was selected.
      Rect selectedBounds = Diagram.SelectedNodeBounds;

      // See if this is the first time the diagram is being displayed.            
      if (selectedBounds.IsEmpty)
      {
        // First time, center the diagram in the display area.
        offset.X += ((this.ActualWidth - (Diagram.ActualWidth * this.Zoom)) / 2);
        offset.Y += ((this.ActualHeight - (Diagram.ActualHeight * this.Zoom)) / 2);
      }
      else
      {
        //// Scroll the diagram so the new primary node is at the location
        //// of the previous selected node. 

        // Offset the distance the diagram is scrolled from the 
        // previous top-left position.
        offset.X += this.previousTopLeftOffset.X;
        offset.Y += this.previousTopLeftOffset.Y;

        // Determine the distance between the two nodes.
        Rect primaryBounds = Diagram.PrimaryNodeBounds;
        Point nodeDelta = new Point();
        nodeDelta.X = (primaryBounds.Left + (primaryBounds.Width / 2)) -
            (selectedBounds.Left + (selectedBounds.Width / 2));
        nodeDelta.Y = (primaryBounds.Top + (primaryBounds.Height / 2)) -
            (selectedBounds.Top + (selectedBounds.Height / 2));

        // Offset the distance between the two nodes.
        offset.X -= (nodeDelta.X * this.Zoom);
        offset.Y -= (nodeDelta.Y * this.Zoom);
      }

      // Scroll the diagram.
      ScrollViewer.ScrollToHorizontalOffset(Grid.Width - offset.X);
      ScrollViewer.ScrollToVerticalOffset(Grid.Height - offset.Y);

      // Set a timer so there is a pause before centering the diagram.
      this.autoCenterTimer.Interval = TimeSpan.FromMilliseconds(Constants.AutoCenterAnimationPauseDuration);
      this.autoCenterTimer.Tick += new EventHandler(this.OnAutoCenterPauseTimer);
      this.autoCenterTimer.IsEnabled = true;
    }

    /// <summary>
    /// Called when [auto center pause timer].
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnAutoCenterPauseTimer(object sender, EventArgs e)
    {
      // Timer only fires once.
      this.autoCenterTimer.IsEnabled = false;

      // Scroll the diagram so it's centered in the display area.
      this.AutoScrollToCenter();
    }

    /// <summary>
    /// Center the diagram in the display area.
    /// </summary>
    private void AutoScrollToCenter()
    {
      // Adjust the offset so the diagram appears in the center of 
      // the display area. First get the top-left offset.
      Point offset = this.GetTopLeftScrollOffset();

      // Now adjust the offset so the diagram is centered.
      offset.X += ((this.ActualWidth - (Diagram.ActualWidth * this.Zoom)) / 2);
      offset.Y += ((this.ActualHeight - (Diagram.ActualHeight * this.Zoom)) / 2);

      // Before auto scroll, determine the start and end 
      // points so the scrolling can be animated.
      Point startLocation = new Point(
          ScrollViewer.HorizontalOffset,
          ScrollViewer.VerticalOffset);

      Point endLocation = new Point(
          Grid.Width - offset.X - startLocation.X,
          Grid.Height - offset.Y - startLocation.Y);

      // Auto scroll the diagram.
      this.ScrollViewer.ScrollToHorizontalOffset(Grid.Width - offset.X);
      this.ScrollViewer.ScrollToVerticalOffset(Grid.Height - offset.Y);

      // Animate the scrollings.
      this.AnimateDiagram(endLocation);
    }

    /// <summary>
    /// Animate the diagram by moving from the startLocation to the endLocation.
    /// </summary>
    /// <param name="endLocation">The end location.</param>
    private void AnimateDiagram(Point endLocation)
    {
      // Create the animations, nonlinear by using accelration and deceleration.
      DoubleAnimation horzAnim = new DoubleAnimation(endLocation.X, 0, TimeSpan.FromMilliseconds(Constants.AutoCenterAnimationDuration));
      horzAnim.AccelerationRatio = .5;
      horzAnim.DecelerationRatio = .5;

      DoubleAnimation vertAnim = new DoubleAnimation(endLocation.Y, 0, TimeSpan.FromMilliseconds(Constants.AutoCenterAnimationDuration));
      vertAnim.AccelerationRatio = .5;
      vertAnim.DecelerationRatio = .5;

      // Animate the transform to make it appear like the diagram is moving.
      TranslateTransform transform = new TranslateTransform();
      transform.BeginAnimation(TranslateTransform.XProperty, horzAnim);
      transform.BeginAnimation(TranslateTransform.YProperty, vertAnim);

      // Animate the grid (that contains the diagram) instead of the 
      // diagram, otherwise nodes are clipped in the diagram.
      Grid.RenderTransform = transform;
    }

    /// <summary>
    /// Static class that contains the private constants
    /// </summary>
    private static class Constants
    {
      /// <summary>
      /// Gets the amount of the diagram that cannot be scrolled, this 
      /// ensures that some of the diagram is always visible.
      /// </summary>
      public static double PanMargin
      {
        get { return 50; }
      }

      /// <summary>
      /// Gets the duration to pause before auto centering the diagram.
      /// </summary>
      public static double AutoCenterAnimationPauseDuration
      {
        get { return 1000; }
      }

      /// <summary>
      /// Gets the duration of the auto center animation.
      /// </summary>
      public static double AutoCenterAnimationDuration
      {
        get
        {
          return 600;
        }
      }
    }
  }
}
