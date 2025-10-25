using UnityEngine;

/// <summary>
/// Dokunulabilir objeler için interface
/// </summary>
public interface ITouchable
{
    /// <summary>
    /// Objeye dokunulduğunda çağrılır
    /// </summary>
    void OnTouch();
    
    /// <summary>
    /// Bu objenin şu anda dokunulabilir olup olmadığını belirtir
    /// </summary>
    bool CanBeTouched { get; }
}