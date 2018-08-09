using System;
using System.Collections.Generic;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Py2Cs.Translators
{
    public partial class Translator
    {
        private SyntaxResult<SyntaxNode[]> TranslateBlock_Members(Statement statement)
        {
            if (statement is SuiteStatement suiteStatement)
            {
                var errors = new List<SyntaxTrivia>();
                var members = new List<SyntaxNode>();

                foreach (Statement memberStatement in suiteStatement.Statements)
                {
                    var member = TranslateStatement(memberStatement);

                    if (member.IsError)
                    {
                        errors.AddRange(member.Errors);
                    }
                    else
                    {
                        members.Add(member.Syntax.WithLeadingTrivia(errors));
                        errors.Clear();
                    }
                }

                if (errors.Count != 0)
                {
                    if (members.Count > 0)
                        members[members.Count - 1] = members[members.Count - 1].WithTrailingTrivia(errors);
                    else
                        return SyntaxResult<SyntaxNode[]>.WithErrors(errors);
                }

                return members.ToArray();
            }
            else
            {
                return SyntaxResult<SyntaxNode[]>.WithError($"// py2cs: Expected SuiteStatement");
            }
        }

        private BlockSyntax TranslateBlock_Block(Statement statement)
        {
            var body = SyntaxFactory.Block();

            var children = TranslateBlock_Members(statement);

            if (children.IsError)
            {
                return body.WithTrailingTrivia(children.Errors);
            }

            foreach (var child in children.Syntax)
            {
                switch (child)
                {
                    case StatementSyntax childStatement:
                        body = body.AddStatements(childStatement);
                        break;
                    default:
                        var comment = SyntaxFactory.Comment($"// py2cs: Unexpected child statement ({child.GetType()})");
                        body = body.WithTrailingTrivia(comment);
                        break;
                }
            }

            return body;
        }
    }
}