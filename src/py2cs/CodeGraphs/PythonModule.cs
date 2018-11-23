using System;
using System.Collections.Generic;
using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Py2Cs.Translators;

namespace Py2Cs.CodeGraphs
{
    public class PythonModule : IPythonNode
    {
        private PythonModule(string name, PythonAst pythonAst)
        {
            this.Name = name;
            this.PythonAst = pythonAst;

            this.Children = new Dictionary<string, IPythonNode>();
        }

        public string Name { get; }

        public PythonAst PythonAst { get; }

        public Dictionary<string, IPythonNode> Children { get; }

        public static PythonModule CreateWithoutFile(string name)
        {
            return new PythonModule(name, null);
        }

        public static PythonModule CreateFromFile(string name)
        {
            var pythonAst = ParsePythonFile(name);
            var file = new PythonModule(name, pythonAst);
            file.ExtractChildren(pythonAst.Body);
            return file;
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