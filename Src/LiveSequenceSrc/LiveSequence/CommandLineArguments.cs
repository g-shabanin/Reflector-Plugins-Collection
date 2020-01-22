using System;
using System.Collections.Generic;
using LiveSequence.Common;

namespace LiveSequence
{
  internal sealed class CommandLineArguments
  {
    private CommandLineArguments(string outputType, string assemblyName, string typeName, string destinationPath, bool includeReferenceAssemblies)
    {
      this.OutputType = outputType;
      this.AssemblyName = assemblyName;
      this.TypeName = typeName;
      this.DestinationPath = destinationPath;
      this.IncludeReferenceAssemblies = includeReferenceAssemblies;
    }

    public string OutputType { get; private set; }

    public string AssemblyName { get; private set; }

    public string TypeName { get; private set; }

    public string DestinationPath { get; private set; }

    public bool IncludeReferenceAssemblies { get; private set; }

    internal static bool ConsoleOnly { get; private set; }

    internal static CommandLineArguments ParseArguments(string[] args)
    {
      if (args == null)
      {
        throw new ArgumentNullException("args");
      }

      ConsoleOnly = false;
      Dictionary<string, string> argumentList = new Dictionary<string, string>();
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i].StartsWith("-", StringComparison.Ordinal))
        {
          // No next argument? Or is next argument an option?
          if (((i + 1) >= args.Length) || args[i + 1].StartsWith("-", StringComparison.Ordinal))
          {
            // No value given for this argument, use empty string as value.
            argumentList.Add(args[i].Substring(1), string.Empty);
          }
          else
          {
            // Next argument is the value of the parameter.
            argumentList.Add(args[i].Substring(1), args[i + 1]);
          }
        }
      }

      return CreateInstance(argumentList);
    }

    private static CommandLineArguments CreateInstance(Dictionary<string, string> argumentList)
    {
      string outputType;
      string assemblyName;
      string typeName;
      string destinationPath;
      bool includeReferences;

      if (!argumentList.ContainsKey("s"))
      {
        // Commandline possibly contains arguments for GUI (no use for them yet....)
        return null;
      }

      outputType = argumentList.ContainsKey("o") ? argumentList["o"] : string.Empty;
      assemblyName = argumentList.ContainsKey("a") ? argumentList["a"] : string.Empty;
      typeName = argumentList.ContainsKey("t") ? argumentList["t"] : string.Empty;
      destinationPath = argumentList.ContainsKey("d") ? argumentList["d"] : string.Empty;
      includeReferences = argumentList.ContainsKey("r") ? true : false;

      if (string.IsNullOrEmpty(assemblyName))
      {
        Logger.Current.Debug("Assembly name is null or empty.");
        return null;
      }

      ConsoleOnly = true;
      return new CommandLineArguments(outputType, assemblyName, typeName, destinationPath, includeReferences);
    }
  }
}
