namespace Py2Cs.Generators
{
    public struct PythonMethodMapping
    {
        public PythonMethodMapping(string file, string functionName, bool generate)
        {
            this.File = file;
            this.FunctionName = functionName;
            this.Generate = generate;
        }

        public string File { get; }
        public string FunctionName { get; }
        public bool Generate{ get; }
    }
}