using System;
using System.Collections.Generic;

namespace LiveSequence.Common.Domain
{
    public class SequenceData
    {
        private readonly List<MethodCallInfo> methodList = new List<MethodCallInfo>();
        private readonly List<ObjectInstanceInfo> objectList = new List<ObjectInstanceInfo>();

        public SequenceData(string mainTypeName)
        {
            MainTypeName = mainTypeName.Substring(0, mainTypeName.IndexOf(')') + 1);
        }

        public string MainTypeName { get; set; }

        // initial type
        // object list
        // method calls
        public void AddObject(string typeName)
        {
            // dont add object if already exists
            bool objectExists = this.objectList.Exists(objectInfo => objectInfo.TypeName.Equals(typeName));

            if (!objectExists)
            {
                string key = this.ConstructUniqueKey(typeName);

                this.objectList.Add(new ObjectInstanceInfo(typeName, key.ToUpperInvariant()));
            }
        }

        public void AddMessage(MethodCallInfo methodCall)
        {
            methodList.Add(methodCall);
        }

        public override string ToString()
        {
            return MainTypeName;
        }

        internal List<ObjectInstanceInfo> GetObjectList()
        {
          return objectList;
        }

        internal List<MethodCallInfo> GetMethodList()
        {
          return methodList;
        }

        private static string AddUniqueIndexToKey(string typeName, string currentKey)
        {
          string key = typeName;

          if (currentKey.Length == typeName.Length)
          {
            // first pass, just add a 01 to the key
            key = currentKey + "01";
          }
          else
          {
            // get index part of the key
            int currentIndex = Convert.ToInt32(currentKey.Substring(typeName.Length, 2));
            currentIndex++;
            if (currentIndex >= 100)
            {
              throw new ArgumentOutOfRangeException(typeName, "Unable to find a suitable key for typename " + typeName);
            }

            key = typeName + currentIndex.ToString().PadLeft(2, '0');
          }

          return key;
        }

        private string ConstructUniqueKey(string typeName)
        {
          string key = typeName.Substring(0, 1);

          bool keyExists = this.objectList.Exists(objectInfo => objectInfo.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

          int len = 1;
          while (keyExists)
          {
            if (len == typeName.Length)
            {
              key = AddUniqueIndexToKey(typeName, key);
            }
            else
            {
              key = typeName.Substring(0, ++len);
            }

            keyExists = this.objectList.Exists(objectInfo => objectInfo.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
          }

          return key;
        }
    }
}