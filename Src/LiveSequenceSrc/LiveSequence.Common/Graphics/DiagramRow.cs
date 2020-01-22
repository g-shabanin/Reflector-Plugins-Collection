#define CODE_ANALYSIS
namespace LiveSequence.Common.Graphics
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Diagnostics.CodeAnalysis;
  using System.Windows;
  using System.Windows.Media;

  /// <summary>
  /// The DiagramRow object contains a list of DiagramGroup objects, which in turn each contain a list of DiagramNode objects.
  /// </summary>
  public sealed class DiagramRow : FrameworkElement
  {
    /// <summary>
    /// Space between each group.
    /// </summary>
    private double groupSpace = 80;

    /// <summary>
    /// Location of the row, relative to the diagram.
    /// </summary>
    private Point location = new Point();

    /// <summary>
    /// List of groups in the row.
    /// </summary>
    private List<DiagramGroup> groups = new List<DiagramGroup>();

    /// <summary>
    /// Gets or sets the space between each group.
    /// </summary>
    /// <value>The group space.</value>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This property has a possible future use.")]
    internal double GroupSpace
    {
      get
      {
        return this.groupSpace;
      }

      set
      {
        this.groupSpace = value;
      }
    }

    /// <summary>
    /// Gets or sets the location of the row, relative to the diagram.
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
    /// Gets the list of groups in the row.
    /// </summary>
    /// <value>The groups collection.</value>
    internal ReadOnlyCollection<DiagramGroup> Groups
    {
      get
      {
        return new ReadOnlyCollection<DiagramGroup>(this.groups);
      }
    }

    /// <summary>
    /// Gets the node count.
    /// </summary>
    /// <value>The node count.</value>
    internal int NodeCount
    {
      get
      {
        int count = 0;
        foreach (DiagramGroup group in this.groups)
        {
          count += group.Nodes.Count;
        }

        return count;
      }
    }

    /// <summary>
    /// Gets the number of visual child elements within this element.
    /// </summary>
    /// <value></value>
    /// <returns>The number of visual child elements for this element.</returns>
    protected override int VisualChildrenCount
    {
      // Return the number of groups.
      get
      {
        return this.groups.Count;
      }
    }

    /// <summary>
    /// Add the group to the row.
    /// </summary>
    /// <param name="group">The diagram group.</param>
    internal void Add(DiagramGroup group)
    {
      this.groups.Add(group);
      this.AddVisualChild(group);
    }

    /// <summary>
    /// Remove all groups from the row.
    /// </summary>
    internal void Clear()
    {
      foreach (DiagramGroup group in this.groups)
      {
        group.Clear();
        this.RemoveVisualChild(group);
      }

      this.groups.Clear();
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
      // Let each group determine how large they want to be.
      Size size = new Size(double.PositiveInfinity, double.PositiveInfinity);
      foreach (DiagramGroup group in this.groups)
      {
        group.Measure(size);
      }

      // Return the total size of the row.
      return this.ArrangeGroups(false);
    }

    /// <summary>
    /// When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement"/> derived class.
    /// </summary>
    /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
    /// <returns>The actual size used.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
      // Arrange the groups in the row, return the total size.
      return this.ArrangeGroups(true);
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
      // Return the requested group.
      return this.groups[index];
    }

    /// <summary>
    /// Arrange the groups in the row, return the total size.
    /// </summary>
    /// <param name="arrange">if set to <c>true</c> [arrange].</param>
    /// <returns>The total size of the DiagramRow</returns>
    private Size ArrangeGroups(bool arrange)
    {
      // Position of the next group.
      double pos = 0;

      // Bounding area of the group.
      Rect bounds = new Rect();

      // Total size of the row.
      Size totalSize = new Size(0, 0);

      foreach (DiagramGroup group in this.groups)
      {
        // Group location.
        bounds.X = pos;
        bounds.Y = 0;

        // Group size.                    
        bounds.Width = group.DesiredSize.Width;
        bounds.Height = group.DesiredSize.Height;

        // Arrange the group, save the location.
        if (arrange)
        {
          group.Arrange(bounds);
          group.Location = bounds.TopLeft;
        }

        // Update the size of the row.
        totalSize.Width = pos + group.DesiredSize.Width;
        totalSize.Height = Math.Max(totalSize.Height, group.DesiredSize.Height);

        pos += (bounds.Width + this.groupSpace);
      }

      return totalSize;
    }
  }
}
