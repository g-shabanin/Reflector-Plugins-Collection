namespace Reflector.Sequence
{
  using System.Collections.Generic;
  using LiveSequence.Common;
  using LiveSequence.Common.Domain;
  using Reflector.CodeModel;

  /// <summary>
  /// Represents the main method data, with the complete call info list.
  /// </summary>
  internal class MethodData
  {
    /// <summary>
    /// Contains a reference to the method cal list with the MethodCallInfo objects.
    /// </summary>
    private List<MethodCallInfo> methodCallList = new List<MethodCallInfo>();

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodData"/> class.
    /// </summary>
    public MethodData()
    {
    }

    /// <summary>
    /// Gets the method call list.
    /// </summary>
    /// <value>The method call list.</value>
    internal List<MethodCallInfo> MethodCallList
    {
      get
      {
        return this.methodCallList;
      }
    }

    /// <summary>
    /// Adds the new call.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="methodDefinition">The method definition.</param>
    /// <param name="callDefinition">The call definition.</param>
    /// <param name="startMethod">The start method.</param>
    /// <returns>The IMethodDeclaration based on the given call definition. This provides a means of working through the method's call stack.</returns>
    internal IMethodDeclaration AddNewCall(string typeName, IMethodDeclaration methodDefinition, object callDefinition, string startMethod)
    {
      Logger.Current.Info(typeName);
      Logger.Current.Info(methodDefinition.Name);
      Logger.Current.Info(startMethod);

      if (!Rules.IsValidCall(callDefinition, new CallRules()))
      {
        return null;
      }

      MethodCallInfo methodInfo = new MethodCallInfo();
      methodInfo.StartMethod = startMethod;
      methodInfo.TypeName = typeName;
      methodInfo.MethodName = ReflectorHelper.CreateNormalizeMethodDefinition(methodDefinition).ToString();

      methodDefinition = callDefinition as IMethodDeclaration;
      IMethodReference methodReference = callDefinition as IMethodReference;

      if (methodDefinition != null || methodReference != null)
      {
        NormalizeMethodDefinition norm = null;
        if (methodDefinition == null)
        {
          norm = ReflectorHelper.CreateNormalizeMethodDefinition(methodReference);
        }
        else
        {
          norm = ReflectorHelper.CreateNormalizeMethodDefinition(methodDefinition);
        }

        methodInfo.MethodCallType = norm.DeclaringTypeFullName;
        methodInfo.MethodCallNamespace = norm.DeclaringTypeNamespace;
        methodInfo.MethodCallName = norm.ToString();

        this.MethodCallList.Add(methodInfo);
      }

      return methodDefinition;
    }
  }
}
