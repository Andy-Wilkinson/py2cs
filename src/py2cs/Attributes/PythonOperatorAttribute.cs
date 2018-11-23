using System;

[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
sealed public class PythonOperatorAttribute : Attribute
{
    public PythonOperatorAttribute(PythonOperator op)
    {
        this.Operator = op;
    }

    public PythonOperator Operator { get; }
}