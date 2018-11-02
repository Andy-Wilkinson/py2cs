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
    public class PythonFile : IPythonNode
    {
        private PythonFile(string filename, PythonAst pythonAst)
        {
            this.Filename = filename;
            this.PythonAst = pythonAst;

            this.Children = new Dictionary<string, IPythonNode>();
        }

        public string Filename { get; }

        public PythonAst PythonAst { get; }

        public Dictionary<string, IPythonNode> Children { get; }

        public static PythonFile CreateFromFile(string filename)
        {
            var pythonAst = ParsePythonFile(filename);
            var file = new PythonFile(filename, pythonAst);
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