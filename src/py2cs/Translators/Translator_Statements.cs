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
        private SyntaxResult<SyntaxNode> TranslateStatement(Statement statement)
        {
            switch (statement)
            {
                case ImportStatement importStatement:
                    return TranslateStatement_ImportStatement(importStatement);
                case ClassDefinition classDefinition:
                    return TranslateStatement_Class(classDefinition);
                case FunctionDefinition functionDefinition:
                    return TranslateStatement_Function(functionDefinition);
                case ExpressionStatement expressionStatement:
                    return TranslateStatement_Expression(expressionStatement);
                case AssignmentStatement assignmentStatement:
                    return TranslateStatement_Assignment(assignmentStatement);
                case ReturnStatement returnStatement:
                    return TranslateStatement_Return(returnStatement);
                case IfStatement ifStatement:
                    return TranslateStatement_If(ifStatement);
                default:
                    return SyntaxResult<SyntaxNode>.WithError($"// py2cs: Unknown statement type ({statement.NodeName})");
            }
        }

        private UsingDirectiveSyntax TranslateStatement_ImportStatement(ImportStatement importStatement)
        {
            var importNames = string.Join(", ", importStatement.Names.Select(name => name.MakeString()));
            var importAsNames = string.Join(", ", importStatement.AsNames);

            var nameSyntax = SyntaxFactory.ParseName(importNames);

            return SyntaxFactory.UsingDirective(nameSyntax);
        }

        private ClassDeclarationSyntax TranslateStatement_Class(ClassDefinition pyClassDefinition)
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(pyClassDefinition.Name);

            var children = TranslateBlock_Members(pyClassDefinition.Body);

            if (children.IsError)
            {
                return classDeclaration.WithTrailingTrivia(children.Errors);
            }

            foreach (var child in children.Syntax)
            {
                switch (child)
                {
                    case MemberDeclarationSyntax member:
                        classDeclaration = classDeclaration.AddMembers(member);
                        break;
                }
            }

            return classDeclaration;
        }

        private MemberDeclarationSyntax TranslateStatement_Function(FunctionDefinition pyFunctionDefinition)
        {
            var returnType = SyntaxFactory.ParseTypeName("void");
            var methodDeclaration = SyntaxFactory.MethodDeclaration(returnType, pyFunctionDefinition.Name);

            foreach (Parameter pyParameter in pyFunctionDefinition.Parameters)
            {
                var parameterSyntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(pyParameter.Name))
                        .WithType(SyntaxFactory.ParseTypeName("object"));

                if (pyParameter.DefaultValue != null)
                {
                    var parameterExpression = TranslateExpression(pyParameter.DefaultValue);

                    if (parameterExpression.IsError)
                        parameterSyntax.WithTrailingTrivia(parameterExpression.Errors);
                    else
                        parameterSyntax = parameterSyntax.WithDefault(SyntaxFactory.EqualsValueClause(parameterExpression.Syntax));
                }

                methodDeclaration = methodDeclaration.AddParameterListParameters(parameterSyntax);
            }

            BlockSyntax body = TranslateBlock_Block(pyFunctionDefinition.Body);

            methodDeclaration = methodDeclaration.WithBody(SyntaxFactory.Block(body));

            return methodDeclaration;
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_Expression(ExpressionStatement expressionStatement)
        {
            var expression = TranslateExpression(expressionStatement.Expression);

            if (expression.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(expression.Errors);
            else
                return SyntaxFactory.ExpressionStatement(expression.Syntax);
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_Assignment(AssignmentStatement assignmentStatement)
        {
            if (assignmentStatement.Left.Count != 1)
                return SyntaxResult<SyntaxNode>.WithError($"// py2cs: Unsupported assignment left expression count");

            var leftExpression = TranslateExpression(assignmentStatement.Left[0]);
            var rightExpression = TranslateExpression(assignmentStatement.Right);

            if (leftExpression.IsError || rightExpression.IsError)
                return SyntaxResult<SyntaxNode>.WithErrors(Enumerable.Concat(leftExpression.Errors, rightExpression.Errors));

            var expression = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, leftExpression.Syntax, rightExpression.Syntax);
            return SyntaxFactory.ExpressionStatement(expression);
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_Return(ReturnStatement returnStatement)
        {
            ReturnStatementSyntax returnStatementSyntax = SyntaxFactory.ReturnStatement();

            if (returnStatement.Expression != null)
            {
                var expression = TranslateExpression(returnStatement.Expression);

                if (expression.IsError)
                    returnStatementSyntax = returnStatementSyntax.WithTrailingTrivia(expression.Errors);
                else
                    returnStatementSyntax = returnStatementSyntax.WithExpression(expression.Syntax);
            }

            return returnStatementSyntax;
        }

        private SyntaxResult<SyntaxNode> TranslateStatement_If(IfStatement ifStatement)
        {
            if (ifStatement.Tests.Count != 1 || ifStatement.ElseStatement != null)
                return SyntaxResult<SyntaxNode>.WithError($"// py2cs: Unsupported if statement structure");

            IfStatementSyntax ifStatementSyntax = null;

            foreach (IfStatementTest ifStatementTest in ifStatement.Tests)
            {
                var expression = TranslateExpression(ifStatementTest.Test);
                var body = TranslateBlock_Block(ifStatementTest.Body);

                if (expression.IsError)
                    return SyntaxResult<SyntaxNode>.WithErrors(expression.Errors);

                ifStatementSyntax = SyntaxFactory.IfStatement(expression.Syntax, body);
            }

            return ifStatementSyntax;
        }
    }
}