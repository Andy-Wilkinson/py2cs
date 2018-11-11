using System.Collections.Generic;
using IronPython.Compiler.Ast;

namespace Py2Cs.CodeGraphs
{
    public class PythonClass : IPythonNode
    {
        private PythonClass(string name, ClassDefinition pythonDefinition)
        {
            this.Name = name;
            this.PythonDefinition = pythonDefinition;

            this.Children = new Dictionary<string, IPythonNode>();
        }

        public string Name { get; }
        public ClassDefinition PythonDefinition { get; }
        public Dictionary<string, IPythonNode> Children { get; }

        public static PythonClass Create(string name)
        {
            return new PythonClass(name, null);
        }

        public static PythonClass Create(ClassDefinition pythonDefinition)
        {
            var node = new PythonClass(pythonDefinition.Name, pythonDefinition);
            var nodeType = new PythonType(node);
            node.ExtractChildren(pythonDefinition.Body);

            foreach (var child in node.Children.Values)
            {
                if (child is PythonFunction pythonFunction)
                {
                    pythonFunction.Parameters[0].Type = nodeType;
                }
            }

            return node;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}