namespace LiveSequence.Common.Graphics
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Windows;
  using System.Windows.Media;

  /// <summary>
  /// The DiagramGroup object is used as a container of related DiagramNode objects.
  /// </summary>
  public sealed class DiagramGroup : FrameworkElement
  {
    /// <summary>
    /// Space between each node.
    /// </summary>
    private const double NodeSpace = 10;

    /// <summary>
    /// Location of the group, relative to the row.
    /// </summary>
    private Point location = new Point();

    /// <summary>
    /// List of nodes in the group.
    /// </summary>
    private List<DiagramNode> nodes = new List<DiagramNode>();

    /// <summary>
    /// Gets or sets the location of the group, relative to the row.
    /// </summary>
    /// <value>The location.</value>
    internal Point Location
    {
      get
      {
        return this.location;
      }

      set
      {
        this.location = value;
      }
    }

    /// <summary>
    /// Gets the list of nodes in the group.
    /// </summary>
    /// <value>The nodes collection.</value>
    internal ReadOnlyCollection<DiagramNode> Nodes
    {
      get
      {
        return new ReadOnlyCollection<DiagramNode>(this.nodes);
      }
    }

    /// <summary>
    /// Gets the number of visual child elements within this element.
    /// </summary>
    /// <value></value>
    /// <returns>The number of visual child elements for this element.</returns>
    protected override int VisualChildrenCount
    {
      // Return the number of nodes.
      get
      {
        return this.nodes.Count;
      }
    }

    /// <summary>
    /// Add the node to the group.
    /// </summary>
    /// <param name="node">The diagram node.</param>
    internal void Add(DiagramNode node)
    {
      this.nodes.Add(node);
      this.AddVisualChild(node);
    }

    /// <summary>
    /// Remove all nodes from the group.
    /// </summary>
    internal void Clear()
    {
      foreach (DiagramNode node in this.nodes)
      {
        this.RemoveVisualChild(node);
      }

      this.nodes.Clear();
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
      // Let each node determine how large they want to be.
      Size size = new Size(double.PositiveInfinity, double.PositiveInfinity);
      foreach (DiagramNode node in this.nodes)
      {
        node.Measure(size);
      }

      // Return the total size of the group.
      return this.ArrangeNodes(false);
    }

    /// <summary>
    /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement"/> derived class.
    /// </summary>
    /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
    /// <returns>The actual size used.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
      // Arrange the nodes in the group, return the total size.
      return this.ArrangeNodes(true);
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
      // Return the requested node.
      return this.nodes[index];
    }

    /// <summary>
    /// Arrange the nodes in the group, return the total size.
    /// </summary>
    /// <param name="arrange">if set to <c>true</c> [arrange].</param>
    /// <returns>The total size of the diagram group.</returns>
    private Size ArrangeNodes(bool arrange)
    {
      // Position of the next node.
      double pos = 0;

      // Bounding area of the node.
      Rect bounds = new Rect();

      // Total size of the group.
      Size totalSize = new Size(0, 0);

      foreach (DiagramNode node in this.nodes)
      {
        // Node location.
        bounds.X = pos;
        bounds.Y = 0;

        // Node size.
        bounds.Width = node.DesiredSize.Width;
        bounds.Height = node.DesiredSize.Height;

        // Arrange the node, save the location.
        if (arrange)
        {
          node.Arrange(bounds);
          node.Location = bounds.TopLeft;
        }

        // Update the size of the group.
        totalSize.Width = pos + node.DesiredSize.Width;
        totalSize.Height = Math.Max(totalSize.Height, node.DesiredSize.Height);

        pos += (bounds.Width + DiagramGroup.NodeSpace);
      }

      return totalSize;
    }
  }
}