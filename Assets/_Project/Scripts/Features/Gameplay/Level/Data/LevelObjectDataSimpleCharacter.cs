using UnityEngine;

public enum ColorType
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Orange
}

[System.Serializable]
public class LevelObjectDataSimpleCharacter : LevelObjectData
{
    public ColorType colorType;
    
    public override string GetDisplayName() => $"Character ({colorType})";
}
