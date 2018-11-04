using System;

[System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
sealed public class PythonFieldAttribute : Attribute
{
    public PythonFieldAttribute(string name)
    {
        this.Name = name;
    }

    public string Name { get; }
    public string File { get; set; }
}