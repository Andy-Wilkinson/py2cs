using System.Collections.Generic;

namespace Py2Cs.CodeGraphs
{
    public class PythonTypes
    {
        private static PythonType _unknown = PythonSpecialType.Create("?");
        private static PythonType _none = PythonSpecialType.Create("None");
        private static PythonType _int = PythonSpecialType.Create("int");
        private static PythonType _float = PythonSpecialType.Create("float");
        private static PythonType _bool = PythonSpecialType.Create("bool");
        private static PythonType _str = PythonSpecialType.Create("str");

        public static PythonType Unknown => _unknown;
        public static PythonType None => _none;
        public static PythonType Int => _int;
        public static PythonType Float => _float;
        public static PythonType Bool => _bool;
        public static PythonType Str => _str;

        private class PythonSpecialType : IPythonNode
        {
            private string _name;

            public PythonSpecialType(string name)
            {
                this._name = name;
                this.Children = new Dictionary<string, IPythonNode>();
            }

            public Dictionary<string, IPythonNode> Children { get; }

            public override string ToString()
            {
                return _name;
            }

            public static PythonType Create(string name)
            {
                var node = new PythonSpecialType(name);
                return new PythonType(node);
            }
        }
    }
}