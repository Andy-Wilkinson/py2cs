using Microsoft.CodeAnalysis;
using System.Linq;

namespace Py2Cs.Generators
{
    public static class RoslynHelpers
    {
        public static PythonClassAttribute GetPythonClassAttribute(this ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass.Name == nameof(PythonClassAttribute))
                {
                    var className = (string)attribute.ConstructorArguments[0].Value;
                    var file = attribute.GetNamedArgument<string>(nameof(PythonClassAttribute.File), null);
                    var generateMethods = attribute.GetNamedArgument<bool>(nameof(PythonClassAttribute.GenerateMethods), false);

                    return new PythonClassAttribute(className) { File = file, GenerateMethods = generateMethods };
                }
            }

            return null;
        }

        public static PythonMethodAttribute GetPythonMethodAttribute(this ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass.Name == nameof(PythonMethodAttribute))
                {
                    var function = (string)attribute.ConstructorArguments[0].Value;
                    var file = attribute.GetNamedArgument<string>(nameof(PythonMethodAttribute.File), null);
                    var generate = attribute.GetNamedArgument<bool>(nameof(PythonMethodAttribute.Generate), false);

                    return new PythonMethodAttribute(function) { File = file, Generate = generate };
                }
            }

            return null;
        }

        public static T GetNamedArgument<T>(this AttributeData attribute, string name, T defaultValue)
        {
            var argument = attribute.NamedArguments.FirstOrDefault(a => a.Key == name);

            if (argument.Key != null)
                return (T)argument.Value.Value;
            else
                return defaultValue;
        }
    }
}