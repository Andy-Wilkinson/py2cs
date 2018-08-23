using Microsoft.CodeAnalysis;
using System.Linq;

namespace Py2Cs.Generators
{
    public static class RoslynHelpers
    {
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