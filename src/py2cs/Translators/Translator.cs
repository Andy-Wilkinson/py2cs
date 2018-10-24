using System;
using System.Collections.Immutable;
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
            var state = new TranslatorState(ImmutableDictionary<string, string>.Empty);

            (var children, _) = TranslateBlock_Members(ast.Body, state);

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

        public MethodDeclarationSyntax TranslateFunctionDefinition(PythonNode node)
        {
            if (node.NodeType != PythonNodeType.Function)
                throw new ArgumentException();

            var function = (FunctionDefinition)node.Statement;

            var returnType = SyntaxFactory.ParseTypeName("void");
            var methodDeclaration = SyntaxFactory.MethodDeclaration(returnType, function.Name)
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                    .WithBody(SyntaxFactory.Block());

            foreach (Parameter pyParameter in function.Parameters)
            {
                var parameterSyntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(pyParameter.Name))
                       .WithType(SyntaxFactory.ParseTypeName("object"));

                if (pyParameter.DefaultValue != null)
                {
                    var parameterExpression = TranslateExpression(pyParameter.DefaultValue, node.State);

                    if (parameterExpression.IsError)
                        parameterSyntax = parameterSyntax.WithTrailingTrivia(parameterExpression.Errors);
                    else
                        parameterSyntax = parameterSyntax.WithDefault(SyntaxFactory.EqualsValueClause(parameterExpression.Syntax));
                }

                methodDeclaration = methodDeclaration.AddParameterListParameters(parameterSyntax);
            }

            return methodDeclaration;
        }

        public BlockSyntax TranslateFunctionBody(PythonNode node)
        {
            if (node.NodeType != PythonNodeType.Function)
                throw new ArgumentException();

            var function = (FunctionDefinition)node.Statement;
            var body = TranslateBlock_Block(function.Body, node.State);

            return body;
        }
    }
}