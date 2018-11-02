using System;
using System.Collections.Generic;
using Py2Cs.Translators;

namespace Py2Cs.CodeGraphs
{
    public class PythonGraph
    {
        public PythonGraph()
        {
            this.Files = new Dictionary<string, PythonFile>();
        }

        public Dictionary<string, PythonFile> Files { get; }

        public PythonFile AddPythonFile(string filename)
        {
            if (Files.TryGetValue(filename, out PythonFile existingFile))
                return existingFile;

            PythonFile file = PythonFile.CreateFromFile(filename);
            Files[filename] = file;

            return file;
        }
    }
}