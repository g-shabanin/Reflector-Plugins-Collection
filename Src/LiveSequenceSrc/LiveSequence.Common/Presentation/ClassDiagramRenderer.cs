namespace LiveSequence.Common.Presentation
{
  using System.Collections.Generic;
  using LiveSequence.Common.Context;
  using LiveSequence.Common.Domain;

  public class ClassDiagramRenderer
  {
    public void Export(ClassModelData data)
    {
      List<ClassModelData> modelData = new List<ClassModelData>();
      modelData.Add(data);
      this.RenderDiagram(modelData);
    }

    public void Export(IList<ClassModelData> data)
    {
      this.RenderDiagram(data);
    }

    private void RenderDiagram(IList<ClassModelData> modelData)
    {
      using (ModelContextScope renderingScope = ContextHelper.CreateModelScope())
      {
        foreach (ClassModelData data in modelData)
        {
          foreach (var item in data.ObjectList)
          {
            Logger.Current.Info("Item added to model: " + item.ToString());
            renderingScope.CurrentContext.AddModelObject(item, item.StartTypeName);
          }

          foreach (var item in data.ConnectorList)
          {
            renderingScope.CurrentContext.AddModelRelation(item);
          }
        }

        renderingScope.CurrentContext.Render();
      }
    }
  }
}
