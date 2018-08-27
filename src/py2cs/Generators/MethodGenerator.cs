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

        public MethodGeneratorRewriter(Generator generator, SemanticModel semanticModel)
        {
            this._generator = generator;
            this._semanticModel = semanticModel;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var methodSymbol = _semanticModel.GetDeclaredSymbol(node);

            var pythonMethodAttribute = GetPythonMethodAttribute(methodSymbol);

            if (pythonMethodAttribute != null && pythonMethodAttribute.Generate == true)
            {
                var pythonFile = GetPythonFilename(node, pythonMethodAttribute.File);
                var ast = ParsePythonFile(pythonFile);

                var translatedDocument = _generator.Translator.Translate(ast);

                var functionParts = pythonMethodAttribute.Function.Split(".");

                if (functionParts.Length != 2)
                    throw new System.NotImplementedException();

                var className = functionParts[0];
                var functionName = functionParts[1];

                var sourceClass = translatedDocument.DescendantNodes()
                        .OfType<ClassDeclarationSyntax>()
                        .Where(c => c.Identifier.Text == className)
                        .First();

                var sourceMethod = sourceClass.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .Where(m => m.Identifier.Text == functionName)
                        .First();

                node = node.WithBody(sourceMethod.Body);
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

        private static PythonAst ParsePythonFile(string path)
        {
            var pythonEngine = Python.CreateEngine();
            var pythonSource = pythonEngine.CreateScriptSourceFromFile(path);
            var pythonSourceUnit = HostingHelpers.GetSourceUnit(pythonSource);
            var context = new CompilerContext(pythonSourceUnit, pythonEngine.GetCompilerOptions(), ErrorSink.Default);
            var options = new PythonOptions();
            var parser = Parser.CreateParser(context, options);
            return parser.ParseFile(false);
        }
    }
}