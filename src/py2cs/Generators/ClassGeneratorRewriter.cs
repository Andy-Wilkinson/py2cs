using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Py2Cs.Translators;

namespace Py2Cs.Generators
{
    public class ClassGeneratorRewriter : CSharpSyntaxRewriter
    {
        private readonly Generator _generator;
        private readonly SemanticModel _semanticModel;
        private readonly PythonCache _pythonCache;

        public ClassGeneratorRewriter(Generator generator, SemanticModel semanticModel, PythonCache pythonCache)
        {
            this._generator = generator;
            this._semanticModel = semanticModel;
            this._pythonCache = pythonCache;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var classSymbol = _semanticModel.GetDeclaredSymbol(node);
            var existingMethods = new List<string>();

            foreach (var member in classSymbol.GetMembers())
            {
                var pythonMethodAttribute = member.GetPythonMethodAttribute();

                if (pythonMethodAttribute != null)
                {
                    existingMethods.Add(pythonMethodAttribute.Function);
                }
            }

            var pythonClassAttribute = classSymbol.GetPythonClassAttribute();

            if (pythonClassAttribute != null && pythonClassAttribute.GenerateMethods == true)
            {
                var rootNode = _pythonCache.GetPythonFile(node, pythonClassAttribute.File);
                var classNode = rootNode.GetDescendent(pythonClassAttribute.ClassName);

                foreach (PythonNode childNode in classNode.Children)
                {
                    if (childNode.NodeType == PythonNodeType.Function)
                    {
                        string functionName = $"{pythonClassAttribute.ClassName}.{childNode.Name}";

                        if (!existingMethods.Contains(functionName))
                        {
                            var newMethod = _generator.Translator.TranslateFunctionDefinition(childNode);

                            var attributeMethodNameArg = SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(functionName)));
                            var attributeFileArg = SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(pythonClassAttribute.File))).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("File")));
                            var attributeGenerateArg = SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)).WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName(nameof(PythonMethodAttribute.Generate))));
                            var attributeArgList = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new AttributeArgumentSyntax[] { attributeMethodNameArg, attributeFileArg, attributeGenerateArg }));
                            var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName(nameof(PythonMethodAttribute)), attributeArgList);
                            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));
                            newMethod = newMethod.WithAttributeLists(SyntaxFactory.SingletonList(attributeList));

                            node = node.AddMembers(newMethod);
                        }
                    }
                }
            }

            return node;
        }
    }
}