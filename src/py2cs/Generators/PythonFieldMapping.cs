namespace Py2Cs.Generators
{
    public struct PythonFieldMapping
    {
        public PythonFieldMapping(string file, string fieldName)
        {
            this.File = file;
            this.FieldName = fieldName;
        }

        public string FieldName { get; }
        public string File { get; }
    }
}