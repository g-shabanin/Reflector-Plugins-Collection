namespace LiveSequence.Common.Domain
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Container class for the object data that is to be used for the class diagram.
  /// </summary>
  public sealed class ClassModelData
  {
    /// <summary>
    /// Contains a reference to the type objects that are part of the current model.
    /// </summary>
    private List<ClassTypeInfo> objectList;

    /// <summary>
    /// Contains a reference to the connectors that are part of the current model.
    /// </summary>
    private List<ClassConnectorInfo> connectorList;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModelData"/> class.
    /// </summary>
    public ClassModelData()
    {
      this.objectList = new List<ClassTypeInfo>();
      this.connectorList = new List<ClassConnectorInfo>();
    }

    /// <summary>
    /// Gets the object list.
    /// </summary>
    /// <value>The object list.</value>
    internal List<ClassTypeInfo> ObjectList
    {
      get
      {
        return this.objectList;
      }
    }

    /// <summary>
    /// Gets the connector list.
    /// </summary>
    /// <value>The connector list.</value>
    internal List<ClassConnectorInfo> ConnectorList
    {
      get
      {
        return this.connectorList;
      }
    }

    /// <summary>
    /// Adds the object.
    /// </summary>
    /// <param name="objectInfo">The object info.</param>
    public void AddObject(ClassTypeInfo objectInfo)
    {
      if (objectInfo == null)
      {
        throw new ArgumentNullException("objectInfo");
      }

      this.objectList.Add(objectInfo);
    }

    /// <summary>
    /// Adds the connector.
    /// </summary>
    /// <param name="connectorInfo">The connector info.</param>
    public void AddConnector(ClassConnectorInfo connectorInfo)
    {
      if (connectorInfo == null)
      {
        throw new ArgumentNullException("connectorInfo");
      }

      this.connectorList.Add(connectorInfo);
    }
  }
}