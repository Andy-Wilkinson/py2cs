using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Py2Cs.CodeGraphs;
using Py2Cs.Logging;
using Py2Cs.Translators;

namespace Py2Cs.Generators
{
    public class Generator
    {
        private readonly PythonCache _pythonCache;

        public Generator(Translator translator, Logger logger)
        {
            _pythonCache = new PythonCache(this);

            this.Logger = logger;
            this.Translator = translator;
        }

        public Logger Logger { get; }

        public string PythonDir { get; set; }

        public Translator Translator { get; }

        public async Task<Project> Generate(Project project)
        {
            PythonMappings pythonMappings = new PythonMappings(PythonDir);
            await pythonMappings.WalkProject(project);

            LogMappings(pythonMappings);

            PythonGraph pythonGraph = new PythonGraph();

            foreach (string entryPoint in pythonMappings.PythonEntryPoints)
            {
                pythonGraph.AddPythonFile(entryPoint);
            }

            LogGraph(pythonGraph);

            // project = await ApplyRewriter(project, model => new ClassGeneratorRewriter(this, model, _pythonCache));
            // project = await ApplyRewriter(project, model => new MethodGeneratorRewriter(this, model, _pythonCache));

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

        public void LogMappings(PythonMappings mappings)
        {
            Logger.LogHeading("Type Mappings", LogLevel.Info);

            foreach (var entry in mappings.TypeMappings)
            {
                string typeName = entry.Key.ToString();
                PythonTypeMapping mapping = entry.Value;
                Logger.Log($"{typeName} -> {mapping.ClassName} ({mapping.File})", LogLevel.Info);
            }

            Logger.LogHeading("Method Mappings", LogLevel.Info);

            foreach (var entry in mappings.MethodMappings)
            {
                string typeName = entry.Key.ToString();
                PythonMethodMapping mapping = entry.Value;
                Logger.Log($"{typeName} -> {mapping.FunctionName} ({mapping.File})", LogLevel.Info);
            }

            Logger.LogHeading("Entry Points", LogLevel.Info);

            foreach (var entryPoint in mappings.PythonEntryPoints)
            {
                Logger.Log(entryPoint, LogLevel.Info);
            }
        }

        public void LogGraph(PythonGraph graph)
        {
            Logger.LogHeading("Python Graph", LogLevel.Info);

            foreach (var entry in graph.Files)
            {
                PythonFile file = entry.Value;
                LogNode(file, "");
            }
        }

        public void LogNode(IPythonNode node, string prefix)
        {
            switch (node)
            {
                case PythonFile pythonFile:
                    Logger.Log($"{prefix}File: {pythonFile.Filename}", LogLevel.Info);
                    break;
                case PythonClass pythonClass:
                    Logger.Log($"{prefix}Class: {pythonClass}", LogLevel.Info);
                    break;
                case PythonFunction pythonFunction:
                    Logger.Log($"{prefix}Function: {pythonFunction}", LogLevel.Info);
                    break;
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    LogNode(child.Value, prefix + "  ");
                }
            }
        }
    }
}