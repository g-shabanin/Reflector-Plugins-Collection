using System;
using LiveSequence.Common.Context;
using LiveSequence.Common.Domain;

namespace LiveSequence.Common.Presentation
{
  public class WPFRenderer : IRenderer
  {
    public string Export(SequenceData data)
    {
      RenderDiagram(data);
      return string.Empty;
    }

    public void RenderDiagram(SequenceData data)
    {
      using (SequenceContextScope renderingScope = ContextHelper.CreateSequenceScope())
      {
        // add all the objects to the scope's context.
        foreach (var item in data.GetObjectList())
        {
          renderingScope.CurrentContext.AddSequenceObject(item.Key, item.TypeName);
        }

        // add all the messages to the scope's context.
        foreach (var item in data.GetMethodList())
        {
          MethodCallInfo itemCopy = item;

          string sourceKey = data.GetObjectList().Find(objectInfo => objectInfo.TypeName.Equals(itemCopy.TypeName, StringComparison.Ordinal)).Key;
          string targetKey = data.GetObjectList().Find(objectInfo => objectInfo.TypeName.Equals(itemCopy.MethodCallType, StringComparison.Ordinal)).Key;

          renderingScope.CurrentContext.AddMessage(sourceKey, targetKey, item.MethodCallName, item);
        }

        // render the diagram.
        renderingScope.CurrentContext.Render();
      }
    }
  }
}