using System;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Py2Cs.Translators
{
    public partial class Translator
    {
        private SyntaxResult<ExpressionSyntax> TranslateExpression(Expression pyExpression)
        {
            switch (pyExpression)
            {
                case ConstantExpression constantExpression:
                    return TranslateExpression_Constant(constantExpression);
                case NameExpression nameExpression:
                    return TranslateExpression_Name(nameExpression);
                case MemberExpression memberExpression:
                    return TranslateExpression_Member(memberExpression);
                default:
                    return SyntaxResult<ExpressionSyntax>.WithError($"// py2cs: Unknown expression type ({pyExpression.NodeName}, {pyExpression.GetType()})");
            }
        }

        private SyntaxResult<ExpressionSyntax> TranslateExpression_Constant(ConstantExpression constantExpression)
        {
            switch (constantExpression.Value)
            {
                case null:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                case string str:
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(str));
                default:
                    return SyntaxResult<ExpressionSyntax>.WithError($"// py2cs: Unknown constant expression type: {constantExpression.Value.GetType()}");
            }
        }

        private ExpressionSyntax TranslateExpression_Name(NameExpression nameExpression)
        {
            switch (nameExpression.Name)
            {
                case "True":
                    return SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
                case "False":
                    return SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
                default:
                    return SyntaxFactory.ParseExpression(nameExpression.Name);
            }
        }

        private SyntaxResult<ExpressionSyntax> TranslateExpression_Member(MemberExpression memberExpression)
        {
            var target = TranslateExpression(memberExpression.Target);
            var name = SyntaxFactory.IdentifierName(memberExpression.Name);

            if (target.IsError)
                return SyntaxResult<ExpressionSyntax>.WithErrors(target.Errors);

            return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, target.Syntax, name);
        }
    }
}