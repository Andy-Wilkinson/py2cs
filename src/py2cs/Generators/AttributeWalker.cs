using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Py2Cs.Generators
{
    public class AttributeWalker
    {
        public AttributeWalker()
        {
            ClassAttributes = new Dictionary<ITypeSymbol, PythonClassAttribute>();
            FieldAttributes = new Dictionary<IFieldSymbol, PythonFieldAttribute>();
            MethodAttributes = new Dictionary<IMethodSymbol, PythonMethodAttribute>();
            OperatorAttributes = new Dictionary<IMethodSymbol, PythonOperatorAttribute>();
            PropertyAttributes = new Dictionary<IPropertySymbol, PythonPropertyAttribute>();
        }

        public Dictionary<ITypeSymbol, PythonClassAttribute> ClassAttributes {get;}
        public Dictionary<IFieldSymbol, PythonFieldAttribute> FieldAttributes {get;}
        public Dictionary<IMethodSymbol, PythonMethodAttribute> MethodAttributes {get;}
        public Dictionary<IMethodSymbol, PythonOperatorAttribute> OperatorAttributes {get;}
        public Dictionary<IPropertySymbol, PythonPropertyAttribute> PropertyAttributes {get;}

        public async Task WalkProject(Project project)
        {
            var compilation = await project.GetCompilationAsync();

            WalkNamespace(compilation.Assembly.GlobalNamespace);
        }

        private void WalkNamespace(INamespaceSymbol namespaceSymbol)
        {
            foreach (var typeMember in namespaceSymbol.GetTypeMembers())
            {
                WalkType(typeMember);
            }

            foreach (var namespaceMember in namespaceSymbol.GetNamespaceMembers())
            {
                WalkNamespace(namespaceMember);
            }
        }

        private void WalkType(INamedTypeSymbol typeSymbol)
        {
            var classAttribute = typeSymbol.GetPythonClassAttribute();

            if (classAttribute != null)
                ClassAttributes.Add(typeSymbol, classAttribute);

            foreach (var memberSymbol in typeSymbol.GetMembers())
            {
                switch (memberSymbol)
                {
                    case IFieldSymbol fieldSymbol:
                        var fieldAttribute = fieldSymbol.GetPythonFieldAttribute();

                        if (fieldAttribute != null)
                            FieldAttributes.Add(fieldSymbol, fieldAttribute);

                        break;
                    case IMethodSymbol methodSymbol:
                        var methodAttribute = methodSymbol.GetPythonMethodAttribute();

                        if (methodAttribute != null)
                            MethodAttributes.Add(methodSymbol, methodAttribute);

                        var operatorAttribute = methodSymbol.GetPythonOperatorAttribute();

                        if (operatorAttribute != null)
                            OperatorAttributes.Add(methodSymbol, operatorAttribute);

                        break;
                    case IPropertySymbol propertySymbol:
                        var propertyAttribute = propertySymbol.GetPythonPropertyAttribute();

                        if (propertyAttribute != null)
                            PropertyAttributes.Add(propertySymbol, propertyAttribute);

                        break;
                }
            }
        }
    }
}