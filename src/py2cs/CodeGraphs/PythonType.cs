namespace Py2Cs.CodeGraphs
{
    public class PythonType
    {
        private PythonType(string name, PythonClass pythonClass)
        {
            this.Name = name;
            this.Class = pythonClass;
        }

        public string Name { get; }

        public PythonClass Class { get; }

        public override string ToString()
        {
            return Name;
        }

        public static PythonType FromClass(PythonClass pythonClass)
        {
            return new PythonType(pythonClass.Name, pythonClass);
        }

        public static PythonType FromName(string name)
        {
            return new PythonType(name, null);
        }
    }
}