using UnityEngine;

public struct OnCharacterClickEvent : IGameEvent
{
    public Character character;
    public Vector3 clickPosition;
    public Vector2Int gridPosition;

    public OnCharacterClickEvent(Character character, Vector3 clickPosition, Vector2Int gridPosition)
    {
        this.character = character;
        this.clickPosition = clickPosition;
        this.gridPosition = gridPosition;
    }
}
