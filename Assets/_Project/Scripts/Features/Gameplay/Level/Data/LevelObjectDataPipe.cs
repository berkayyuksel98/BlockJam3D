using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelObjectDataPipe : LevelObjectData
{
    public List<LevelObjectDataSimpleCharacter> characters = new List<LevelObjectDataSimpleCharacter>();
    
    public override string GetDisplayName() => $"Pipe ({characters?.Count ?? 0} chars)";
}
