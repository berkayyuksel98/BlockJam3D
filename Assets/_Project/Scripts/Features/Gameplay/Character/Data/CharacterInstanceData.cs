using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterInstanceData
{

}

[System.Serializable]
public class SimpleCharacterData : CharacterInstanceData
{
    public ColorType characterColorType;
}

[System.Serializable]
public class BarrelData : CharacterInstanceData
{
    public ColorType characterColorType;
}

[System.Serializable]
public class PipeData : CharacterInstanceData
{
    public List<ColorType> characterColorTypes;
    public PipeDirection pipeDirection;
}

public enum PipeDirection
{
    Forward,
    Back,
    Right,
    Left
}