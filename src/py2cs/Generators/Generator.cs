using System;
using System.Threading.Tasks;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Py2Cs.Translators;

namespace Py2Cs.Generators
{
    public class Generator
    {
        private readonly PythonCache _pythonCache;

        public Generator(Translator translator)
        {
            _pythonCache = new PythonCache(this);

            this.Translator = translator;
        }

        public string PythonDir { get; set; }

        public Translator Translator { get; }

        public async Task<Project> Generate(Project project)
        {
            project = await ApplyRewriter(project, model => new ClassGeneratorRewriter(this, model, _pythonCache));
            project = await ApplyRewriter(project, model => new MethodGeneratorRewriter(this, model, _pythonCache));

            return project;
        }

        private async Task<Project> ApplyRewriter(Project project, Func<SemanticModel, CSharpSyntaxRewriter> rewriterFactory)
        {
            var compilation = await project.GetCompilationAsync();

            foreach (var documentId in project.DocumentIds)
            {
                var document = project.GetDocument(documentId);
                var documentTree = await document.GetSyntaxTreeAsync();
                var documentRoot = await documentTree.GetRootAsync();

                var model = compilation.GetSemanticModel(documentTree);
                var rewriter = rewriterFactory(model);
                documentRoot = rewriter.Visit(documentRoot);

                var newDocument = document.WithSyntaxRoot(documentRoot.NormalizeWhitespace());
                project = newDocument.Project;
            }

            return project;
        }
    }
}