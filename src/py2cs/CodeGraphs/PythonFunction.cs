using System.Collections.Generic;
using System.Linq;
using IronPython.Compiler.Ast;

namespace Py2Cs.CodeGraphs
{
    public class PythonFunction : IPythonNode
    {
        private PythonFunction(string name, PythonType returnType, IList<PythonParameter> parameters)
        {
            this.Name = name;
            this.ReturnType = returnType;
            this.Parameters = parameters;

            this.Children = new Dictionary<string, IPythonNode>();
        }

        public string Name { get; }
        public FunctionDefinition PythonDefinition { get; set; }
        public IList<PythonParameter> Parameters { get; }
        public PythonType ReturnType { get; set; }
        public Dictionary<string, IPythonNode> Children { get; }

        public static PythonFunction Create(FunctionDefinition pythonDefinition)
        {
            var parameters = pythonDefinition.Parameters.Select(p => new PythonParameter(p)).ToList();
            return new PythonFunction(pythonDefinition.Name, PythonTypes.Unknown, parameters) { PythonDefinition = pythonDefinition };
        }

        public static PythonFunction Create(string name, PythonType returnType, IList<PythonParameter> parameters)
        {
            return new PythonFunction(name, returnType, parameters);
        }

        public override string ToString()
        {
            var parameterList = string.Join(", ", Parameters);
            return $"{Name}({parameterList}) : {ReturnType}";
        }
    }
}