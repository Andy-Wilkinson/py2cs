namespace Py2Cs.Generators
{
    public struct PythonMethodMapping
    {
        public PythonMethodMapping(string file, string functionName)
        {
            this.File = file;
            this.FunctionName = functionName;
        }

        public string File { get; }
        public string FunctionName { get; }
    }
}