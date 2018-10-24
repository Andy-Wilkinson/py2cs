using System.Collections.Generic;
using System.IO;
using IronPython;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Py2Cs.Translators;

namespace Py2Cs.Generators
{
    public class PythonCache
    {
        private Generator _generator;
        private Dictionary<string, PythonNode> _pythonCache = new Dictionary<string, PythonNode>();

        public PythonCache(Generator generator)
        {
            this._generator = generator;
        }

        public PythonNode GetPythonFile(CSharpSyntaxNode node, string path)
        {
            var sourceFile = node.GetLocation().SourceTree.FilePath;
            var sourceLocalFilename = Path.Combine(Path.GetDirectoryName(sourceFile), path);

            if (File.Exists(sourceLocalFilename))
                return GetPythonFile(sourceLocalFilename);

            var pythonFolderFilename = Path.Combine(_generator.PythonDir, path);

            if (File.Exists(pythonFolderFilename))
                return GetPythonFile(pythonFolderFilename);

            throw new FileNotFoundException();
        }

        public PythonNode GetPythonFile(string path)
        {
            path = Path.GetFullPath(path);
            PythonNode rootNode;

            if (!_pythonCache.TryGetValue(path, out rootNode))
            {
                var pythonAst = ParsePythonFile(path);
                rootNode = _generator.Translator.Extract(pythonAst);
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