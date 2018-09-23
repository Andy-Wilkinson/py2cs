using System.Collections.Generic;
using System.IO;
using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Py2Cs.Translators;

namespace Py2Cs.Generators
{
    public class PythonCache
    {
        private Translator _translator;
        private Dictionary<string, PythonNode> _pythonCache = new Dictionary<string, PythonNode>();

        public PythonCache(Translator translator)
        {
            this._translator = translator;
        }

        public PythonNode GetPythonFile(string path)
        {
            path = Path.GetFullPath(path);
            PythonNode rootNode;

            if (!_pythonCache.TryGetValue(path, out rootNode))
            {
                var pythonAst = ParsePythonFile(path);
                rootNode = _translator.Extract(pythonAst);
                _pythonCache[path] = rootNode;
            }

            return rootNode;
        }

        private PythonAst ParsePythonFile(string path)
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