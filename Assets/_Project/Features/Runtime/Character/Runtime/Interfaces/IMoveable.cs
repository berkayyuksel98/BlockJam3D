using UnityEngine;

/// <summary>
/// Hareket edebilen objeler için interface
/// </summary>
public interface IMoveable
{
    /// <summary>
    /// Belirtilen pozisyona hareket et
    /// </summary>
    /// <param name="targetPosition">Hedef pozisyon</param>
    void MoveTo(Vector2Int targetPosition);
    
    /// <summary>
    /// Belirtilen yöne hareket edebilir mi kontrol eder
    /// </summary>
    /// <param name="direction">Hareket yönü</param>
    /// <returns>Hareket edebilirse true</returns>
    bool CanMoveInDirection(Vector2Int direction);
    
    /// <summary>
    /// Karakterin şu anki grid pozisyonu
    /// </summary>
    Vector2Int GridPosition { get; set; }
    
    /// <summary>
    /// Hareket animasyonu tamamlandığında çağrılacak event
    /// </summary>
    System.Action OnMoveCompleted { get; set; }
}