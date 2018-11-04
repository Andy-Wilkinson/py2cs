using System;

[System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
sealed public class PythonPropertyAttribute : Attribute
{
    public PythonPropertyAttribute(string getterFunction, string setterFunction)
    {
        this.GetterFunction = getterFunction;
        this.SetterFunction = setterFunction;
    }

    public PythonPropertyAttribute(string getterFunction) : this(getterFunction, null)
    {
    }

    public string GetterFunction { get; }
    public string SetterFunction { get; }
    public string File { get; set; }
    public bool Generate { get; set; }
}