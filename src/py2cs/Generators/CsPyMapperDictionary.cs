using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Py2Cs.CodeGraphs;

namespace Py2Cs.Generators
{
    public class CsPyMapperDictionary
    {
        Dictionary<SpecialType, PythonClass> _specialTypeDictionary = new Dictionary<SpecialType, PythonClass>();
        Dictionary<ITypeSymbol, PythonClass> _classDictionary = new Dictionary<ITypeSymbol, PythonClass>();

        public PythonClass this[ITypeSymbol type]
        {
            get => _classDictionary[type];
            set => _classDictionary[type] = value;
        }

        public PythonClass this[SpecialType type]
        {
            get => _specialTypeDictionary[type];
            set => _specialTypeDictionary[type] = value;
        }

        public Dictionary<SpecialType, PythonClass> SpecialTypeDictionary => _specialTypeDictionary;
        public Dictionary<ITypeSymbol, PythonClass> ClassDictionary => _classDictionary;

        public PythonType GetPythonType(ITypeSymbol type)
        {
            if (type.SpecialType == SpecialType.None)
            {
                if (_classDictionary.TryGetValue(type, out var pythonClass))
                    return new PythonType(pythonClass);
                else
                    return PythonTypes.Unknown;
            }
            else if (type.SpecialType == SpecialType.System_Void)
            {
                return PythonTypes.None;
            }
            else
            {
                if (_specialTypeDictionary.TryGetValue(type.SpecialType, out var pythonClass))
                    return new PythonType(pythonClass);
                else
                    return PythonTypes.Unknown;
            }
        }
    }
}