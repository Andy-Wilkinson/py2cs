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
            AttributeWalker projectAttributes = new AttributeWalker();
            await projectAttributes.WalkProject(project);

            LogAttributes(projectAttributes);

            PythonGraph pythonGraph = new PythonGraph();
            CsPyMapper.MapAttributesToGraph(pythonGraph, projectAttributes);

            // foreach (var entryPoint in pythonMappings.PythonEntryPoints)
            // {
            //     pythonGraph.AddPythonFile(entryPoint);
            // }

            // foreach (var mapping in pythonMappings.FieldMappings)
            // {
            //     var fieldNameParts = mapping.Value.FieldName.Split(".");
            //     var className = string.Join(".", fieldNameParts.SkipLast(1));
            //     var fieldName = fieldNameParts.Last();

            //     var pythonClass = pythonGraph.GetClass(mapping.Value.File, className);
            //     var pythonFieldType = GetPythonType(pythonGraph, pythonMappings, mapping.Key.Type);
            //     var pythonField = PythonField.Create(fieldName, pythonFieldType);
            //     pythonClass.Children[fieldName] = pythonField;
            // }

            // foreach (var mapping in pythonMappings.MethodMappings)
            // {
            //     var pythonFunction = pythonGraph.GetFunction(mapping.Value.File, mapping.Value.FunctionName);
            //     MapFunctionTypes(pythonGraph, pythonMappings, pythonFunction, mapping.Key);
            // }

            LogGraph(pythonGraph);

            // // project = await ApplyRewriter(project, model => new ClassGeneratorRewriter(this, model, _pythonCache));
            // project = await ApplyRewriter(project, model => new MethodGeneratorRewriter(this, model, pythonGraph, pythonMappings));

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

        public void LogAttributes(AttributeWalker projectAttributes)
        {
            Logger.LogHeading("Attributes - Class", LogLevel.Info);

            foreach (var entry in projectAttributes.ClassAttributes)
            {
                string target = entry.Key.ToString();
                PythonClassAttribute attribute = entry.Value;
                Logger.Log($"{target} -> {attribute.ModuleName}:{attribute.ClassName} ({attribute.File}) {(attribute.GenerateMethods ? "*" : "")}", LogLevel.Info);
            }

            Logger.LogHeading("Attributes - Fields", LogLevel.Info);

            foreach (var entry in projectAttributes.FieldAttributes)
            {
                string target = entry.Key.ToString();
                PythonFieldAttribute attribute = entry.Value;
                Logger.Log($"{target} -> {attribute.Name} ({attribute.File})", LogLevel.Info);
            }

            Logger.LogHeading("Attributes - Methods", LogLevel.Info);

            foreach (var entry in projectAttributes.MethodAttributes)
            {
                string target = entry.Key.ToString();
                PythonMethodAttribute attribute = entry.Value;
                Logger.Log($"{target} -> {attribute.Function} ({attribute.File}) {(attribute.Generate ? "*" : "")}", LogLevel.Info);
            }

            Logger.LogHeading("Attributes - Properties", LogLevel.Info);

            foreach (var entry in projectAttributes.PropertyAttributes)
            {
                string target = entry.Key.ToString();
                PythonPropertyAttribute attribute = entry.Value;

                Logger.Log($"{target} -> {attribute.Name} ({attribute.File})", LogLevel.Info);
            }

            Logger.LogHeading("Attributes - Operators", LogLevel.Info);

            foreach (var entry in projectAttributes.OperatorAttributes)
            {
                string target = entry.Key.ToString();
                PythonOperatorAttribute attribute = entry.Value;
                Logger.Log($"{target} -> {attribute.Operator}", LogLevel.Info);
            }
        }

        public void LogGraph(PythonGraph graph)
        {
            Logger.LogHeading("Python Graph", LogLevel.Info);

            foreach (var entry in graph.Modules)
            {
                PythonModule module = entry.Value;
                LogNode(module, "");
            }
        }

        public void LogNode(IPythonNode node, string prefix)
        {
            switch (node)
            {
                case PythonModule pythonModule:
                    Logger.Log($"{prefix}Module: {pythonModule.Name}", LogLevel.Info);
                    break;
                case PythonClass pythonClass:
                    Logger.Log($"{prefix}Class: {pythonClass}", LogLevel.Info);
                    break;
                case PythonField pythonField:
                    Logger.Log($"{prefix}Field: {pythonField}", LogLevel.Info);
                    break;
                case PythonProperty pythonProperty:
                    Logger.Log($"{prefix}Property: {pythonProperty}", LogLevel.Info);
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