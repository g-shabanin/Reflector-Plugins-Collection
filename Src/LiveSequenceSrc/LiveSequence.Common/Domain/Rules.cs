using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSequence.Common.Domain
{
    public static class Rules
    {
        public static bool IsValidMethod(string methodName)
        {
          // check in ignored methods
          var query = from ignoredMethod in Settings.IgnoredMethodList()
                      where methodName.StartsWith(ignoredMethod, StringComparison.OrdinalIgnoreCase)
                      select ignoredMethod;

          return query.Count() == 0;
        }

        public static bool IsValidCall(object insOperand, ICallRules callRules)
        {
          string operandMethodName = string.Empty;
          string declaringTypeName = string.Empty;

          KeyValuePair<string, string> operandMethodAndDeclaringType = callRules.DetermineOperandAndDeclaringType(insOperand);

          operandMethodName = operandMethodAndDeclaringType.Key;
          declaringTypeName = operandMethodAndDeclaringType.Value;

          // must not be true
          if (operandMethodName.Length == 0 || declaringTypeName.Length == 0)
          {
            return false;
          }

          // check if the method we are calling is valid
          if (!IsValidMethod(operandMethodName))
          {
            return false;
          }

          // now check in ignored classes
          var query = from ignoredType in Settings.IgnoredTypeList()
                      where declaringTypeName.StartsWith(ignoredType, StringComparison.OrdinalIgnoreCase)
                      select ignoredType;

          return query.Count() == 0;
        }

        /// <summary>
        /// Check if the callee method and declaring type is valid
        /// </summary>
        /// <param name="insOperand"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static bool IsValidCall(object insOperand, string typeName, ICallRules callRules)
        {
            if (!IsValidType(typeName))
            {
                return false;
            }

            return IsValidCall(insOperand, callRules);
        }

        public static bool IsValidInstruction(string instruction)
        {
            bool isValid = false;

            switch (instruction)
            {
                case "call":
                    isValid = true;
                    break;
                case "callvirt":
                    isValid = true;
                    break;
                default:
                    break;
            }

            return isValid;
        }


        public static bool IsValidAssembly(string assemblyName)
        {
          // check in ignored methods
          var query = from ignoredAssembly in Settings.IgnoredAssemblyList()
                      where assemblyName.StartsWith(ignoredAssembly, StringComparison.OrdinalIgnoreCase)
                      select ignoredAssembly;

          return query.Count() == 0;
        }

        public static bool IsValidType(string typeName)
        {
          // check in ignored methods
          var query = from ignoredType in Settings.IgnoredTypeList()
                      where typeName.StartsWith(ignoredType, StringComparison.OrdinalIgnoreCase)
                      select ignoredType;

          return query.Count() == 0;
        }
    }
}