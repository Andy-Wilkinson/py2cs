using System;

[System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed public class PythonClassAttribute : Attribute
{
    public PythonClassAttribute(string moduleName, string className)
    {
        this.ModuleName = moduleName;
        this.ClassName = className;
    }

    public string ModuleName { get; }
    public string ClassName { get; }
    public string File { get; set; }
    public bool GenerateMethods { get; set; }
}