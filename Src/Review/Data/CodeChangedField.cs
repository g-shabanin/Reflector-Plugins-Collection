using System;

namespace Reflector.Review.Data
{
    [Serializable]
    internal sealed class CodeChangedField
    {
        private readonly string name;
        public string Name
        {
            get { return this.name; }
        }

        private readonly string value;
        public string Value
        {
            get { return this.value; }
        }

        public CodeChangedField(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            return String.Format("{0} = '{1}'", this.Name, this.Value);
        }
    }
}
