using UnityEngine;

public class PoolableCharacter : MonoBehaviour
{
    private CharacterType characterType;
    private CharacterFactory factory;
    
    public CharacterType CharacterType => characterType;
    
    public void Initialize(CharacterType type, CharacterFactory characterFactory)
    {
        characterType = type;
        factory = characterFactory;
    }
    
    /// <summary>
    /// Pool'dan alındığında çağrılır
    /// </summary>
    public virtual void OnSpawned()
    {
        // Override edilebilir - karakter spawn olduğunda yapılacaklar
    }
    
    /// <summary>
    /// Pool'a geri döndürüldüğünde çağrılır
    /// </summary>
    public virtual void OnDespawned()
    {
        // Override edilebilir - karakter despawn olduğunda yapılacaklar
        ResetCharacter();
    }
    
    /// <summary>
    /// Karakteri pool'a geri döndürür
    /// </summary>
    public void ReturnToPool()
    {
        if (factory != null)
        {
            factory.ReturnCharacter(gameObject);
        }
    }
    
    /// <summary>
    /// Karakter durumunu sıfırlar
    /// </summary>
    protected virtual void ResetCharacter()
    {
        // Transform sıfırlama
        transform.localScale = Vector3.one;
        
        // Rigidbody sıfırlama (varsa)
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Diğer component'leri sıfırla
    }
}