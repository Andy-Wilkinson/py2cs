using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Py2Cs.CodeGraphs;

namespace Py2Cs.Translators
{
    public struct ExpressionResult
    {
        public ExpressionResult(ExpressionSyntax syntax, PythonType type, IEnumerable<SyntaxTrivia> errors)
        {
            this.Syntax = syntax;
            this.Type = type;
            this.Errors = errors;
        }

        public ExpressionSyntax Syntax
        {
            get;
            private set;
        }

        public PythonType Type
        {
            get;
            private set;
        }

        public IEnumerable<SyntaxTrivia> Errors
        {
            get;
            private set;
        }

        public bool IsError
        {
            get => Syntax == null;
        }

        static public ExpressionResult Result(ExpressionSyntax syntax, PythonType type)
        {
            return new ExpressionResult(syntax, type, new SyntaxTrivia[] { });
        }

        static public ExpressionResult WithError(string error)
        {
            var comment = SyntaxFactory.Comment(error);
            return new ExpressionResult(null, null, new[] { comment });
        }

        static public ExpressionResult WithErrors(IEnumerable<SyntaxTrivia> errors)
        {
            return new ExpressionResult(null, null, errors);
        }
    }
}