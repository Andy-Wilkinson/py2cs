using Microsoft.CodeAnalysis;
using System.IO;
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
                    var moduleName = (string)attribute.ConstructorArguments[0].Value;
                    var className = (string)attribute.ConstructorArguments[1].Value;
                    var file = attribute.GetNamedArgument<string>(nameof(PythonClassAttribute.File), null);
                    var generateMethods = attribute.GetNamedArgument<bool>(nameof(PythonClassAttribute.GenerateMethods), false);

                    return new PythonClassAttribute(moduleName, className) { File = file, GenerateMethods = generateMethods };
                }
            }

            return null;
        }

        public static PythonFieldAttribute GetPythonFieldAttribute(this ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass.Name == nameof(PythonFieldAttribute))
                {
                    var name = (string)attribute.ConstructorArguments[0].Value;

                    return new PythonFieldAttribute(name);
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

        public static PythonPropertyAttribute GetPythonPropertyAttribute(this ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass.Name == nameof(PythonPropertyAttribute))
                {
                    var name = (string)attribute.ConstructorArguments[0].Value;
                    var getterFunction = attribute.GetNamedArgument<string>(nameof(PythonPropertyAttribute.GetterFunction), null);
                    var setterFunction = attribute.GetNamedArgument<string>(nameof(PythonPropertyAttribute.SetterFunction), null);
                    var file = attribute.GetNamedArgument<string>(nameof(PythonPropertyAttribute.File), null);
                    var generate = attribute.GetNamedArgument<bool>(nameof(PythonPropertyAttribute.Generate), false);

                    return new PythonPropertyAttribute(name)
                    {
                        GetterFunction = getterFunction,
                        SetterFunction = setterFunction,
                        File = file,
                        Generate = generate
                    };
                }
            }

            return null;
        }

        public static PythonOperatorAttribute GetPythonOperatorAttribute(this ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass.Name == nameof(PythonOperatorAttribute))
                {
                    var op = (PythonOperator)attribute.ConstructorArguments[0].Value;

                    return new PythonOperatorAttribute(op);
                }
            }

            return null;
        }

        public static string GetPythonFile(this ISymbol symbol)
        {
            PythonMethodAttribute pythonMethodAttribute = symbol.GetPythonMethodAttribute();

            if (pythonMethodAttribute != null && pythonMethodAttribute.File != null)
                return pythonMethodAttribute.File;

            while (symbol != null)
            {
                PythonClassAttribute pythonClassAttribute = symbol.GetPythonClassAttribute();

                if (pythonClassAttribute != null && pythonClassAttribute.File != null)
                    return pythonClassAttribute.File;

                symbol = symbol.ContainingType;
            }

            return null;
        }

        public static string LocatePythonFile(this ISymbol symbol, string pythonDir)
        {
            string relativeFilename = symbol.GetPythonFile();

            var sourceFile = symbol.Locations[0].SourceTree.FilePath;
            var sourceLocalFilename = Path.Combine(Path.GetDirectoryName(sourceFile), relativeFilename);

            if (File.Exists(sourceLocalFilename))
                return sourceLocalFilename;

            var pythonFolderFilename = Path.Combine(pythonDir, relativeFilename);

            if (File.Exists(pythonFolderFilename))
                return pythonFolderFilename;

            throw new FileNotFoundException();
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