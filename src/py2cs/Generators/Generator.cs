using System.Threading.Tasks;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Py2Cs.Generators
{
    public class Generator
    {
        public async Task<Project> Generate(Project project)
        {
            var compilation = await project.GetCompilationAsync();

            foreach (var documentId in project.DocumentIds)
            {
                var document = project.GetDocument(documentId);
                var documentTree = await document.GetSyntaxTreeAsync();
                var documentRoot = await documentTree.GetRootAsync();

                var model = compilation.GetSemanticModel(documentTree);

                var methodGenerator = new MethodGeneratorRewriter(model);

                documentRoot = methodGenerator.Visit(documentRoot);

                var newDocument = document.WithSyntaxRoot(documentRoot.NormalizeWhitespace());
                project = newDocument.Project;
            }

            return project;
        }
    }
}