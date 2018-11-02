namespace Py2Cs.Generators
{
    public struct PythonTypeMapping
    {
        public PythonTypeMapping(string file, string className)
        {
            this.File = file;
            this.ClassName = className;
        }

        public string File { get; }

        public string ClassName { get; }
    }
}