using System.Collections.Generic;
using System.Linq;
using IronPython.Compiler.Ast;

namespace Py2Cs.CodeGraphs
{
    public class PythonFunction : IPythonNode
    {
        private PythonFunction(FunctionDefinition pythonDefinition, IList<PythonParameter> parameters)
        {
            this.PythonDefinition = pythonDefinition;
            this.Parameters = parameters;

            this.Children = new Dictionary<string, IPythonNode>();
        }

        public string Name => PythonDefinition.Name;
        public FunctionDefinition PythonDefinition { get; }
        public IList<PythonParameter> Parameters { get; }
        public Dictionary<string, IPythonNode> Children { get; }

        public static PythonFunction Create(FunctionDefinition pythonDefinition)
        {
            var parameters = pythonDefinition.Parameters.Select(p => new PythonParameter(p)).ToList();
            return new PythonFunction(pythonDefinition, parameters);
        }

        public override string ToString()
        {
            var parameterList = string.Join(", ", Parameters);
            return $"{Name}({parameterList})";
        }
    }
}