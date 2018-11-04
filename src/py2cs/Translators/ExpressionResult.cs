using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Py2Cs.Translators
{
    public struct ExpressionResult
    {
        public ExpressionResult(ExpressionSyntax syntax, IEnumerable<SyntaxTrivia> errors)
        {
            this.Syntax = syntax;
            this.Errors = errors;
        }

        public ExpressionSyntax Syntax
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

        static public implicit operator ExpressionResult(ExpressionSyntax value)
        {
            return new ExpressionResult(value, new SyntaxTrivia[] { });
        }

        static public ExpressionResult WithError(string error)
        {
            var comment = SyntaxFactory.Comment(error);
            return new ExpressionResult(null, new[] { comment });
        }

        static public ExpressionResult WithErrors(IEnumerable<SyntaxTrivia> errors)
        {
            return new ExpressionResult(null, errors);
        }
    }
}