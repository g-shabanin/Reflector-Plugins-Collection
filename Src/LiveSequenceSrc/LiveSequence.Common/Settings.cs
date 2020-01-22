using System;
using System.Collections.Generic;
using System.Configuration;

namespace LiveSequence.Common
{
    public static class Settings
    {
        private static bool includeAssemblyReferences = Convert.ToBoolean(ConfigurationManager.AppSettings["IncludeAssemblyReferences"]);

        private static string outputType = ConfigurationManager.AppSettings["OutputType"];

        public static bool IncludeAssemblyReferences
        {
            get
            {
                return includeAssemblyReferences;
            }

            set
            {
                includeAssemblyReferences = value;
            }
        }

        public static string OutputType
        {
            get 
            {
                if (string.IsNullOrEmpty(outputType))
                {
                    return ConfigurationManager.AppSettings["OutputType"];
                }

                return outputType;
            }

            set
            {
                outputType = value;
            }
        }

        public static string Pic2PlotPath()
        {
            return ConfigurationManager.AppSettings["Pic2PlotPath"];
        }

        public static string [] IgnoredMethodList()
        {
            return ConfigurationManager.AppSettings["IgnoredMethodList"].Split('|');
        }

        public static string[] IgnoredTypeList()
        {
            return ConfigurationManager.AppSettings["IgnoredTypeList"].Split('|');
        }

        internal static IEnumerable<string> IgnoredAssemblyList()
        {
            return ConfigurationManager.AppSettings["IgnoredAssemblyList"].Split('|');
        }
    }
}