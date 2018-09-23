using System.Collections.Immutable;
using System.Linq;
using IronPython.Compiler.Ast;

namespace Py2Cs.Translators
{
    public struct PythonNode
    {
        private PythonNode(PythonNodeType nodeType, Statement statement, string name, TranslatorState state, ImmutableList<PythonNode> children)
        {
            this.NodeType = nodeType;
            this.Statement = statement;
            this.Name = name;
            this.State = state;
            this.Children = children;
        }

        public PythonNodeType NodeType { get; }
        public Statement Statement { get; }
        public string Name { get; }
        public TranslatorState State { get; }
        public ImmutableList<PythonNode> Children { get; }

        public PythonNode GetDescendent(string path)
        {
            var pathParts = path.Split(".");
            var node = this;

            foreach (string pathPart in pathParts)
            {
                node = node.Children.FirstOrDefault(n => n.Name == pathPart);
            }

            return node;
        }

        public static PythonNode CreateRoot()
        {
            return new PythonNode(PythonNodeType.Root, null, "", null, ImmutableList<PythonNode>.Empty);
        }

        public static PythonNode CreateClass(ClassDefinition definition, TranslatorState state)
        {
            return new PythonNode(PythonNodeType.Class, definition, definition.Name, state, ImmutableList<PythonNode>.Empty);
        }

        public static PythonNode CreateFunction(FunctionDefinition definition, TranslatorState state)
        {
            return new PythonNode(PythonNodeType.Function, definition, definition.Name, state, ImmutableList<PythonNode>.Empty);
        }

        public PythonNode WithChild(PythonNode child)
        {
            var children = Children.Add(child);
            return new PythonNode(NodeType, Statement, Name, State, children);
        }
    }
}