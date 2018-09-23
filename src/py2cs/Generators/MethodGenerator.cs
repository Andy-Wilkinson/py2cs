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
using Py2Cs.Translators;
using System.IO;
using System.Linq;

namespace Py2Cs.Generators
{
    public class MethodGeneratorRewriter : CSharpSyntaxRewriter
    {
        private readonly Generator _generator;
        private readonly SemanticModel _semanticModel;
        private readonly PythonCache _pythonCache;

        public MethodGeneratorRewriter(Generator generator, SemanticModel semanticModel, PythonCache pythonCache)
        {
            this._generator = generator;
            this._semanticModel = semanticModel;
            this._pythonCache = pythonCache;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var methodSymbol = _semanticModel.GetDeclaredSymbol(node);

            var pythonMethodAttribute = GetPythonMethodAttribute(methodSymbol);

            if (pythonMethodAttribute != null && pythonMethodAttribute.Generate == true)
            {
                var pythonFile = GetPythonFilename(node, pythonMethodAttribute.File);
                var rootNode = _pythonCache.GetPythonFile(pythonFile);
                var functionNode = rootNode.GetDescendent(pythonMethodAttribute.Function);
                var body = _generator.Translator.TranslateFunctionBody(functionNode);

                node = node.WithBody(body);
            }

            return node;
        }

        private string GetPythonFilename(MethodDeclarationSyntax node, string pythonFile)
        {
            var sourceFile = node.GetLocation().SourceTree.FilePath;
            var sourceLocalFilename = Path.Combine(Path.GetDirectoryName(sourceFile), pythonFile);

            if (File.Exists(sourceLocalFilename))
                return sourceLocalFilename;

            var pythonFolderFilename = Path.Combine(_generator.PythonDir, pythonFile);

            if (File.Exists(pythonFolderFilename))
                return pythonFolderFilename;

            throw new FileNotFoundException();
        }

        private static PythonMethodAttribute GetPythonMethodAttribute(IMethodSymbol methodSymbol)
        {
            foreach (var attribute in methodSymbol.GetAttributes())
            {
                if (attribute.AttributeClass.Name == nameof(PythonMethodAttribute))
                {
                    var function = (string)attribute.ConstructorArguments[0].Value;
                    var file = attribute.GetNamedArgument<string>(nameof(PythonMethodAttribute.File), null);
                    var generate = attribute.GetNamedArgument<bool>(nameof(PythonMethodAttribute.Generate), false);

                    return new PythonMethodAttribute(function) { File = file, Generate = generate };
                }
            }

            return null;
        }
    }
}