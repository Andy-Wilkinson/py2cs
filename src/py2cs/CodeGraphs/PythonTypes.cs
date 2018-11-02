namespace Py2Cs.CodeGraphs
{
    public class PythonTypes
    {
        private static PythonType _unknown = PythonType.FromName("?");
        private static PythonType _none = PythonType.FromName("None");
        private static PythonType _int = PythonType.FromName("int");
        private static PythonType _float = PythonType.FromName("float");
        private static PythonType _bool = PythonType.FromName("bool");
        private static PythonType _str = PythonType.FromName("str");

        public static PythonType Unknown => _unknown;
        public static PythonType None => _none;
        public static PythonType Int => _int;
        public static PythonType Float => _float;
        public static PythonType Bool => _bool;
        public static PythonType Str => _str;
    }
}