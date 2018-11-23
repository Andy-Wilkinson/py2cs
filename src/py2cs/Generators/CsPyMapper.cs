using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Py2Cs.CodeGraphs;

namespace Py2Cs.Generators
{
    public static class CsPyMapper
    {
        public static void MapAttributesToGraph(PythonGraph pythonGraph, AttributeWalker projectAttributes)
        {
            var typeDictionary = new CsPyMapperDictionary();

            // Add core Python types

            AddCorePythonTypes(pythonGraph, typeDictionary);

            // Add custom Python types

            foreach (var entry in projectAttributes.ClassAttributes)
            {
                var target = entry.Key;
                var attribute = entry.Value;

                var pythonClass = pythonGraph.GetOrAddModule(attribute.ModuleName).GetOrAddClass(attribute.ClassName);
                typeDictionary[target] = pythonClass;
            }

            // Add fields

            foreach (var entry in projectAttributes.FieldAttributes)
            {
                var target = entry.Key;
                var attribute = entry.Value;

                var pythonClass = typeDictionary[target.ContainingType];
                var fieldName = attribute.Name ?? target.Name;
                var fieldType = typeDictionary.GetPythonType(target.Type);

                var pythonField = PythonField.Create(fieldName, fieldType);
                pythonClass.Children.Add(fieldName, pythonField);
            }

            // Add methods

            foreach (var entry in projectAttributes.MethodAttributes)
            {
                var target = entry.Key;
                var attribute = entry.Value;

                MapPythonFunction(target, attribute.Function, typeDictionary);
            }

            // Add operators

            foreach (var entry in projectAttributes.OperatorAttributes)
            {
                var target = entry.Key;
                var attribute = entry.Value;

                MapPythonFunction(target, $"@operator.{attribute.Operator}", typeDictionary);
            }

            // Add properties

            foreach (var entry in projectAttributes.PropertyAttributes)
            {
                var target = entry.Key;
                var attribute = entry.Value;

                var pythonClass = typeDictionary[target.ContainingType];
                var propertyName = attribute.Name ?? target.Name;
                var propertyType = typeDictionary.GetPythonType(target.Type);

                var pythonProperty = PythonProperty.Create(propertyName, propertyType);

                if (target.GetMethod != null)
                {
                    var getterName = attribute.GetterFunction ?? propertyName;
                    var parameters = new[] { new PythonParameter("self", new PythonType(pythonClass)) };
                    pythonProperty.GetterFunction = PythonFunction.Create(getterName, propertyType, parameters);
                }

                if (target.SetMethod != null)
                {
                    var setterName = attribute.SetterFunction ?? propertyName;
                    var parameters = new[] { new PythonParameter("self", new PythonType(pythonClass)),
                                             new PythonParameter("value", propertyType) };
                    pythonProperty.SetterFunction = PythonFunction.Create(setterName, PythonTypes.None, parameters);
                }

                pythonClass.Children.Add(propertyName, pythonProperty);
            }
        }

        private static void AddCorePythonTypes(PythonGraph pythonGraph, CsPyMapperDictionary typeDictionary)
        {
            var coreModule = pythonGraph.GetOrAddModule("@python.core");
            typeDictionary[SpecialType.System_String] = coreModule.GetOrAddClass("str");
            typeDictionary[SpecialType.System_Int32] = coreModule.GetOrAddClass("int");
            typeDictionary[SpecialType.System_Double] = coreModule.GetOrAddClass("float");
            typeDictionary[SpecialType.System_Boolean] = coreModule.GetOrAddClass("bool");
        }

        private static void MapPythonFunction(IMethodSymbol target, string functionName, CsPyMapperDictionary typeDictionary)
        {
            var parameters = target.Parameters.Select(p => GetPythonParameter(p, typeDictionary)).ToList();

            PythonClass pythonClass;

            if (target.IsStatic)
            {
                pythonClass = parameters[0].Type.Node as PythonClass;
            }
            else
            {
                pythonClass = typeDictionary[target.ContainingType];
                parameters.Insert(0, new PythonParameter("self", new PythonType(pythonClass)));
            }

            var returnType = typeDictionary.GetPythonType(target.ReturnType);

            var pythonFunction = PythonFunction.Create(functionName, returnType, parameters);
            pythonClass.Children.Add(functionName, pythonFunction);
        }

        private static PythonParameter GetPythonParameter(IParameterSymbol parameter, CsPyMapperDictionary typeDictionary)
        {
            var name = parameter.Name;
            var type = typeDictionary.GetPythonType(parameter.Type);

            return new PythonParameter(name, type);
        }
    }
}