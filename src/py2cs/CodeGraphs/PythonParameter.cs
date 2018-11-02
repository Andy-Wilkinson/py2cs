using IronPython.Compiler.Ast;

namespace Py2Cs.CodeGraphs
{
    public class PythonParameter
    {
        public PythonParameter(Parameter parameter)
        {
            this.Parameter = parameter;
            this.Type = PythonTypes.Unknown;
        }

        public string Name => Parameter.Name;
        public Parameter Parameter { get; }
        public PythonType Type { get; set; }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }
}