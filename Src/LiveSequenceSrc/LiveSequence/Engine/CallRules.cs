using System.Collections.Generic;
using LiveSequence.Common.Domain;
using Mono.Cecil;

namespace LiveSequence.Engine
{
  public class CallRules : ICallRules
  {
    internal CallRules()
    {
    }

    #region ICallRules Members

    public KeyValuePair<string, string> DetermineOperandAndDeclaringType(object insOperand)
    {
      string operandMethodName = string.Empty;
      string declaringTypeName = string.Empty;

      MethodReference methodReference = insOperand as MethodReference;
      MethodDefinition methodDefinition = insOperand as MethodDefinition;

      if (methodReference != null)
      {
        operandMethodName = methodReference.Name;
        declaringTypeName = methodReference.DeclaringType.FullName;
      }
      else if (methodDefinition != null)
      {
        operandMethodName = methodDefinition.Name;
        declaringTypeName = methodDefinition.DeclaringType.FullName;
      }

      return new KeyValuePair<string, string>(operandMethodName, declaringTypeName);
    }

    #endregion
  }
}
