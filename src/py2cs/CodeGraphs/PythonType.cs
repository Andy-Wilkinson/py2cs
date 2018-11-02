namespace Py2Cs.CodeGraphs
{
    public class PythonType
    {
        private PythonType(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        public static PythonType FromName(string name)
        {
            return new PythonType(name);
        }
    }
}