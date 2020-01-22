namespace Reflector.Sequence
{
  using System.Collections;
  using System.Reflection;
  using System.Reflection.Emit;
  using LiveSequence.Common;

  /// <summary>
  /// Converts the code to a name.
  /// </summary>
  public sealed class CodeConverter
  {
    /// <summary>
    /// Contains a reference to a Hashtable that contains the conversion info.
    /// </summary>
    private Hashtable opcodeName = new Hashtable();

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeConverter"/> class.
    /// </summary>
    public CodeConverter()
    {
      foreach (FieldInfo fi in typeof(OpCodes).GetFields(
                                                    BindingFlags.Public |
                                                    BindingFlags.Static))
      {
        OpCode code = (OpCode)fi.GetValue(null);
        this.opcodeName[(int)code.Value] = code;
      }
    }

    /// <summary>
    /// Converts the specified code.
    /// </summary>
    /// <param name="code">The code value.</param>
    /// <returns>The name of the specified code value.</returns>
    public string Convert(int code)
    {
      Logger.Current.Info(">>Instruction code to convert:" + code);

      object o = this.opcodeName[code];

      if (o == null)
      {
        return string.Empty;
      }

      OpCode op = (OpCode)o;

      Logger.Current.Info("Converted Instruction:" + op.Name);

      return op.Name;
    }
  }
}
