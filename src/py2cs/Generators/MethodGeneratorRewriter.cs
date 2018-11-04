using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Py2Cs.CodeGraphs;
using Py2Cs.Translators;
using System.IO;
using System.Linq;

namespace Py2Cs.Generators
{
    public class MethodGeneratorRewriter : CSharpSyntaxRewriter
    {
        private readonly Generator _generator;
        private readonly SemanticModel _semanticModel;
        private readonly PythonGraph _pythonGraph;
        private readonly PythonMappings _pythonMappings;

        public MethodGeneratorRewriter(Generator generator, SemanticModel semanticModel, PythonGraph pythonGraph, PythonMappings pythonMappings)
        {
            this._generator = generator;
            this._semanticModel = semanticModel;
            this._pythonGraph = pythonGraph;
            this._pythonMappings = pythonMappings;
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var methodSymbol = _semanticModel.GetDeclaredSymbol(node);
            var body = GenerateMethodBody(methodSymbol);

            if (body != null)
                node = node.WithBody(body);

            return node;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var methodSymbol = _semanticModel.GetDeclaredSymbol(node);
            var body = GenerateMethodBody(methodSymbol);

            if (body != null)
                node = node.WithBody(body);

            return node;
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            var methodSymbol = _semanticModel.GetDeclaredSymbol(node);
            var body = GenerateMethodBody(methodSymbol);

            if (body != null)
                node = node.WithBody(body).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None));

            return node;
        }

        public BlockSyntax GenerateMethodBody(IMethodSymbol methodSymbol)
        {
            if (_pythonMappings.MethodMappings.TryGetValue(methodSymbol, out PythonMethodMapping methodMapping)
                    && methodMapping.Generate)
            {
                var pythonFunction = _pythonGraph.GetFunction(methodMapping.File, methodMapping.FunctionName);

                var state = TranslatorState.Empty;

                if (!methodSymbol.IsStatic)
                {
                    var selfSyntax = SyntaxFactory.ParseExpression("this");
                    state = state.WithVariable(pythonFunction.Parameters[0].Name, ExpressionResult.Result(selfSyntax, pythonFunction.Parameters[0].Type));
                }

                var body = _generator.Translator.TranslateFunctionBody(pythonFunction, state);

                return body;
            }

            return null;
        }
    }
}