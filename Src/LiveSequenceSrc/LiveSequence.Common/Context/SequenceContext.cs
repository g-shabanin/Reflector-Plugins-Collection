#define CODE_ANALYSIS
namespace LiveSequence.Common.Context
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;
  using LiveSequence.Common.Domain;

  /// <summary>
  /// Provides a context for the rendering of the DiagramViewer to show a sequence for the selected method.
  /// </summary>
  public sealed class SequenceContext : ContextBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceContext"/> class.
    /// </summary>
    /// <param name="contextParameters">The context parameters.</param>
    internal SequenceContext(ContextParameters contextParameters)
    {
      if (contextParameters as EmptyContextParameters == null)
      {
        throw new ArgumentException(Properties.Resources.ArgumentHasInvalidType, "contextParameters");
      }

      // clear DiagramContext
      DiagramContext.Clear();
    }

    /// <summary>
    /// Adds the sequence object.
    /// </summary>
    /// <param name="key">The key of the type.</param>
    /// <param name="typeName">Name of the type.</param>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "The method is designed as an instance method to provide means to start with a fresh context.")]
    public void AddSequenceObject(string key, string typeName)
    {
      if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(typeName))
      {
        throw new ArgumentNullException(string.IsNullOrEmpty(key) ? "key" : "typeName");
      }

      ObjectInfo objectInfo = new ObjectInfo(key, typeName);
      DiagramContext.DiagramObjects.Add(objectInfo);
    }

    /// <summary>
    /// Adds the message.
    /// </summary>
    /// <param name="sourceKey">The source key.</param>
    /// <param name="targetKey">The target key.</param>
    /// <param name="methodCallName">Name of the method call.</param>
    /// <param name="methodCallInfo">The method call info.</param>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "The method is designed as an instance method to provide means to start with a fresh context.")]
    public void AddMessage(string sourceKey, string targetKey, string methodCallName, MethodCallInfo methodCallInfo)
    {
      if (string.IsNullOrEmpty(sourceKey) || string.IsNullOrEmpty(targetKey) || string.IsNullOrEmpty(methodCallName))
      {
        throw new ArgumentNullException(string.IsNullOrEmpty(sourceKey) ? "sourceKey" : string.IsNullOrEmpty(targetKey) ? "targetKey" : "typeName");
      }

      ObjectInfo sourceInfo = DiagramContext.DiagramObjects.Find(sourceKey);
      ObjectInfo targetInfo = DiagramContext.DiagramObjects.Find(targetKey);

      if (sourceInfo == null || targetInfo == null)
      {
        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.SourceOrTargetObjectInfoNotFound, sourceInfo == null ? sourceKey : targetKey), sourceInfo == null ? "sourceKey" : "targetKey");
      }

      MessageInfo message = new MessageInfo(sourceInfo, targetInfo, methodCallName, methodCallInfo);
      DiagramContext.Messages.Add(message);
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
