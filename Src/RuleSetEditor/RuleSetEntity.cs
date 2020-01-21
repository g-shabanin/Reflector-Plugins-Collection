using System;
using System.Collections.Generic;
using System.Text;
using System.Workflow.Activities.Rules;

namespace Reflector.RuleSetEditor
{
    internal class RuleSetEntity
    {
        public RuleSet RuleSet
        {
            get;
            set;
        }

        public string AssemblyQualifiedName
        {
            get;
            set;
        }

        public string FullName
        {
            get;
            set;
        }

        public string CodeBase
        {
            get;
            set;
        }

        public Type Type
        {
            get;
            set;
        }
    }
}
