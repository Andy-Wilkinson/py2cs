using System;
using System.Collections.Generic;
using System.Linq;
using IronPython.Compiler.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Py2Cs.Translators
{
    public partial class Translator
    {
        private (SyntaxResult<SyntaxNode>, TranslatorState) TranslateStatement(Statement statement, TranslatorState state)
        {
            switch (statement)
            {
                // case FromImportStatement fromImportStatement:
                //     return TranslateStatement_FromImportStatement(fromImportStatement, state);
                case ExpressionStatement expressionStatement:
                    return (TranslateStatement_Expression(expressionStatement, state), state);
                case AssignmentStatement assignmentStatement:
                    return (TranslateStatement_Assignment(assignmentStatement, state), state);
                case ReturnStatement returnStatement:
                    return (TranslateStatement_Return(returnStatement, state), state);
                case IfStatement ifStatement:
                    return (TranslateStatement_If(ifStatement, state), state);
                case WhileStatement whileStatement:
                    return (TranslateStatement_While(whileStatement, state), state);
                case WithStatement withStatement:
                    return (TranslateStatement_With(withStatement, state), state);
                case RaiseStatement raiseStatement:
                    return (TranslateStatement_Raise(raiseStatement, state), state);
                case AssertStatement assertStatement:
                    return (TranslateStatement_Assert(assertStatement, state), state);
                default:
                    return (SyntaxResult<SyntaxNode>.WithError($"// py2cs: Unknown statement type ({statement.NodeName})"), state);
            }
        }

        // private (SyntaxResult<SyntaxNode>, TranslatorState) TranslateStatement_FromImportStatement(FromImportStatement fromImportStatement, TranslatorState state)
        // {
        //     for (int nameIndex = 0; nameIndex < fromImportStatement.Names.Count; nameIndex++)
        //     {
        //         var name = fromImportStatement.Names[nameIndex];
        //         var asName = fromImportStatement.AsNames[nameIndex] ?? name;

        //         state = state.WithVariable(asName, name);
        //     }

        //     return (SyntaxFactory.EmptyStatement(), state);
        // }

        private SyntaxResult<SyntaxNode> TranslateStatement_Expression(ExpressionStatement expressionStatement, TranslatorState state)
        {
            if (expressionStatement.Expression is ConstantExpression constantExpression)
            {
                return SyntaxResult<SyntaxNode>.WithError("/* " + constantExpression.Value + " */");
            }

            var expression = TranslateExpression(expressionStatement.Expression, state);

            if (expression.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(expression.Errors);

            return SyntaxFactory.ExpressionStatement(expression.Syntax);
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_Assignment(AssignmentStatement assignmentStatement, TranslatorState state)
        {
            if (assignmentStatement.Left.Count != 1)
                return SyntaxResult<SyntaxNode>.WithError($"// py2cs: Unsupported assignment left expression count");

            var leftExpression = TranslateExpression(assignmentStatement.Left[0], state);
            var rightExpression = TranslateExpression(assignmentStatement.Right, state);

            if (leftExpression.IsError || rightExpression.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(Enumerable.Concat(leftExpression.Errors, rightExpression.Errors));

            var expression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, leftExpression.Syntax, rightExpression.Syntax);
            return SyntaxFactory.ExpressionStatement(expression);
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_Return(ReturnStatement returnStatement, TranslatorState state)
        {
            ReturnStatementSyntax returnStatementSyntax = SyntaxFactory.ReturnStatement();

            if (returnStatement.Expression != null)
            {
                var expression = TranslateExpression(returnStatement.Expression, state);

                if (expression.IsError)
                    returnStatementSyntax = returnStatementSyntax.WithTrailingTrivia(expression.Errors);
                else
                    returnStatementSyntax = returnStatementSyntax.WithExpression(expression.Syntax);
            }

            return returnStatementSyntax;
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_If(IfStatement ifStatement, TranslatorState state)
        {
            var ifSyntax = TranslateStatement_If(ifStatement.Tests, ifStatement.ElseStatement, state);

            if (ifSyntax.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(ifSyntax.Errors);

            return ifSyntax.Syntax;
        }

        private SyntaxResult<StatementSyntax> TranslateStatement_If(IList<IfStatementTest> tests, Statement elseStatement, TranslatorState state)
        {
            var expression = TranslateExpression(tests[0].Test, state);
            var body = TranslateBlock_Block(tests[0].Body, state);

            if (expression.IsError)
                return SyntaxResult<StatementSyntax>.WithErrors(expression.Errors);

            IfStatementSyntax ifStatementSyntax = SyntaxFactory.IfStatement(expression.Syntax, body);

            if (tests.Count > 1)
            {
                var elseIf = TranslateStatement_If(tests.Skip(1).ToList(), elseStatement, state);

                if (elseIf.IsError)
                    return SyntaxResult<StatementSyntax>.WithErrors(expression.Errors);

                var elseClause = SyntaxFactory.ElseClause(elseIf.Syntax);
                ifStatementSyntax = ifStatementSyntax.WithElse(elseClause);
            }
            else if (elseStatement != null)
            {
                var elseBody = TranslateBlock_Block(elseStatement, state);
                var elseClause = SyntaxFactory.ElseClause(elseBody);
                ifStatementSyntax = ifStatementSyntax.WithElse(elseClause);
            }

            return ifStatementSyntax;
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_While(WhileStatement whileStatement, TranslatorState state)
        {
            var expression = TranslateExpression(whileStatement.Test, state);
            var body = TranslateBlock_Block(whileStatement.Body, state);

            if (expression.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(expression.Errors);

            return SyntaxFactory.WhileStatement(expression.Syntax, body);
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_Raise(RaiseStatement raiseStatement, TranslatorState state)
        {
            var value = TranslateExpression(raiseStatement.ExceptType, state);

            if (value.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(value.Errors);

            return SyntaxFactory.ThrowStatement(value.Syntax);
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_With(WithStatement withStatement, TranslatorState state)
        {
            var contextManager = TranslateExpression(withStatement.ContextManager, state);
            var body = TranslateBlock_Block(withStatement.Body, state);

            if (contextManager.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(contextManager.Errors);

            var usingStatement = SyntaxFactory.UsingStatement(body).WithExpression(contextManager.Syntax);

            if (withStatement.Variable != null)
            {
                if (withStatement.Variable is NameExpression nameExpression)
                {
                    var declarationType = SyntaxFactory.ParseTypeName("object");
                    var variable = SyntaxFactory.VariableDeclarator(nameExpression.Name);
                    var declaration = SyntaxFactory.VariableDeclaration(declarationType, SyntaxFactory.SingletonSeparatedList(variable));
                    usingStatement = usingStatement.WithDeclaration(declaration);
                }
                else
                {
                    return SyntaxResult<SyntaxNode>.WithError($"// py2cs: Unknown with statment variable ({withStatement.Variable})");
                }
            }

            return usingStatement;
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_Assert(AssertStatement assertStatement, TranslatorState state)
        {
            var argumentList = SyntaxFactory.SeparatedList<ArgumentSyntax>();

            var test = TranslateExpression(assertStatement.Test, state);

            if (test.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(test.Errors);

            argumentList = argumentList.Add(SyntaxFactory.Argument(test.Syntax));

            if (assertStatement.Message != null)
            {
                var message = TranslateExpression(assertStatement.Message, state);

                if (message.IsError)
                    return SyntaxResult<SyntaxNode>.WithErrors(message.Errors);

                argumentList = argumentList.Add(SyntaxFactory.Argument(message.Syntax));
            }

            var target = SyntaxFactory.ParseName("System.Diagnostics.Debug.Assert");
            var invocationExpression = SyntaxFactory.InvocationExpression(target, SyntaxFactory.ArgumentList(argumentList));

            return SyntaxFactory.ExpressionStatement(invocationExpression);
        }
    }
}