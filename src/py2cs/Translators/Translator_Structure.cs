using System;
using System.Collections.Immutable;
using IronPython.Compiler.Ast;

namespace Py2Cs.Translators
{
    public partial class Translator
    {
        public PythonNode Extract(PythonAst ast)
        {
            var rootNode = PythonNode.CreateRoot();
            var state = TranslatorState.Empty;

            (rootNode, _) = Extract_Children(rootNode, ast.Body, state);

            return rootNode;
        }

        private (PythonNode, TranslatorState) Extract_Children(PythonNode node, Statement statement, TranslatorState state)
        {
            if (statement is SuiteStatement suiteStatement)
            {
                foreach (Statement memberStatement in suiteStatement.Statements)
                {
                    switch (memberStatement)
                    {
                        // case FromImportStatement fromImportStatement:
                        //     state = ExtractStatement_FromImportStatement(fromImportStatement, state);
                        //     break;
                        case ClassDefinition classDefinition:
                            var pythonClass = ExtractStatement_Class(classDefinition, state);
                            node = node.WithChild(pythonClass);
                            break;
                        case FunctionDefinition functionDefinition:
                            var pythonFunction = ExtractStatement_Function(functionDefinition, state);
                            node = node.WithChild(pythonFunction);
                            break;

                    }
                }

                return (node, state);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        // private TranslatorState ExtractStatement_FromImportStatement(FromImportStatement fromImportStatement, TranslatorState state)
        // {
        //     for (int nameIndex = 0; nameIndex < fromImportStatement.Names.Count; nameIndex++)
        //     {
        //         var name = fromImportStatement.Names[nameIndex];
        //         var asName = fromImportStatement.AsNames[nameIndex] ?? name;

        //         state = state.WithVariable(asName, name);
        //     }

        //     return state;
        // }

        private PythonNode ExtractStatement_Class(ClassDefinition definition, TranslatorState state)
        {
            var node = PythonNode.CreateClass(definition, state);
            (node, _) = Extract_Children(node, definition.Body, state);
            return node;
        }

        private PythonNode ExtractStatement_Function(FunctionDefinition definition, TranslatorState state)
        {
            var node = PythonNode.CreateFunction(definition, state);
            (node, _) = Extract_Children(node, definition.Body, state);
            return node;
        }
    }
}