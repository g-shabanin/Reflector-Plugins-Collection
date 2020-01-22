namespace LiveSequence.Common.Presentation
{
  using System;
  using System.IO;
  using System.Windows;
  using LiveSequence.Common;
  using LiveSequence.Common.Context;
  using LiveSequence.Common.Domain;
  using LiveSequence.Common.Graphics;

  public sealed class PngRenderer : IRenderer
  {
    public PngRenderer(bool useUniqueFileName, string destinationPath)
    {
      this.UseUniqueFileName = useUniqueFileName;
      this.DestinationPath = destinationPath;
    }

    public bool UseUniqueFileName { get; private set; }

    public string DestinationPath { get; private set; }

    #region IRenderEngine Members

    public string Export(SequenceData data)
    {
      DiagramViewer viewer = new DiagramViewer();
      Size size = new Size(2000D, 2000D);
      viewer.Measure(size);
      viewer.Arrange(new Rect(size));

      string outputFileName = data.ToString();
      outputFileName = Helper.RemoveInvalidCharsFromFileName(outputFileName);
      string extension = "png";
      if (!this.UseUniqueFileName)
      {
        outputFileName = "seq";
      }

      outputFileName = Path.Combine(this.DestinationPath, string.Concat(outputFileName, ".", extension));
      using (SaveContextScope saveScope = ContextHelper.CreateSaveScope(viewer, outputFileName))
      {
        using (SequenceContextScope renderScope = ContextHelper.CreateSequenceScope())
        {
          foreach (ObjectInstanceInfo item in data.GetObjectList())
          {
            renderScope.CurrentContext.AddSequenceObject(item.Key, item.TypeName);
          }

          foreach (MethodCallInfo item in data.GetMethodList())
          {
            string sourceKey = data.GetObjectList().Find(objectInfo => objectInfo.TypeName.Equals(item.TypeName, StringComparison.Ordinal)).Key;
            string targetKey = data.GetObjectList().Find(objectInfo => objectInfo.TypeName.Equals(item.MethodCallType, StringComparison.Ordinal)).Key;

            renderScope.CurrentContext.AddMessage(sourceKey, targetKey, item.MethodCallName, item);
          }

          renderScope.CurrentContext.Render();
        }
      }

      return outputFileName;
    }

    #endregion
  }
}
