using System.Collections.Generic;

namespace LiveSequence.Common.Domain
{
  /// <summary>
  /// Interface to be used to validate the call signature.
  /// </summary>
  public interface ICallRules
  {
    /// <summary>
    /// Determines the type of the operand and declaring.
    /// </summary>
    /// <param name="insOperand">The ins operand.</param>
    /// <returns>A KeyValuePair is being abused as some sort of Tuple to return the operand method name and the declaring type.</returns>
    KeyValuePair<string, string> DetermineOperandAndDeclaringType(object insOperand);
  }
}
