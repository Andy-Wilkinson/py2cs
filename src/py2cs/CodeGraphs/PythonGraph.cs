using System;
using System.Collections.Generic;
using Py2Cs.Translators;

namespace Py2Cs.CodeGraphs
{
    public class PythonGraph
    {
        public PythonGraph()
        {
            this.Modules = new Dictionary<string, PythonModule>();
        }

        public Dictionary<string, PythonModule> Modules { get; }

        // public PythonFile AddPythonFile(string filename)
        // {
        //     if (Files.TryGetValue(filename, out PythonFile existingFile))
        //         return existingFile;

        //     PythonFile file = PythonFile.CreateFromFile(filename);
        //     Files[filename] = file;

        //     return file;
        // }

        public PythonModule GetOrAddModule(string moduleName)
        {
            if (Modules.TryGetValue(moduleName, out var pythonModule))
                return pythonModule;

            pythonModule = PythonModule.CreateWithoutFile(moduleName);
            Modules[moduleName] = pythonModule;
            return pythonModule;
        }

        // public PythonFunction GetFunction(string filename, string functionName)
        // {
        //     var pythonFile = Files[filename];
        //     var childNode = pythonFile.GetDescendent(functionName);
        //     return (PythonFunction)childNode;
        // }
    }
}