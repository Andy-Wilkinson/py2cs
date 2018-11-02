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
    }
}