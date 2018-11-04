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
            var body = GenerateMethodBody(node);

            if (body != null)
                node = node.WithBody(body);

            return node;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var body = GenerateMethodBody(node);

            if (body != null)
                node = node.WithBody(body);

            return node;
        }

        public BlockSyntax GenerateMethodBody(BaseMethodDeclarationSyntax node)
        {
            var methodSymbol = _semanticModel.GetDeclaredSymbol(node);

            var pythonMethodAttribute = methodSymbol.GetPythonMethodAttribute();

            if (pythonMethodAttribute != null && pythonMethodAttribute.Generate == true)
            {
                var pythonFile = methodSymbol.LocatePythonFile(_generator.PythonDir);
                var pythonFunction = _pythonGraph.GetFunction(pythonFile, pythonMethodAttribute.Function);
                var body = _generator.Translator.TranslateFunctionBody(pythonFunction);

                return body;
            }

            return null;
        }
    }
}