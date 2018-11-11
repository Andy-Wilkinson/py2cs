using System;
using System.Linq;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Py2Cs.CodeGraphs;

namespace Py2Cs.Translators
{
    public partial class Translator
    {
        private ExpressionResult TranslateExpression(Expression pyExpression, TranslatorState state)
        {
            switch (pyExpression)
            {
                // case UnaryExpression unaryExpression:
                //     return TranslateExpression_Unary(unaryExpression, state);
                // case BinaryExpression binaryExpression:
                //     return TranslateExpression_Binary(binaryExpression, state);
                // case AndExpression andExpression:
                //     return TranslateExpression_And(andExpression, state);
                // case OrExpression orExpression:
                //     return TranslateExpression_Or(orExpression, state);
                // case ParenthesisExpression parenthesisExpression:
                //     return TranslateExpression_Parenthesis(parenthesisExpression, state);
                case ConstantExpression constantExpression:
                    return TranslateExpression_Constant(constantExpression);
                // case ListExpression listExpression:
                //     return TranslateExpression_List(listExpression, state);
                // case DictionaryExpression dictionaryExpression:
                //     return TranslateExpression_Dictionary(dictionaryExpression, state);
                case NameExpression nameExpression:
                    return TranslateExpression_Name(nameExpression, state);
                case MemberExpression memberExpression:
                    return TranslateExpression_Member(memberExpression, state);
                // case IndexExpression indexExpression:
                //     return TranslateExpression_Index(indexExpression, state);
                // case TupleExpression tupleExpression:
                //     return TranslateExpression_Tuple(tupleExpression, state);
                // case CallExpression callExpression:
                //     return TranslateExpression_Call(callExpression, state);
                default:
                    return ExpressionResult.WithError($"// py2cs: Unknown expression type ({pyExpression.NodeName}, {pyExpression.GetType()})");
            }
        }

        // private SyntaxKind TranslateOperator(PythonOperator pythonOperator)
        // {
        //     switch (pythonOperator)
        //     {
        //         // Unary expressions
        //         case PythonOperator.Not: return SyntaxKind.LogicalNotExpression;
        //         // TODO : Pos
        //         // TODO : Invert
        //         // TODO : Negate

        //         // Binary expressions
        //         case PythonOperator.Add: return SyntaxKind.AddExpression;
        //         case PythonOperator.Subtract: return SyntaxKind.SubtractExpression;
        //         case PythonOperator.Multiply: return SyntaxKind.MultiplyExpression;
        //         case PythonOperator.Divide: return SyntaxKind.DivideExpression;
        //         case PythonOperator.TrueDivide: return SyntaxKind.DivideExpression;
        //         case PythonOperator.Mod: return SyntaxKind.ModuloExpression;
        //         case PythonOperator.BitwiseAnd: return SyntaxKind.BitwiseAndExpression;
        //         // TODO : Xor
        //         case PythonOperator.BitwiseOr: return SyntaxKind.BitwiseOrExpression;
        //         case PythonOperator.ExclusiveOr: return SyntaxKind.ExclusiveOrExpression;
        //         case PythonOperator.LeftShift: return SyntaxKind.LeftShiftExpression;
        //         case PythonOperator.RightShift: return SyntaxKind.RightShiftExpression;
        //         // TODO : Power
        //         // TODO : FloorDivide
        //         case PythonOperator.LessThan: return SyntaxKind.LessThanExpression;
        //         case PythonOperator.LessThanOrEqual: return SyntaxKind.LessThanOrEqualExpression;
        //         case PythonOperator.GreaterThan: return SyntaxKind.GreaterThanExpression;
        //         case PythonOperator.GreaterThanOrEqual: return SyntaxKind.GreaterThanOrEqualExpression;
        //         // TODO : Equal
        //         case PythonOperator.Equals: return SyntaxKind.EqualsExpression;
        //         // TODO : NotEqual
        //         case PythonOperator.NotEquals: return SyntaxKind.NotEqualsExpression;
        //         // TODO : In
        //         // TODO : NotIn
        //         // Note: IsNot is implemented as a special case
        //         case PythonOperator.Is: return SyntaxKind.IsExpression;
        //         default: return SyntaxKind.None;
        //     }
        // }

        // private ExpressionResult TranslateExpression_Unary(UnaryExpression unaryExpression, TranslatorState state)
        // {
        //     var operatorKind = TranslateOperator(unaryExpression.Op);

        //     if (operatorKind == SyntaxKind.None)
        //         return ExpressionResult.WithError($"// py2cs: Unknown unary expression type ({unaryExpression.Op})");

        //     return TranslateExpression_Unary(operatorKind, unaryExpression.Expression, state);
        // }

        // private ExpressionResult TranslateExpression_Unary(SyntaxKind kind, Expression expression, TranslatorState state)
        // {
        //     var exp = TranslateExpression(expression, state);

        //     if (exp.IsError)
        //         return ExpressionResult.WithErrors(exp.Errors);

        //     return SyntaxFactory.PrefixUnaryExpression(kind, exp.Syntax);
        // }

        // private ExpressionResult TranslateExpression_Binary(BinaryExpression binaryExpression, TranslatorState state)
        // {
        //     if (binaryExpression.Operator == PythonOperator.IsNot)
        //         return TranslateExpression_Binary_IsNot(binaryExpression.Left, binaryExpression.Right, state);

        //     var operatorKind = TranslateOperator(binaryExpression.Operator);

        //     if (operatorKind == SyntaxKind.None)
        //         return ExpressionResult.WithError($"// py2cs: Unknown binary expression type ({binaryExpression.Operator})");

        //     return TranslateExpression_Binary(operatorKind, binaryExpression.Left, binaryExpression.Right, state);
        // }

        // private ExpressionResult TranslateExpression_Binary_IsNot(Expression left, Expression right, TranslatorState state)
        // {
        //     var isExpression = TranslateExpression_Binary(SyntaxKind.IsExpression, left, right, state);

        //     if (isExpression.IsError)
        //         return ExpressionResult.WithErrors(isExpression.Errors);

        //     var parenthesisExpression = SyntaxFactory.ParenthesizedExpression(isExpression.Syntax);
        //     return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, parenthesisExpression);
        // }

        // private ExpressionResult TranslateExpression_Binary(SyntaxKind kind, Expression leftExpression, Expression rightExpression, TranslatorState state)
        // {
        //     var left = TranslateExpression(leftExpression, state);
        //     var right = TranslateExpression(rightExpression, state);

        //     if (left.IsError || right.IsError)
        //         return ExpressionResult.WithErrors(Enumerable.Concat(left.Errors, right.Errors));

        //     return SyntaxFactory.BinaryExpression(kind, left.Syntax, right.Syntax);
        // }

        // private ExpressionResult TranslateExpression_And(AndExpression andExpression, TranslatorState state)
        // {
        //     return TranslateExpression_Binary(SyntaxKind.LogicalAndExpression, andExpression.Left, andExpression.Right, state);
        // }

        // private ExpressionResult TranslateExpression_Or(OrExpression orExpression, TranslatorState state)
        // {
        //     return TranslateExpression_Binary(SyntaxKind.LogicalOrExpression, orExpression.Left, orExpression.Right, state);
        // }

        // private ExpressionResult TranslateExpression_Parenthesis(ParenthesisExpression parenthesisExpression, TranslatorState state)
        // {
        //     var expression = TranslateExpression(parenthesisExpression.Expression, state);

        //     if (expression.IsError)
        //         return ExpressionResult.WithErrors(expression.Errors);

        //     return SyntaxFactory.ParenthesizedExpression(expression.Syntax);
        // }

        private ExpressionResult TranslateExpression_Constant(ConstantExpression constantExpression)
        {
            switch (constantExpression.Value)
            {
                case null:
                    return ExpressionResult.Result(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression), PythonTypes.None);
                case int value:
                    return ExpressionResult.Result(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value)), PythonTypes.Int);
                case string str:
                    return ExpressionResult.Result(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(str)), PythonTypes.Str);
                default:
                    return ExpressionResult.WithError($"// py2cs: Unknown constant expression type: {constantExpression.Value.GetType()}");
            }
        }

        // private ExpressionResult TranslateExpression_List(ListExpression listExpression, TranslatorState state)
        // {
        //     var items = listExpression.Items.Select(item => TranslateExpression(item, state));

        //     if (items.Count(item => item.IsError) > 0)
        //     {
        //         var errors = items.SelectMany(item => item.Errors);
        //         return ExpressionResult.WithErrors(errors.ToList());
        //     }

        //     var arrayType = SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName("object[]"));
        //     var itemExpressions = SyntaxFactory.SeparatedList<ExpressionSyntax>(items.Select(item => item.Syntax));
        //     var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, itemExpressions);

        //     return SyntaxFactory.ArrayCreationExpression(arrayType, initializer);
        // }

        // private ExpressionResult TranslateExpression_Dictionary(DictionaryExpression dictionaryExpression, TranslatorState state)
        // {
        //     var dictionaryType = SyntaxFactory.ParseTypeName("Dictionary<object,object>");
        //     var dictionaryCreator = SyntaxFactory.ObjectCreationExpression(dictionaryType);

        //     if (dictionaryExpression.Items.Count > 0)
        //     {
        //         var items = SyntaxFactory.SeparatedList<ExpressionSyntax>();

        //         foreach (var item in dictionaryExpression.Items)
        //         {
        //             if (item.SliceStep != null || item.StepProvided == true)
        //                 return ExpressionResult.WithError("// py2cs: Unsupported slice step in dictionary expression");

        //             var keyExpression = TranslateExpression(item.SliceStart, state);
        //             var valueExpression = TranslateExpression(item.SliceStop, state);

        //             if (keyExpression.IsError || valueExpression.IsError)
        //                 return ExpressionResult.WithErrors(Enumerable.Concat(keyExpression.Errors, valueExpression.Errors));

        //             var keyValueList = SyntaxFactory.SeparatedList<ExpressionSyntax>(new[] { keyExpression.Syntax, valueExpression.Syntax });
        //             var keyValuePair = SyntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, keyValueList);
        //             items = items.Add(keyValuePair);
        //         }

        //         var itemExpressions = SyntaxFactory.SeparatedList<ExpressionSyntax>(items);
        //         var initializer = SyntaxFactory.InitializerExpression(SyntaxKind.CollectionInitializerExpression, itemExpressions);
        //         dictionaryCreator = dictionaryCreator.WithInitializer(initializer);
        //     }

        //     return dictionaryCreator;
        // }

        private ExpressionResult TranslateExpression_Name(NameExpression nameExpression, TranslatorState state)
        {
            switch (nameExpression.Name)
            {
                case "True":
                    return ExpressionResult.Result(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression), PythonTypes.Bool);
                case "False":
                    return ExpressionResult.Result(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression), PythonTypes.Bool);
                default:
                    if (state.Variables.TryGetValue(nameExpression.Name, out ExpressionResult variable))
                        return variable;
                    else
                        return ExpressionResult.WithError($"// py2cs: Unknown name expression called: {nameExpression.Name}");
            }
        }

        private ExpressionResult TranslateExpression_Member(MemberExpression memberExpression, TranslatorState state)
        {
            var target = TranslateExpression(memberExpression.Target, state);

            if (target.IsError)
                return ExpressionResult.WithErrors(target.Errors);

            var targetClass = target.Type.Node as PythonClass;

            if (targetClass == null)
                return ExpressionResult.WithError($"// py2cs: Unknown type for member expression: {memberExpression.Name}");

            if (!targetClass.Children.TryGetValue(memberExpression.Name, out var memberNode))
                return ExpressionResult.WithError($"// py2cs: Unknown member expression on type {targetClass}: {memberExpression.Name}");

            var name = SyntaxFactory.IdentifierName(memberExpression.Name);
            var expression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, target.Syntax, name);
            var expressionType = GetMemberExpressionType(memberNode);

            return ExpressionResult.Result(expression, expressionType);
        }

        private PythonType GetMemberExpressionType(IPythonNode memberNode)
        {
            switch (memberNode)
            {
                case PythonField pythonField:
                    return pythonField.Type;
                default:
                    return PythonTypes.Unknown;
            }
        }

        // private ExpressionResult TranslateExpression_Index(IndexExpression indexExpression, TranslatorState state)
        // {
        //     var target = TranslateExpression(indexExpression.Target, state);
        //     var index = TranslateExpression(indexExpression.Index, state);

        //     if (target.IsError || index.IsError)
        //         return ExpressionResult.WithErrors(Enumerable.Concat(target.Errors, index.Errors));

        //     var indexArgument = SyntaxFactory.Argument(index.Syntax);
        //     var argumentList = SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList(indexArgument));

        //     return SyntaxFactory.ElementAccessExpression(target.Syntax, argumentList);
        // }

        // private ExpressionResult TranslateExpression_Tuple(TupleExpression tupleExpression, TranslatorState state)
        // {
        //     var argumentList = SyntaxFactory.SeparatedList<ArgumentSyntax>();

        //     foreach (Expression expression in tupleExpression.Items)
        //     {
        //         var argumentExpression = TranslateExpression(expression, state);

        //         if (argumentExpression.IsError)
        //             return ExpressionResult.WithErrors(argumentExpression.Errors);

        //         argumentList = argumentList.Add(SyntaxFactory.Argument(argumentExpression.Syntax));
        //     }

        //     return SyntaxFactory.TupleExpression(argumentList);
        // }

        // private ExpressionResult TranslateExpression_Call(CallExpression callExpression, TranslatorState state)
        // {
        //     var target = TranslateExpression(callExpression.Target, state);

        //     if (target.IsError)
        //         return ExpressionResult.WithErrors(target.Errors);

        //     var argumentList = SyntaxFactory.SeparatedList<ArgumentSyntax>();

        //     foreach (Arg arg in callExpression.Args)
        //     {
        //         var argumentExpression = TranslateExpression(arg.Expression, state);

        //         if (argumentExpression.IsError)
        //             return ExpressionResult.WithErrors(argumentExpression.Errors);

        //         var argument = SyntaxFactory.Argument(argumentExpression.Syntax);

        //         if (arg.Name != null)
        //         {
        //             var name = SyntaxFactory.NameColon(arg.Name);
        //             argument = argument.WithNameColon(name);
        //         }

        //         argumentList = argumentList.Add(argument);
        //     }

        //     return SyntaxFactory.InvocationExpression(target.Syntax, SyntaxFactory.ArgumentList(argumentList));
        // }
    }
}