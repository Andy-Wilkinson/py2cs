using System;

[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
sealed class PythonMethodAttribute : Attribute
{
    public PythonMethodAttribute(string function)
    {
        this.Function = function;
    }

    public string Function { get; }
    public string File { get; set; }
    public bool Generate { get; set; }
}