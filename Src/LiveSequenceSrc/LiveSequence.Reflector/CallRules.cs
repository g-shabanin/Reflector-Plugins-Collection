namespace Reflector.Sequence
{
  using System.Collections.Generic;
  using LiveSequence.Common.Domain;
  using Reflector.CodeModel;

  /// <summary>
  /// Implements the ICallRules interface
  /// </summary>
  public class CallRules : ICallRules
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CallRules"/> class.
    /// </summary>
    /// <remarks>Prevents the creation of instances outside this assembly.</remarks>
    internal CallRules()
    {
    }

    #region ICallRules Members

    /// <summary>
    /// Determines the type of the operand and declaring.
    /// </summary>
    /// <param name="insOperand">The ins operand.</param>
    /// <returns>
    /// A KeyValuePair is being abused as some sort of Tuple to return the operand method name and the declaring type.
    /// </returns>
    public KeyValuePair<string, string> DetermineOperandAndDeclaringType(object insOperand)
    {
      string operandMethodName = string.Empty;
      string declaringTypeNamespace = string.Empty;
      string declaringTypeName = string.Empty;

      IMethodReference operandMethodReference = insOperand as IMethodReference;
      IMethodDeclaration operandMethodDefinition = insOperand as IMethodDeclaration;
      if (operandMethodReference != null)
      {
        operandMethodName = operandMethodReference.Name;
        ITypeReference typeReference = operandMethodReference.DeclaringType as ITypeReference;
        declaringTypeNamespace = typeReference.Namespace;
        declaringTypeName = typeReference.Name;
      }
      else if (operandMethodDefinition != null)
      {
        operandMethodName = operandMethodDefinition.Name;
        ITypeReference typeReference = operandMethodReference.DeclaringType as ITypeReference;
        declaringTypeNamespace = typeReference.Namespace;
        declaringTypeName = typeReference.Name;
      }

      return new KeyValuePair<string, string>(operandMethodName, declaringTypeName);
    }

    #endregion
  }
}
