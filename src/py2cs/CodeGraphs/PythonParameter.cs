using IronPython.Compiler.Ast;

namespace Py2Cs.CodeGraphs
{
    public class PythonParameter
    {
        public PythonParameter(string name, PythonType type)
        {
            this.Name = name;
            this.Type = type;
        }

        public PythonParameter(Parameter parameter)
        {
            this.Parameter = parameter;
            this.Name = parameter.Name;
            this.Type = PythonTypes.Unknown;
        }

        public string Name { get; }
        public Parameter Parameter { get; }
        public PythonType Type { get; set; }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }
}