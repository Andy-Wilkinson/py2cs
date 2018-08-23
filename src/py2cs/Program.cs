using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using IronPython;
using IronPython.Compiler;
using IronPython.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Py2Cs.Generators;

namespace Py2Cs
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Test();
            // return;

            var inputCsprojFile = "../../../test-py/test-py.csproj";

            // Load C# project

            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(inputCsprojFile);
            var workspace = analyzer.GetWorkspace();
            var project = workspace.CurrentSolution.Projects.First();

            // Code generation

            var generator = new Generator();
            var newProject = await generator.Generate(project);

            // Save project changes

            // NB : This should work to save changes? Maybe isn't implemented with Buildalyzer?
            // bool success = workspace.TryApplyChanges(newProject.Solution);

            var projectChanges = newProject.GetChanges(project);

            foreach (var changedDocumentId in projectChanges.GetChangedDocuments())
            {
                var document = newProject.GetDocument(changedDocumentId);
                var documentRoot = await document.GetSyntaxRootAsync();
                var code = documentRoot.ToFullString();
                File.WriteAllText(document.FilePath, code);
            }
        }

        static void Test()
        {
            var text = "public class Test { public A() { var x = new System.Collections.Generic.Dictionary<object, object>{ {1,2}, {2,3}}}; }";
            SyntaxTree x = SyntaxFactory.ParseSyntaxTree(text);
            Dump(x.GetRoot(), "");
        }

        private static void Dump(SyntaxNode syntaxNode, string prefix)
        {
            Console.WriteLine(prefix + " " + syntaxNode.GetType() + "[" + syntaxNode.Kind() + "]");

            if (syntaxNode is CSharpSyntaxNode csharpSyntaxNode)
            {
                foreach (var trivia in csharpSyntaxNode.GetLeadingTrivia())
                    Console.WriteLine(prefix + "> Leading Trivia: " + trivia.GetType());

                foreach (var trivia in csharpSyntaxNode.GetTrailingTrivia())
                    Console.WriteLine(prefix + "> Trailing Trivia: " + trivia.GetType());
            }

            foreach (var child in syntaxNode.ChildNodes())
                Dump(child, prefix + "-");
        }
    }
}
