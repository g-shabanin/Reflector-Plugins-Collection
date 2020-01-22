#define CODE_ANALYSIS
namespace LiveSequence.Common.Context
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;
  using LiveSequence.Common.Domain;

  /// <summary>
  /// Provides a context for the rendering of the DiagramViewer to show a class model diagram for the selected namespace (or assembly ???).
  /// </summary>
  public sealed class ModelContext : ContextBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelContext"/> class.
    /// </summary>
    /// <param name="contextParameters">The context parameters.</param>
    internal ModelContext(ContextParameters contextParameters)
    {
      if (contextParameters as EmptyContextParameters == null)
      {
        throw new ArgumentException(Properties.Resources.ArgumentHasInvalidType, "contextParameters");
      }

      DiagramContext.Clear();
    }

    /// <summary>
    /// Adds the model object.
    /// </summary>
    /// <param name="extendedTypeData">The extended type data.</param>
    public void AddModelObject(ClassTypeInfo extendedTypeData)
    {
      this.AddModelObject(extendedTypeData, "default");
    }

    /// <summary>
    /// Adds the model object.
    /// </summary>
    /// <param name="extendedTypeData">The extended type data.</param>
    /// <param name="classModelGroup">The class model group.</param>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "The method is designed as an instance method to provide means to start with a fresh context.")]
    public void AddModelObject(ClassTypeInfo extendedTypeData, string classModelGroup)
    {
      if (extendedTypeData == null)
      {
        throw new ArgumentNullException("extendedTypeData");
      }

      ExtendedObjectInfo objectInfo = new ExtendedObjectInfo(extendedTypeData, classModelGroup);
      DiagramContext.DiagramObjects.Add(objectInfo);
    }

    /// <summary>
    /// Adds the message.
    /// </summary>
    /// <param name="associationData">The association data.</param>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "The method is designed as an instance method to provide means to start with a fresh context.")]
    public void AddModelRelation(ClassConnectorInfo associationData)
    {
      if (associationData == null)
      {
        throw new ArgumentNullException("associationData");
      }
      else if (associationData.Parent == null || associationData.Child == null)
      {
        throw new ArgumentException(Properties.Resources.ParentOrChildDataNotFound, "associationData");
      }

      ExtendedObjectInfo parent = DiagramContext.DiagramObjects.Find(associationData.Parent.Key) as ExtendedObjectInfo;
      ExtendedObjectInfo child = DiagramContext.DiagramObjects.Find(associationData.Child.Key) as ExtendedObjectInfo;

      if (parent == null || child == null)
      {
        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ParentOrChildObjectInfoNotFound, parent == null ? associationData.Parent.Key : associationData.Child.Key), "associationData");
      }

      ObjectRelationInfo relation = new ObjectRelationInfo(parent, child, associationData.Name);
      DiagramContext.Relations.Add(relation);
    }

    /// <summary>
    /// Renders this instance.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "The method is designed as an instance method to provide means to start with a fresh context.")]
    public void Render()
    {
      // This will tell other views using this data to update themselves using the new data
      DiagramContext.DiagramObjects.OnContentChanged();

      DiagramContext.DiagramObjects.IsDirty = false;

      // set the Current (primary object) to the first in the list...
      DiagramContext.DiagramObjects.Current = DiagramContext.DiagramObjects[0];

      DiagramContext.DiagramObjects.IsDirty = false;
    }

    /// <summary>
    /// Releases the context.
    /// </summary>
    internal override void ReleaseContext()
    {
      // perform clean up of the context
    }
  }
}