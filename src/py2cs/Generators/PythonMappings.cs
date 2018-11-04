using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Py2Cs.Generators
{
    public class PythonMappings
    {
        private string _pythonDir;

        public PythonMappings(string pythonDir)
        {
            TypeMappings = new Dictionary<ITypeSymbol, PythonTypeMapping>();
            MethodMappings = new Dictionary<IMethodSymbol, PythonMethodMapping>();
            PythonEntryPoints = new HashSet<string>();

            _pythonDir = pythonDir;
        }

        public Dictionary<ITypeSymbol, PythonTypeMapping> TypeMappings { get; }
        public Dictionary<IMethodSymbol, PythonMethodMapping> MethodMappings { get; }
        public HashSet<string> PythonEntryPoints { get; }

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
            PythonClassAttribute classAttribute = typeSymbol.GetPythonClassAttribute();

            if (classAttribute != null)
            {
                string pythonFile = typeSymbol.LocatePythonFile(_pythonDir);

                TypeMappings[typeSymbol] = new PythonTypeMapping(pythonFile, classAttribute.ClassName);
                PythonEntryPoints.Add(pythonFile);
            }

            foreach (var memberSymbol in typeSymbol.GetMembers())
            {
                switch (memberSymbol)
                {
                    case IMethodSymbol methodSymbol:
                        var methodAttribute = methodSymbol.GetPythonMethodAttribute();

                        if (methodAttribute != null)
                        {
                            string pythonFile = methodSymbol.LocatePythonFile(_pythonDir);

                            MethodMappings[methodSymbol] = new PythonMethodMapping(pythonFile, methodAttribute.Function, methodAttribute.Generate);
                            PythonEntryPoints.Add(pythonFile);
                        }

                        break;
                    case IPropertySymbol propertySymbol:
                        var propertyAttribute = propertySymbol.GetPythonPropertyAttribute();

                        if (propertyAttribute != null)
                        {
                            string pythonFile = propertySymbol.LocatePythonFile(_pythonDir);

                            if (propertyAttribute.GetterFunction != null)
                                MethodMappings[propertySymbol.GetMethod] = new PythonMethodMapping(pythonFile, propertyAttribute.GetterFunction, propertyAttribute.Generate);
                            if (propertyAttribute.SetterFunction != null)
                                MethodMappings[propertySymbol.SetMethod] = new PythonMethodMapping(pythonFile, propertyAttribute.SetterFunction, propertyAttribute.Generate);

                            PythonEntryPoints.Add(pythonFile);
                        }

                        break;
                }
            }
        }
    }
}