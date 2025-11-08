using UnityEngine;

/// <summary>
/// Conditional field attribute - basit version
/// </summary>
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string fieldName;
    public bool expectedValue;
    
    public ConditionalFieldAttribute(string fieldName, bool expectedValue)
    {
        this.fieldName = fieldName;
        this.expectedValue = expectedValue;
    }
}