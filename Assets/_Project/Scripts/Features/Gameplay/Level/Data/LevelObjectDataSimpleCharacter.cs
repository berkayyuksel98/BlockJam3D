using UnityEngine;

[System.Serializable]
public class LevelObjectDataSimpleCharacter : LevelObjectData
{
    public ColorType colorType;
    
    public override string GetDisplayName() => $"Character ({colorType})";
}
