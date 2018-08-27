using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using IronPython;
using IronPython.Compiler;
using IronPython.Hosting;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Py2Cs.Generators;
using Py2Cs.Translators;

namespace Py2Cs
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        => await CommandLineApplication.ExecuteAsync<Program>(args);

        [Argument(0, "Project", "The .csproj file to convert")]
        [Required]
        public string Project { get; }

        [Option(Description = "The root directory for locating python files")]
        public string PythonDir { get; }

        private async Task OnExecute()
        {
            // Load C# project

            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(Project);
            var workspace = analyzer.GetWorkspace();
            var project = workspace.CurrentSolution.Projects.First();

            // Code generation

            var translator = new Translator();
            var generator = new Generator(translator)
            {
                PythonDir = this.PythonDir
            };
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
    }
}
