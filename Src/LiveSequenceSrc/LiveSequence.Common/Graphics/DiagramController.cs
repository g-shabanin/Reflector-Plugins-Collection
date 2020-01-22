#define CODE_ANALYSIS
namespace LiveSequence.Common.Graphics
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Windows;
  using LiveSequence.Common.Context;
  using LiveSequence.Common.Domain;

  /// <summary>
  /// The main controller object for the diagram rendering.
  /// </summary>
  internal sealed class DiagramController
  {
    /// <summary>
    /// Contains a reference to the connections list.
    /// </summary>
    private List<DiagramConnector> connections = new List<DiagramConnector>();

    /// <summary>
    /// Contains a reference to a dictionary, used to lookup the object info nodes.
    /// </summary>
    private Dictionary<ObjectInfo, DiagramConnectorNode> objectInfoLookup = new Dictionary<ObjectInfo, DiagramConnectorNode>();

    /// <summary>
    /// Contains a reference to the collection of info objects.
    /// </summary>
    private ObjectInfoCollection modelCollection;

    /// <summary>
    /// Contains a reference to the collection of messages in the sequence.
    /// </summary>
    private MessageCollection messageCollection;

    /// <summary>
    /// Contains a reference to the collection of relations in the model.
    /// </summary>
    private RelationCollection relationCollection;

    /// <summary>
    /// Callback when a node is clicked.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "The field is used in a property setter. There is no getter. Provided for possible future use.")]
    private RoutedEventHandler nodeClickHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramController"/> class.
    /// </summary>
    internal DiagramController()
    {
      this.modelCollection = DiagramContext.DiagramObjects;
      this.messageCollection = DiagramContext.Messages;
      this.relationCollection = DiagramContext.Relations;

      this.Clear();
    }

    /// <summary>
    /// Gets the diagram model.
    /// </summary>
    /// <value>The diagram model.</value>
    internal ObjectInfoCollection DiagramModel
    {
      get
      {
        return this.modelCollection;
      }
    }

    /// <summary>
    /// Gets the messages.
    /// </summary>
    /// <value>The messages.</value>
    internal MessageCollection Messages
    {
      get
      {
        return this.messageCollection;
      }
    }

    /// <summary>
    /// Gets the relations.
    /// </summary>
    /// <value>The relations.</value>
    internal RelationCollection Relations
    {
      get
      {
        return this.relationCollection;
      }
    }

    /// <summary>
    /// Gets the connections.
    /// </summary>
    /// <value>The connections.</value>
    internal List<DiagramConnector> Connections
    {
      get
      {
        return this.connections;
      }
    }

    /// <summary>
    /// Gets the sequence lookup.
    /// </summary>
    /// <value>The sequence lookup.</value>
    internal Dictionary<ObjectInfo, DiagramConnectorNode> ObjectInfoLookup
    {
      get
      {
        return this.objectInfoLookup;
      }
    }

    /// <summary>
    /// Sets the callback that is called when a node is clicked.
    /// </summary>
    /// <value>The node click handler.</value>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Since there is no getter, this is raised in error. Provided for possible future use.")]
    internal RoutedEventHandler NodeClickHandler
    {
      set
      {
        this.nodeClickHandler = value;
      }
    }

    /// <summary>
    /// Clears this instance.
    /// </summary>
    internal void Clear()
    {
      this.connections.Clear();
      this.objectInfoLookup.Clear();
    }

    /// <summary>
    /// Gets the diagram node.
    /// </summary>
    /// <param name="objectInfo">The object info.</param>
    /// <returns>The diagram node identified by the given object info.</returns>
    internal DiagramNode GetDiagramNode(ObjectInfo objectInfo)
    {
      if (objectInfo == null)
      {
        return null;
      }

      if (!this.objectInfoLookup.ContainsKey(objectInfo))
      {
        return null;
      }

      return this.objectInfoLookup[objectInfo].Node;
    }

    /// <summary>
    /// Gets the node bounds.
    /// </summary>
    /// <param name="objectInfo">The object info.</param>
    /// <returns>A Rect object that contains the node's bounds.</returns>
    internal Rect GetNodeBounds(ObjectInfo objectInfo)
    {
      Rect bounds = Rect.Empty;
      if (objectInfo != null && this.objectInfoLookup.ContainsKey(objectInfo))
      {
        DiagramConnectorNode connector = this.objectInfoLookup[objectInfo];
        bounds = new Rect(connector.TopLeft.X, connector.TopLeft.Y, connector.Node.ActualWidth, connector.Node.ActualHeight);
      }

      return bounds;
    }

    /// <summary>
    /// Creates the primary row.
    /// </summary>
    /// <returns>The primary row.</returns>
    internal DiagramRow CreateSequencePrimaryRow()
    {
      // The object info nodes are contained in one groups, 
      DiagramGroup primaryGroup = new DiagramGroup();

      // Set up the row.
      DiagramRow primaryRow = new DiagramRow();

      foreach (ObjectInfo objectInfo in this.DiagramModel)
      {
        DiagramNode node = CreateNode(objectInfo);
        primaryGroup.Add(node);
        this.objectInfoLookup.Add(node.ObjectInfo, new DiagramConnectorNode(node, primaryGroup, primaryRow));
      }

      primaryRow.Add(primaryGroup);

      return primaryRow;
    }

    /// <summary>
    /// Creates the message row.
    /// </summary>
    /// <param name="messageInfo">The message info.</param>
    /// <returns>A new message row.</returns>
    internal DiagramRow CreateSequenceMessageRow(MessageInfo messageInfo)
    {
      DiagramGroup group = new DiagramGroup();

      // Set up the row.
      DiagramRow row = new DiagramRow();
      DiagramConnectorNode sourceConnector = null;
      DiagramConnectorNode targetConnector = null;

      foreach (DiagramConnectorNode connectorNode in this.ObjectInfoLookup.Values)
      {
        DiagramNode node = CreateNode(messageInfo);
        if (messageInfo != null &&
          (connectorNode.Node.ObjectInfo == messageInfo.Source || connectorNode.Node.ObjectInfo == messageInfo.Target))
        {
          node.ObjectInfo = connectorNode.Node.ObjectInfo;
          if (connectorNode.Node.ObjectInfo == messageInfo.Source && sourceConnector == null)
          {
            sourceConnector = new DiagramConnectorNode(node, group, row);
          }
          else
          {
            targetConnector = new DiagramConnectorNode(node, group, row);
          }
        }

        group.Add(node);
      }

      if (targetConnector == null)
      {
        targetConnector = sourceConnector;
      }

      if (sourceConnector != null)
      {
        // add the message connection between the two nodes
        this.connections.Add(new DiagramCallConnector(sourceConnector, targetConnector));
      }

      row.Add(group);

      return row;
    }

    /// <summary>
    /// Creates the primary model row.
    /// </summary>
    /// <param name="primaryObjectInfo">The primary object info.</param>
    /// <returns>The created DiagramRow instance.</returns>
    internal DiagramRow CreateClassModelPrimaryRow(ObjectInfo primaryObjectInfo)
    {
      // TODO: find proper algorithm to layout the shapes...
      // The object info nodes are contained in different groups, 
      DiagramGroup primaryGroup = new DiagramGroup();
      DiagramGroup baselessGroup = new DiagramGroup();

      // Set up the row.
      DiagramRow primaryRow = new DiagramRow();

      // do something with objectInfo's group
      ExtendedObjectInfo extendedInfo = primaryObjectInfo as ExtendedObjectInfo;
      if (extendedInfo != null)
      {
        // code is here currently to use property getter;
        if (string.Compare(extendedInfo.Group, "default", StringComparison.OrdinalIgnoreCase) == 0)
        {
          // this is the default group...
        }
        else
        {
          // these are any other groups...
        }

        foreach (ExtendedObjectInfo objectInfo in this.DiagramModel)
        {
          // find base type within current namespace
          ExtendedObjectInfo baseInfo = this.FindBaseTypeInNamespace(objectInfo);
          if (baseInfo != null)
          {
            if (!this.objectInfoLookup.ContainsKey(baseInfo))
            {
              DiagramNode node = CreateNode(baseInfo);

              // code is here currently to use property getter;
              if (node.NodeType == NodeType.TypeInfo || node.NodeType == NodeType.MessageInfo)
              {
                // exit.....
                if (node.Location.X == 0)
                {
                  // exit even faster...
                }
              }

              primaryGroup.Add(node);
              this.objectInfoLookup.Add(node.ObjectInfo, new DiagramConnectorNode(node, primaryGroup, primaryRow));
            }
          }
          else
          {
            if (baselessGroup.Nodes.Count < 10)
            {
              if (!this.objectInfoLookup.ContainsKey(objectInfo))
              {
                DiagramNode node = CreateNode(objectInfo);
                baselessGroup.Add(node);
                this.objectInfoLookup.Add(node.ObjectInfo, new DiagramConnectorNode(node, baselessGroup, primaryRow));
              }
            }
          }
        }
      }

      primaryRow.Add(primaryGroup);
      primaryRow.Add(baselessGroup);

      return primaryRow;
    }

    /// <summary>
    /// Creates the model descendant row.
    /// </summary>
    /// <returns>The newly created DiagramRow.</returns>
    internal DiagramRow CreateClassModelDescendantRow()
    {
      // TODO: Find proper algorithm to layout the shapes...
      // The object info nodes are contained in one groups, 
      DiagramGroup descendantGroup = new DiagramGroup();
      DiagramGroup baselessGroup = new DiagramGroup();

      // Set up the row.
      DiagramRow descendantRow = new DiagramRow();

      foreach (ExtendedObjectInfo objectInfo in this.DiagramModel)
      {
        // find descendants from primaryRow.PrimaryGroup.Nodes
        if (objectInfo.BaseType != null && this.objectInfoLookup.ContainsKey(objectInfo.BaseType) && !this.objectInfoLookup.ContainsKey(objectInfo))
        {
          if (!this.objectInfoLookup.ContainsKey(objectInfo))
          {
            DiagramNode node = CreateNode(objectInfo);
            descendantGroup.Add(node);
            DiagramConnectorNode startConnector = new DiagramConnectorNode(node, descendantGroup, descendantRow);
            this.objectInfoLookup.Add(node.ObjectInfo, startConnector);

            DiagramConnectorNode endConnector = this.objectInfoLookup[objectInfo.BaseType];

            // create inheritance connector
            this.connections.Add(new DiagramInheritanceConnector(startConnector, endConnector));

            // create association connector
            this.connections.Add(new DiagramAssociationConnector(startConnector, endConnector));
          }
        }
        else if (objectInfo.BaseType != null && !this.objectInfoLookup.ContainsKey(objectInfo))
        {
          if (!this.objectInfoLookup.ContainsKey(objectInfo))
          {
            DiagramNode node = CreateNode(objectInfo);
            descendantGroup.Add(node);
            this.objectInfoLookup.Add(node.ObjectInfo, new DiagramConnectorNode(node, descendantGroup, descendantRow));
          }
        }

        // add four more nodes (if available) to baselessGroup
      }

      descendantRow.Add(descendantGroup);
      descendantRow.Add(baselessGroup);

      return descendantRow;
    }

    /// <summary>
    /// Creates the models connections.
    /// </summary>
    internal void CreateClassModelConnections()
    {
      foreach (ObjectRelationInfo relationInfo in this.relationCollection)
      {
        DiagramConnectorNode parent = this.objectInfoLookup.ContainsKey(relationInfo.Parent) ? this.objectInfoLookup[relationInfo.Parent] : null;
        DiagramConnectorNode child = this.objectInfoLookup.ContainsKey(relationInfo.Child) ? this.objectInfoLookup[relationInfo.Child] : null;
        if (parent != null && child != null)
        {
          if (!string.IsNullOrEmpty(relationInfo.RelationName))
          {
            this.connections.Add(new DiagramAssociationConnector(parent, child));
          }
          else
          {
            this.connections.Add(new DiagramInheritanceConnector(parent, child));
          }
        }
      }
    }

    /// <summary>
    /// Create a DiagramNode.
    /// </summary>
    /// <param name="objectInfo">The object info.</param>
    /// <returns>The newly created DiagramNode.</returns>
    private static DiagramNode CreateNode(ObjectInfo objectInfo)
    {
      DiagramNode node = new DiagramSequenceNode();
      node.ObjectInfo = objectInfo;
      node.NodeType = NodeType.TypeInfo;
      return node;
    }

    /// <summary>
    /// Creates the node.
    /// </summary>
    /// <param name="objectInfo">The extended object info.</param>
    /// <returns>A new DiagramNode based on the given objectInfo.</returns>
    private static DiagramNode CreateNode(ExtendedObjectInfo objectInfo)
    {
      DiagramModelNode node = new DiagramModelNode();
      node.ObjectInfo = objectInfo;
      switch (objectInfo.Modifier)
      {
        case TypeModifier.None:
          node.NodeType = NodeType.Class;
          break;
        case TypeModifier.Interface:
          node.NodeType = NodeType.Interface;
          break;
        case TypeModifier.Abstract:
          node.NodeType = NodeType.Abstract;
          break;
        case TypeModifier.Sealed:
          node.NodeType = NodeType.Sealed;
          break;
        case TypeModifier.Static:
          node.NodeType = NodeType.Static;
          break;
        case TypeModifier.Enumeration:
          node.NodeType = NodeType.Enumeration;
          break;
        case TypeModifier.Struct:
          node.NodeType = NodeType.Struct;
          break;
        case TypeModifier.Delegate:
          node.NodeType = NodeType.Delegate;
          break;
        default:
          node.NodeType = NodeType.Class;
          break;
      }

      if (objectInfo.BaseType != null)
      {
        if (string.Compare(objectInfo.Namespace, objectInfo.BaseType.Namespace, StringComparison.Ordinal) != 0)
        {
          node.NodeBaseLabel = objectInfo.BaseType.FullName;
        }
        else
        {
          node.NodeBaseLabel = objectInfo.BaseType.ToString();
        }
        node.MeasureWidth();
      }

      return node;
    }

    /// <summary>
    /// Create a DiagramNode.
    /// </summary>
    /// <param name="messageInfo">The message info.</param>
    /// <returns>The newly created DiagramNode.</returns>
    private static DiagramNode CreateNode(MessageInfo messageInfo)
    {
      DiagramNode node = new DiagramSequenceNode();
      node.MessageInfo = messageInfo;
      node.NodeType = NodeType.MessageInfo;
      return node;
    }

    /// <summary>
    /// Finds the base type in namespace.
    /// </summary>
    /// <param name="objectInfo">The object info.</param>
    /// <returns>The found base type info, null when not found.</returns>
    private ExtendedObjectInfo FindBaseTypeInNamespace(ExtendedObjectInfo objectInfo)
    {
      ExtendedObjectInfo result = null;
      if (objectInfo.BaseType != null && objectInfo.BaseType.Namespace == objectInfo.Namespace)
      {
        ExtendedObjectInfo baseType = this.DiagramModel.Find(objectInfo.BaseType.Key) as ExtendedObjectInfo;
        if (baseType != null)
        {
          result = this.FindBaseTypeInNamespace(baseType);
          if (result == null)
          {
            return baseType;
          }
        }
      }

      return result;
    }
  }
}
