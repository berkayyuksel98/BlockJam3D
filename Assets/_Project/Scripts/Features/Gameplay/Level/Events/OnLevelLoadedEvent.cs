public struct OnLevelLoadedEvent : IGameEvent
{
    public string levelName;

    public OnLevelLoadedEvent(string levelName)
    {
        this.levelName = levelName;
    }
}
