using System;

[System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed public class PythonPropertyAttribute : Attribute
{
    public PythonPropertyAttribute(string name = null)
    {
        this.Name = name;
    }

    public string Name { get; }
    public string GetterFunction { get; set; }
    public string SetterFunction { get; set; }
    public string File { get; set; }
    public bool Generate { get; set; }
}