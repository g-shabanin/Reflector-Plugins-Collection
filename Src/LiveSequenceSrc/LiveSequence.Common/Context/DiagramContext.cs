namespace LiveSequence.Common.Context
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using LiveSequence.Common.Domain;

  /// <summary>
  /// This is a container class to hold some static references that support the DiagramViewer and DiagramController classes.
  /// </summary>
  public static class DiagramContext
  {
    /// <summary>
    /// Contains a static reference to a list of already processed MethodCallInfo objects.
    /// </summary>
    private static List<MessageInfo> processedCalls = new List<MessageInfo>();

    /// <summary>
    /// Contains a static reference to the collection of ObjectInfo objects.
    /// </summary>
    private static ObjectInfoCollection objectInfoCollection = new ObjectInfoCollection();

    /// <summary>
    /// Contains a static reference to the collection of MessageInfo objects.
    /// </summary>
    private static MessageCollection messageCollection = new MessageCollection();

    /// <summary>
    /// Contains a static reference to the collection of ObjectRelationInfo objects.
    /// </summary>
    private static RelationCollection relationCollection = new RelationCollection();

    /// <summary>
    /// Gets the diagram objects.
    /// </summary>
    /// <value>The diagram objects.</value>
    internal static ObjectInfoCollection DiagramObjects
    {
      get
      {
        return objectInfoCollection;
      }
    }

    /// <summary>
    /// Gets the messages.
    /// </summary>
    /// <value>The messages.</value>
    internal static MessageCollection Messages
    {
      get
      {
        return messageCollection;
      }
    }

    /// <summary>
    /// Gets the relations.
    /// </summary>
    /// <value>The relations.</value>
    internal static RelationCollection Relations
    {
      get
      {
        return relationCollection;
      }
    }

    /// <summary>
    /// Clears the collections in this context.
    /// </summary>
    internal static void Clear()
    {
      objectInfoCollection.Clear();
      messageCollection.Clear();
      relationCollection.Clear();
    }

    /// <summary>
    /// Filters the out.
    /// </summary>
    /// <param name="info">The object info.</param>
    internal static void FilterOut(ObjectInfo info)
    {
      if (info == null)
      {
        throw new ArgumentNullException("info");
      }

      objectInfoCollection.Remove(info);
      FilterOut(info, true);
    }

    /// <summary>
    /// Filters the out.
    /// </summary>
    /// <param name="info">The object info.</param>
    /// <param name="includeTargetCalls">if set to <c>true</c> include target calls.</param>
    internal static void FilterOut(ObjectInfo info, bool includeTargetCalls)
    {
      // find all Messages for this info...
      var query = from m in Messages
                  where m.Source == info || (includeTargetCalls && m.Target == info)
                  select m;
      List<MessageInfo> removals = query.ToList();

      foreach (var message in removals)
      {
        Messages.Remove(message);
      }
    }

    /// <summary>
    /// Processes the parents.
    /// </summary>
    /// <param name="info">The message info.</param>
    internal static void ProcessParents(MessageInfo info)
    {
      MessageInfo mi = Messages[0];
      mi.Parent = null;
      if (info.ParentIsSet)
      {
        return;
      }

      mi.NestingLevel = 0;
      for (int i = 1; i < Messages.Count; i++)
      {
        MessageInfo item = Messages[i];

        if (string.Compare(mi.MethodCallInfo.MethodName, item.MethodCallInfo.MethodName, StringComparison.OrdinalIgnoreCase) == 0)
        {
          item.Parent = null;
        }
        else if (string.Compare(mi.MethodCallInfo.MethodCallName, item.MethodCallInfo.MethodName, StringComparison.OrdinalIgnoreCase) == 0)
        {
          item.Parent = mi;
        }
        else
        {
          var query = from m in Messages
                      where m.ParentIsSet &&
                            string.Compare(m.MethodCallInfo.MethodCallName, item.MethodCallInfo.MethodName, StringComparison.OrdinalIgnoreCase) == 0 &&
                            string.Compare(m.MethodCallInfo.MethodCallName, m.MethodCallInfo.MethodName, StringComparison.OrdinalIgnoreCase) != 0
                      select m;

          List<MessageInfo> queryList = query.ToList();
          if (queryList.Count > 0)
          {
            item.Parent = queryList[queryList.Count - 1];
          }
        }

        if (item.Parent != null && string.Compare(item.Parent.MethodCallInfo.MethodCallType, item.Parent.MethodCallInfo.TypeName, StringComparison.Ordinal) == 0)
        {
          item.NestingLevel = item.Parent.NestingLevel + 1;
        }
        else
        {
          item.NestingLevel = 0;
        }

        Logger.Current.Debug(string.Format("item: {0} - {2}; parent: {1} - {3};", item, item.Parent, item.NestingLevel, (item.Parent != null ? item.Parent.NestingLevel : 0)));
      }
    }

    /// <summary>
    /// Determines the nested offset.
    /// </summary>
    /// <param name="item">The message info item.</param>
    /// <returns>The nesting level of the specified item.</returns>
    internal static int DetermineNestedOffset(MessageInfo item)
    {
      ProcessParents(item);

      return item.NestingLevel;
    }

    /// <summary>
    /// Determines the total count of nested calls.
    /// </summary>
    /// <param name="item">The message info item.</param>
    /// <returns>The number of nested calls.</returns>
    internal static int NestedCallCount(MessageInfo item)
    {
      processedCalls.Clear();
      ProcessParents(item);
      List<MessageInfo> messages = new List<MessageInfo>(Messages);
      List<MessageInfo> nestedMessages = FindAllNestedCalls(messages, item, 0);

      return nestedMessages.Count;
    }

    /// <summary>
    /// Finds all nested calls.
    /// </summary>
    /// <param name="infoList">The info list.</param>
    /// <param name="item">The message info item.</param>
    /// <param name="level">The current level.</param>
    /// <returns>A list of MessageInfo objects found to be nested calls for the specified item.</returns>
    private static List<MessageInfo> FindAllNestedCalls(List<MessageInfo> infoList, MessageInfo item, int level)
    {
      if (processedCalls.Contains(item))
      {
        return new List<MessageInfo>();
      }

      List<MessageInfo> result = new List<MessageInfo>();

      var query = from m in infoList
                  where m.Parent == item
                  select m;

      result.AddRange(query);
      processedCalls.Add(item);

      foreach (MessageInfo resultItem in result.ToArray())
      {
        result.AddRange(FindAllNestedCalls(infoList, resultItem, level + 1));
      }

      return result;
    }
  }
}
