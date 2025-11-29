using UnityEngine;

public struct OnGridChangedEvent : IGameEvent
{
    public string GridID;

    public OnGridChangedEvent(string gridID)
    {
        GridID = gridID;
    }
}
