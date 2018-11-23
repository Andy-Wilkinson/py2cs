using System;
using System.Collections.Generic;
using IronPython.Compiler.Ast;

namespace Py2Cs.CodeGraphs
{
    public static class PythonNodeHelper
    {
        public static void ExtractChildren(this IPythonNode node, Statement statement)
        {
            if (statement is SuiteStatement suiteStatement)
            {
                foreach (Statement memberStatement in suiteStatement.Statements)
                {
                    switch (memberStatement)
                    {
                        case ClassDefinition classDefinition:
                            var pythonClass = PythonClass.Create(classDefinition);
                            node.Children[pythonClass.Name] = pythonClass;
                            break;
                        case FunctionDefinition functionDefinition:
                            var pythonFunction = PythonFunction.Create(functionDefinition);
                            node.Children[pythonFunction.Name] = pythonFunction;
                            break;

                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static IPythonNode GetDescendent(this IPythonNode node, string path)
        {
            var pathParts = path.Split(".");

            foreach (string pathPart in pathParts)
            {
                if (!node.Children.TryGetValue(pathPart, out node))
                    return null;
            }

            return node;
        }

        public static PythonClass GetOrAddClass(this IPythonNode node, string className)
        {
            var pathParts = className.Split(".");

            foreach (string pathPart in pathParts)
            {
                if (!node.Children.TryGetValue(pathPart, out var childNode))
                {
                    childNode = PythonClass.Create(pathPart);
                    node.Children[pathPart] = childNode;
                }

                node = childNode;
            }

            return (PythonClass)node;
        }
    }
}