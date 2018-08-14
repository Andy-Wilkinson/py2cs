using System;
using System.IO;
using IronPython;
using IronPython.Compiler;
using IronPython.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Runtime;
using Py2Cs.Translators;

namespace Py2Cs
{
    class Program
    {
        static void Main(string[] args)
        {
            // Test();
            // return;

            var inputFile = "../../../test-py/test.py";
            var outputFile = "../../../test-py/test.cs";

            var pythonEngine = Python.CreateEngine();
            var pythonSource = pythonEngine.CreateScriptSourceFromFile(inputFile);
            var pythonSourceUnit = HostingHelpers.GetSourceUnit(pythonSource);
            var context = new CompilerContext(pythonSourceUnit, pythonEngine.GetCompilerOptions(), ErrorSink.Default);
            var options = new PythonOptions();
            var parser = Parser.CreateParser(context, options);

            var ast = parser.ParseFile(false);

            var translator = new Translator();
            var translated = translator.Translate(ast);

            var code = translated.NormalizeWhitespace().ToFullString();
            File.WriteAllText(outputFile, code);
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
