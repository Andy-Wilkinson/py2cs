using System.Collections.Generic;
using IronPython.Compiler.Ast;

namespace Py2Cs.CodeGraphs
{
    public interface IPythonNode
    {
        Dictionary<string, IPythonNode> Children { get; }
    }
}