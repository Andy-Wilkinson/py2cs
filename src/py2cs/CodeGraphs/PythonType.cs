namespace Py2Cs.CodeGraphs
{
    public struct PythonType
    {
        public PythonType(IPythonNode node)
        {
            this.Node = node;
        }

        public IPythonNode Node { get; }

        public override string ToString()
        {
            return Node.ToString();
        }
    }
}