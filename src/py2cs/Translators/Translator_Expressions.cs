using System;
using System.Linq;
using IronPython.Compiler;
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
                case AndExpression andExpression:
                    return TranslateExpression_And(andExpression);
                case OrExpression orExpression:
                    return TranslateExpression_Or(orExpression);
                case BinaryExpression binaryExpression:
                    return TranslateExpression_Binary(binaryExpression);
                case ParenthesisExpression parenthesisExpression:
                    return TranslateExpression_Parenthesis(parenthesisExpression);
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

        private SyntaxResult<ExpressionSyntax> TranslateExpression_And(AndExpression andExpression)
        {
            return TranslateExpression_Binary(SyntaxKind.LogicalAndExpression, andExpression.Left, andExpression.Right);
        }

        private SyntaxResult<ExpressionSyntax> TranslateExpression_Or(OrExpression orExpression)
        {
            return TranslateExpression_Binary(SyntaxKind.LogicalOrExpression, orExpression.Left, orExpression.Right);
        }

        private SyntaxResult<ExpressionSyntax> TranslateExpression_Binary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Operator == PythonOperator.IsNot)
                return TranslateExpression_Binary_IsNot(binaryExpression.Left, binaryExpression.Right);

            var operatorKind = TranslateOperator(binaryExpression.Operator);

            if (operatorKind == SyntaxKind.None)
                return SyntaxResult<ExpressionSyntax>.WithError($"// py2cs: Unknown binary expression type ({binaryExpression.Operator})");

            return TranslateExpression_Binary(operatorKind, binaryExpression.Left, binaryExpression.Right);
        }

        private SyntaxResult<ExpressionSyntax> TranslateExpression_Binary_IsNot(Expression left, Expression right)
        {
            var isExpression = TranslateExpression_Binary(SyntaxKind.IsExpression, left, right);

            if (isExpression.IsError)
                return SyntaxResult<ExpressionSyntax>.WithErrors(isExpression.Errors);

            var parenthesisExpression = SyntaxFactory.ParenthesizedExpression(isExpression.Syntax);
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, parenthesisExpression);
        }

        private SyntaxKind TranslateOperator(PythonOperator pythonOperator)
        {
            switch (pythonOperator)
            {
                case PythonOperator.Add: return SyntaxKind.AddExpression;
                case PythonOperator.Subtract: return SyntaxKind.SubtractExpression;
                case PythonOperator.Multiply: return SyntaxKind.MultiplyExpression;
                case PythonOperator.Divide: return SyntaxKind.DivideExpression;
                case PythonOperator.BitwiseAnd: return SyntaxKind.BitwiseAndExpression;
                case PythonOperator.BitwiseOr: return SyntaxKind.BitwiseOrExpression;
                case PythonOperator.Equals: return SyntaxKind.EqualsExpression;
                case PythonOperator.LessThan: return SyntaxKind.LessThanExpression;
                case PythonOperator.LessThanOrEqual: return SyntaxKind.LessThanOrEqualExpression;
                case PythonOperator.GreaterThan: return SyntaxKind.GreaterThanExpression;
                case PythonOperator.GreaterThanOrEqual: return SyntaxKind.GreaterThanOrEqualExpression;
                case PythonOperator.NotEquals: return SyntaxKind.NotEqualsExpression;
                case PythonOperator.Is: return SyntaxKind.IsExpression;
                default: return SyntaxKind.None;
            }
        }

        private SyntaxResult<ExpressionSyntax> TranslateExpression_Binary(SyntaxKind kind, Expression leftExpression, Expression rightExpression)
        {
            var left = TranslateExpression(leftExpression);
            var right = TranslateExpression(rightExpression);

            if (left.IsError || right.IsError)
                return SyntaxResult<ExpressionSyntax>.WithErrors(Enumerable.Concat(left.Errors, right.Errors));

            return SyntaxFactory.BinaryExpression(kind, left.Syntax, right.Syntax);
        }

        private SyntaxResult<ExpressionSyntax> TranslateExpression_Parenthesis(ParenthesisExpression parenthesisExpression)
        {
            var expression = TranslateExpression(parenthesisExpression.Expression);

            if (expression.IsError)
                return SyntaxResult<ExpressionSyntax>.WithErrors(expression.Errors);

            return SyntaxFactory.ParenthesizedExpression(expression.Syntax);
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