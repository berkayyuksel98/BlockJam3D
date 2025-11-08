using UnityEngine;

[System.Serializable]
public abstract class LevelObjectData
{
    public virtual string GetDisplayName() => GetType().Name;
}
