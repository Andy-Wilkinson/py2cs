using System;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Py2Cs.Translators
{
    public partial class Translator
    {
        public CompilationUnitSyntax Translate(PythonAst ast)
        {
            var compilationUnit = SyntaxFactory.CompilationUnit();

            var children = TranslateBlock_Members(ast.Body);

            if (children.IsError)
            {
                return compilationUnit.WithTrailingTrivia(children.Errors);
            }

            foreach (var child in children.Syntax)
            {
                switch (child)
                {
                    case MemberDeclarationSyntax member:
                        compilationUnit = compilationUnit.AddMembers(member);
                        break;
                    case UsingDirectiveSyntax usingDirective:
                        compilationUnit = compilationUnit.AddUsings(usingDirective);
                        break;
                    default:
                        var comment = SyntaxFactory.Comment($"// py2cs: Unexpected child statement ({child.GetType()})");
                        compilationUnit = compilationUnit.WithTrailingTrivia(comment);
                        break;
                }
            }

            return compilationUnit;
        }
    }
}