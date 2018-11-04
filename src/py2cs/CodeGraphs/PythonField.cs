using System.Collections.Generic;

namespace Py2Cs.CodeGraphs
{
    public class PythonField : IPythonNode
    {
        private PythonField(string name, PythonType type)
        {
            this.Name = name;
            this.Type = type;

            this.Children = new Dictionary<string, IPythonNode>();
        }

        public string Name { get; }
        public PythonType Type { get; set; }
        public Dictionary<string, IPythonNode> Children { get; }

        public static PythonField Create(string name, PythonType type)
        {
            return new PythonField(name, type);
        }

        public override string ToString()
        {
            return $"{Name} : {Type}";
        }
    }
}