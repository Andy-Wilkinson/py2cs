using System.Collections.Generic;
using IronPython.Compiler.Ast;

namespace Py2Cs.CodeGraphs
{
    public class PythonClass : IPythonNode
    {
        private PythonClass(ClassDefinition pythonDefinition)
        {
            this.PythonDefinition = pythonDefinition;
            this.Type = PythonType.FromName(pythonDefinition.Name);

            this.Children = new Dictionary<string, IPythonNode>();
        }

        public string Name => PythonDefinition.Name;
        public ClassDefinition PythonDefinition { get; }
        public PythonType Type { get; }
        public Dictionary<string, IPythonNode> Children { get; }

        public static PythonClass Create(ClassDefinition pythonDefinition)
        {
            var node = new PythonClass(pythonDefinition);
            node.ExtractChildren(pythonDefinition.Body);

            foreach (var child in node.Children.Values)
            {
                if (child is PythonFunction pythonFunction)
                {
                    pythonFunction.Parameters[0].Type = node.Type;
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