using System.Collections.Generic;

namespace Py2Cs.CodeGraphs
{
    public class PythonProperty : IPythonNode
    {
        private PythonProperty(string name, PythonType type)
        {
            this.Name = name;
            this.Type = type;

            this.Children = new Dictionary<string, IPythonNode>();
        }

        public string Name { get; }
        public PythonFunction GetterFunction
        {
            get => Children["get"] as PythonFunction;
            set => Children["get"] = value;
        }
        public PythonFunction SetterFunction
        {
            get => Children["set"] as PythonFunction;
            set => Children["set"] = value;
        }
        public PythonType Type { get; set; }
        public Dictionary<string, IPythonNode> Children { get; }

        public static PythonProperty Create(string name, PythonType type)
        {
            return new PythonProperty(name, type);
        }

        public override string ToString()
        {
            return $"{Name} : {Type}";
        }
    }
}