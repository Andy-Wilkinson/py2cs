using System;
using System.Collections.Generic;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Py2Cs.Translators
{
    public struct SyntaxResult<T> where T : class
    {
        private SyntaxResult(T syntax, IEnumerable<SyntaxTrivia> errors)
        {
            this.Syntax = syntax;
            this.Errors = errors;
        }

        public T Syntax
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

        static public implicit operator SyntaxResult<T>(T value)
        {
            return new SyntaxResult<T>(value, new SyntaxTrivia[] { });
        }

        static public SyntaxResult<T> WithError(string error)
        {
            var comment = SyntaxFactory.Comment(error);
            return new SyntaxResult<T>(null, new[] { comment });
        }

        static public SyntaxResult<T> WithErrors(IEnumerable<SyntaxTrivia> errors)
        {
            return new SyntaxResult<T>(null, errors);
        }
    }
}