namespace LiveSequence.Common.Graphics
{
  using System.Globalization;
  using System.Windows;
  using System.Windows.Controls;
  using LiveSequence.Common.Domain;

  /// <summary>
  /// Represents a node in the class model diagram
  /// </summary>
  public sealed class DiagramModelNode : DiagramNode
  {
    /// <summary>
    /// Dependency property for the NodeColor property.
    /// </summary>
    public static readonly DependencyProperty NodeColorProperty =
        DependencyProperty.Register("NodeColor", typeof(string), typeof(DiagramModelNode));

    /// <summary>
    /// Dependency property for the NodeSubLabel property.
    /// </summary>
    public static readonly DependencyProperty NodeSubLabelProperty =
        DependencyProperty.Register("NodeSubLabel", typeof(string), typeof(DiagramModelNode));

    /// <summary>
    /// Dependency property for the NodeBaseLabel property.
    /// </summary>
    public static readonly DependencyProperty NodeBaseLabelProperty =
        DependencyProperty.Register("NodeBaseLabel", typeof(string), typeof(DiagramModelNode));

    /// <summary>
    /// Dependency property for the NodeLabelStyle property.
    /// </summary>
    public static readonly DependencyProperty NodeLabelStyleProperty =
        DependencyProperty.Register("NodeLabelStyle", typeof(string), typeof(DiagramModelNode));

    /// <summary>
    /// Dependency property for the NodeBorderStyle property.
    /// </summary>
    public static readonly DependencyProperty NodeBorderStyleProperty =
        DependencyProperty.Register("NodeBorderStyle", typeof(string), typeof(DiagramModelNode));

    /// <summary>
    /// Dependency property for the NodeBorderThickness property.
    /// </summary>
    public static readonly DependencyProperty NodeBorderThicknessProperty =
        DependencyProperty.Register("NodeBorderThickness", typeof(string), typeof(DiagramModelNode));

    /// <summary>
    /// Dependency property for the NodeWidth property.
    /// </summary>
    public static readonly DependencyProperty NodeWidthProperty =
        DependencyProperty.Register("NodeWidth", typeof(string), typeof(DiagramModelNode));

    /// <summary>
    /// Gets or sets the color of the node.
    /// </summary>
    /// <value>The color of the node.</value>
    public string NodeColor
    {
      get
      {
        return (string)this.GetValue(DiagramModelNode.NodeColorProperty);
      }

      set
      {
        SetValue(DiagramModelNode.NodeColorProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the node label style.
    /// </summary>
    /// <value>The node label style.</value>
    public string NodeLabelStyle
    {
      get
      {
        return (string)this.GetValue(DiagramModelNode.NodeLabelStyleProperty);
      }

      set
      {
        SetValue(DiagramModelNode.NodeLabelStyleProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the node sub label.
    /// </summary>
    /// <value>The node sub label.</value>
    public string NodeSubLabel
    {
      get
      {
        return (string)this.GetValue(DiagramModelNode.NodeSubLabelProperty);
      }

      set
      {
        SetValue(DiagramModelNode.NodeSubLabelProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the node base label.
    /// </summary>
    /// <value>The node base label.</value>
    public string NodeBaseLabel
    {
      get
      {
        return (string)this.GetValue(DiagramModelNode.NodeBaseLabelProperty);
      }

      set
      {
        SetValue(DiagramModelNode.NodeBaseLabelProperty, value);
      }
    }

    /// <summary>
    /// Gets a value indicating whether the node has a base.
    /// </summary>
    /// <value><c>true</c> if [node has base]; otherwise, <c>false</c>.</value>
    public string NodeHasBase
    {
      get
      {
        if (string.IsNullOrEmpty((string)this.GetValue(DiagramModelNode.NodeBaseLabelProperty)))
        {
          return "Hidden";
        }

        return "Visible";
      }
    }

    /// <summary>
    /// Gets or sets the node border style.
    /// </summary>
    /// <value>The node border style.</value>
    public string NodeBorderStyle
    {
      get
      {
        return (string)this.GetValue(DiagramModelNode.NodeBorderStyleProperty);
      }

      set
      {
        SetValue(DiagramModelNode.NodeBorderStyleProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the node border thickness.
    /// </summary>
    /// <value>The node border thickness.</value>
    public string NodeBorderThickness
    {
      get
      {
        return (string)this.GetValue(DiagramModelNode.NodeBorderThicknessProperty);
      }

      set
      {
        SetValue(DiagramModelNode.NodeBorderThicknessProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the width of the node.
    /// </summary>
    /// <value>The width of the node.</value>
    public string NodeWidth
    {
      get
      {
        return (string)this.GetValue(DiagramModelNode.NodeWidthProperty);
      }

      set
      {
        SetValue(DiagramModelNode.NodeWidthProperty, value);
      }
    }

    /// <summary>
    /// Update the node template based on the node type.
    /// </summary>
    /// <param name="nodeType">Type of the node.</param>
    internal override void UpdateTemplate(NodeType nodeType)
    {
      this.DataContext = this;
      string templateName = "RoundedNodeTemplate";
      if (nodeType == NodeType.Enumeration || nodeType == NodeType.Struct)
      {
        templateName = "BlockedNodeTemplate";
      }

      this.Template = FindResource(templateName) as ControlTemplate;
    }

    /// <summary>
    /// Updates the node layout.
    /// </summary>
    /// <param name="nodeType">Type of the node.</param>
    internal override void UpdateNodeLayout(NodeType nodeType)
    {
      base.UpdateNodeLayout(nodeType);

      this.NodeLabelStyle = string.Empty;
      switch (nodeType)
      {
        case NodeType.Class:
          this.UpdateNodeLayout("#FFD3DCEF", string.Empty, "1", "Class");
          break;
        case NodeType.Abstract:
          this.UpdateNodeLayout("#FFD3DCEF", "2 1", "1", "Abstract Class");
          this.NodeLabelStyle = "Italic";
          break;
        case NodeType.Static:
          this.UpdateNodeLayout("#FFD3DCEF", "4 2", "2", "Static Class");
          break;
        case NodeType.Sealed:
          this.UpdateNodeLayout("#FFD3DCEF", string.Empty, "2", "Sealed Class");
          break;
        case NodeType.Interface:
          this.UpdateNodeLayout("#FFE5F5D5", string.Empty, "1", "Interface");
          break;
        case NodeType.Enumeration:
          this.UpdateNodeLayout("#FFDDD6EF", string.Empty, "1", "Enum");
          break;
        case NodeType.Struct:
          this.UpdateNodeLayout("#FFD3DCEF", string.Empty, "1", "Struct");
          break;
        case NodeType.Delegate:
          this.UpdateNodeLayout("#FFEDDADC", string.Empty, "1", "Delegate");
          break;
        default:
          break;
      }

      // add generic to sub label if necessary
      ExtendedObjectInfo extendedInfo = this.ObjectInfo as ExtendedObjectInfo;
      if (extendedInfo != null)
      {
        if (extendedInfo.GenericParameters.Count > 0)
        {
          this.NodeSubLabel = "Generic " + this.NodeSubLabel;
        }
      }
    }

    /// <summary>
    /// Measures the width.
    /// </summary>
    internal void MeasureWidth()
    {
      this.NodeWidth = DiagramUtility.DetermineLength(this.NodeBaseLabel, "Arial", 7.0F, false, this.NodeWidth).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Called to remeasure a control.
    /// </summary>
    /// <param name="constraint">The maximum size that the method can return.</param>
    /// <returns>
    /// The size of the control, up to the maximum specified by <paramref name="constraint"/>.
    /// </returns>
    protected override Size MeasureOverride(Size constraint)
    {
      Size result = base.MeasureOverride(constraint);

      DiagramSupplement supplement = this.Template.FindName("InterfaceList", this) as DiagramSupplement;
      if (supplement != null)
      {
        // if interfaces found in current type implementation
        ExtendedObjectInfo extendedInfo = this.ObjectInfo as ExtendedObjectInfo;
        if (extendedInfo != null && extendedInfo.Interfaces.Count > 0)
        {
          foreach (string item in extendedInfo.Interfaces)
          {
            supplement.AddInterface(item);
          }
        }

        // add them to addon here and calculate new size
        result.Height = result.Height + supplement.CalculatedHeight;
      }

      return result;
    }

    /// <summary>
    /// Updates the node label.
    /// </summary>
    /// <param name="info">The info that is to be displayed.</param>
    protected override void UpdateNodeLabel(string info)
    {
      base.UpdateNodeLabel(info);

      // set node width
      this.NodeWidth = DiagramUtility.DetermineLength(info, "Arial", 9.0F, true).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Updates the node layout.
    /// </summary>
    /// <param name="color">The color.</param>
    /// <param name="style">The style.</param>
    /// <param name="thickness">The thickness.</param>
    /// <param name="subLabel">The sub label.</param>
    private void UpdateNodeLayout(string color, string style, string thickness, string subLabel)
    {
      // set node color
      this.NodeColor = color;

      // set node border style (dashed or solid)
      this.NodeBorderStyle = style;
      
      // set node border thickness
      this.NodeBorderThickness = thickness;
      
      // set node sub label
      this.NodeSubLabel = subLabel;
    }
  }
}
