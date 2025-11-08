using UnityEngine;

/// <summary>
/// Pool edilebilir objeler için interface
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Pool'dan alındığında çağrılır
    /// </summary>
    void OnSpawned();
    
    /// <summary>
    /// Pool'a geri döndürüldüğünde çağrılır
    /// </summary>
    void OnDespawned();
    
    /// <summary>
    /// Pool key'ini döndürür (hangi pool'a ait olduğu)
    /// </summary>
    string GetPoolKey();
}