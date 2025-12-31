using UnityEngine;

public class PoolableObject : MonoBehaviour, IPoolable
{
    [SerializeField] private string poolKey;
    
    public void Initialize(string key)
    {
        poolKey = key;
    }
    
    public string GetPoolKey()
    {
        return poolKey;
    }
    
    public virtual void OnSpawned()
    {
    }
    
    public virtual void OnDespawned()
    {
        ResetObject();
    }
    
    public void ReturnToPool()
    {
        PoolingManager.Instance.Release(gameObject);
    }
    
    protected virtual void ResetObject()
    {
        // Transform reset
        transform.localScale = Vector3.one;
        
        // Rigidbody reset (varsa)
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Particle system reset (varsa)
        var particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
            ps.Clear();
        }
    }
}