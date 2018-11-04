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

            foreach (var entryPoint in pythonMappings.PythonEntryPoints)
            {
                pythonGraph.AddPythonFile(entryPoint);
            }

            foreach (var mapping in pythonMappings.FieldMappings)
            {
                var fieldNameParts = mapping.Value.FieldName.Split(".");
                var className = string.Join(".", fieldNameParts.SkipLast(1));
                var fieldName = fieldNameParts.Last();

                var pythonClass = pythonGraph.GetClass(mapping.Value.File, className);
                var pythonFieldType = GetPythonType(pythonGraph, pythonMappings, mapping.Key.Type);
                var pythonField = PythonField.Create(fieldName, pythonFieldType);
                pythonClass.Children[fieldName] = pythonField;
            }

            foreach (var mapping in pythonMappings.MethodMappings)
            {
                var pythonFunction = pythonGraph.GetFunction(mapping.Value.File, mapping.Value.FunctionName);
                MapFunctionTypes(pythonGraph, pythonMappings, pythonFunction, mapping.Key);
            }

            LogGraph(pythonGraph);

            // project = await ApplyRewriter(project, model => new ClassGeneratorRewriter(this, model, _pythonCache));
            project = await ApplyRewriter(project, model => new MethodGeneratorRewriter(this, model, pythonGraph, pythonMappings));

            return project;
        }

        private void MapFunctionTypes(PythonGraph pythonGraph, PythonMappings pythonMappings, PythonFunction pythonFunction, IMethodSymbol csMethod)
        {
            if (pythonFunction.Parameters.Count == 0)
            {
                if (csMethod.Parameters.Length != 0)
                    Logger.Log($"Function {pythonFunction} does not have the expected number of parameters.", LogLevel.Error);

                return;
            }

            int parameterOffset = csMethod.IsStatic ? 0 : 1;

            if (csMethod.Parameters.Length != pythonFunction.Parameters.Count - parameterOffset)
            {
                Logger.Log($"Function {pythonFunction} does not have the expected number of parameters.", LogLevel.Error);
                return;
            }

            // If this method is not a static method then check that the first parameter is the correct type

            if (!csMethod.IsStatic)
            {
                var typeMapping = pythonMappings.TypeMappings[csMethod.ContainingType];
                var pythonClass = pythonGraph.GetClass(typeMapping.File, typeMapping.ClassName);

                if (pythonFunction.Parameters[0].Type != pythonClass.Type)
                {
                    Logger.Log($"The first parameter of {pythonFunction} does not have the right type.", LogLevel.Error);
                    return;
                }
            }

            // Set the types for all the parameters

            for (int parameterIndex = 0; parameterIndex < csMethod.Parameters.Length; parameterIndex++)
            {
                var csParameter = csMethod.Parameters[parameterIndex];
                var pythonType = GetPythonType(pythonGraph, pythonMappings, csParameter.Type);

                var pythonParameter = pythonFunction.Parameters[parameterIndex + parameterOffset];

                pythonParameter.Type = pythonType;
            }

            // Set the return type

            var returnType = GetPythonType(pythonGraph, pythonMappings, csMethod.ReturnType);
            pythonFunction.ReturnType = returnType;
        }

        private PythonType GetPythonType(PythonGraph pythonGraph, PythonMappings pythonMappings, ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.None:
                    var typeMapping = pythonMappings.TypeMappings[type];
                    var pythonClass = pythonGraph.GetClass(typeMapping.File, typeMapping.ClassName);
                    return pythonClass.Type;
                case SpecialType.System_Void:
                    return PythonTypes.None;
                case SpecialType.System_String:
                    return PythonTypes.Str;
                case SpecialType.System_Int32:
                    return PythonTypes.Int;
                case SpecialType.System_Double:
                    return PythonTypes.Float;
                case SpecialType.System_Boolean:
                    return PythonTypes.Bool;
                default:
                    return PythonTypes.Unknown;
            }
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
                case PythonField pythonField:
                    Logger.Log($"{prefix}Field: {pythonField}", LogLevel.Info);
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