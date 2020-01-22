using System.Collections.Generic;
using System.Text;
using LiveSequence.Common;
using LiveSequence.Common.Domain;
using Mono.Cecil;

namespace LiveSequence.Engine
{
  internal static class CecilAssemblyHelper
  {
    internal static NormalizeMethodDefinition CreateNormalizeMethodDefinition(MethodDefinition methodDef)
    {
      return new NormalizeMethodDefinition(
        methodDef.DeclaringType.Name,
        methodDef.DeclaringType.Namespace,
        methodDef.Name,
        BuildParameterList(methodDef.Parameters),
        methodDef.ReturnType.ReturnType.Name,
        methodDef.DeclaringType.Scope.Name);
    }

    internal static NormalizeMethodDefinition CreateNormalizeMethodDefinition(MethodReference methodRef)
    {
      return new NormalizeMethodDefinition(
        methodRef.DeclaringType.Name,
        methodRef.DeclaringType.Namespace,
        methodRef.Name,
        BuildParameterList(methodRef.Parameters),
        methodRef.ReturnType.ReturnType.Name,
        methodRef.DeclaringType.Scope.Name);
    }

    internal static TypeData CreateTypeData(TypeDefinition typeDef, string namespaceText)
    {
      return new TypeData(
        Helper.RemoveAnyExtension(typeDef.Module.Name),
        namespaceText,
        typeDef.Name,
        typeDef.Methods.Count,
        GetTypeFields(typeDef),
        GetTypeProperties(typeDef));
    }


    private static IEnumerable<string> GetTypeFields(TypeDefinition typeDef)
    {
      foreach (FieldDefinition f in typeDef.Fields)
      {
        if (Rules.IsValidAssembly(f.FieldType.FullName))
          yield return f.FieldType.Name + " " + f.Name;
      }
    }

    private static IEnumerable<string> GetTypeProperties(TypeDefinition typeDef)
    {
      foreach (PropertyDefinition pDef in typeDef.Properties)
      {
        if (Rules.IsValidAssembly(pDef.PropertyType.FullName))
          yield return pDef.PropertyType.Name + " " + pDef.Name;
      }
    }

    private static string BuildParameterList(ParameterDefinitionCollection parameterDefinitionCollection)
    {
      var sb = new StringBuilder();
      int paramCount = 1;
      foreach (ParameterDefinition paramDef in parameterDefinitionCollection)
      {
        sb.Append(paramDef.ParameterType.Name);

        if (paramCount < parameterDefinitionCollection.Count)
        {
          sb.Append(",");
          paramCount++;
        }
      }

      return sb.ToString();
    }
  }
}
