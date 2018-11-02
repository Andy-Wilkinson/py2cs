namespace Py2Cs.CodeGraphs
{
    public class PythonType
    {
        private static PythonType _unknown = new PythonType("?");

        private PythonType(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        public static PythonType Unknown
        {
            get
            {
                return _unknown;
            }
        }

        public static PythonType FromName(string name)
        {
            return new PythonType(name);
        }
    }
}