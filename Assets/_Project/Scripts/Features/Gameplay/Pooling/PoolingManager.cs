using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Genel pooling sistemi - her türlü prefab için kullanılabilir
/// </summary>
public class PoolingManager : Singleton<PoolingManager>
{
    [Header("Pool Configurations")]
    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // String-based pool system
    private Dictionary<string, ObjectPool<GameObject>> pools;
    
    private void Awake()
    {
        InitializePools();
    }
    
    /// <summary>
    /// Pool'ları başlatır
    /// </summary>
    private void InitializePools()
    {
        pools = new Dictionary<string, ObjectPool<GameObject>>();
        
        foreach (var config in poolConfigs)
        {
            if (config.prefab == null || string.IsNullOrEmpty(config.poolKey))
            {
                Debug.LogWarning($"Invalid pool config: {config.poolKey}");
                continue;
            }
            
            CreatePool(config);
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"PoolingManager initialized with {pools.Count} pools");
        }
    }
    
    /// <summary>
    /// Yeni pool oluşturur
    /// </summary>
    private void CreatePool(PoolConfig config)
    {
        var pool = new ObjectPool<GameObject>(
            createFunc: () => CreatePooledObject(config),
            actionOnGet: (obj) => OnGetFromPool(obj, config.poolKey),
            actionOnRelease: (obj) => OnReleaseToPool(obj, config.poolKey),
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: config.defaultCapacity
        );
        
        pools[config.poolKey] = pool;
        
        // Prewarm
        if (config.prewarm)
        {
            PrewarmPool(config.poolKey, config.defaultCapacity);
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"Pool created: {config.poolKey} (capacity: {config.defaultCapacity})");
        }
    }
    
    /// <summary>
    /// Pool'u önceden ısıtır
    /// </summary>
    private void PrewarmPool(string poolKey, int count)
    {
        if (!pools.TryGetValue(poolKey, out var pool)) return;
        
        List<GameObject> prewarmObjects = new List<GameObject>();
        
        for (int i = 0; i < count; i++)
        {
            GameObject obj = pool.Get();
            prewarmObjects.Add(obj);
        }
        
        foreach (var obj in prewarmObjects)
        {
            pool.Release(obj);
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Pool'dan obje al
    /// </summary>
    public GameObject Get(string poolKey, Vector3 position = default, Quaternion rotation = default)
    {
        if (!pools.TryGetValue(poolKey, out var pool))
        {
            Debug.LogError($"Pool not found: {poolKey}");
            return null;
        }
        
        GameObject obj = pool.Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        
        if (enableDebugLogs)
        {
            Debug.Log($"Object spawned from pool: {poolKey}");
        }
        
        return obj;
    }
    
    /// <summary>
    /// Objeyi pool'a geri döndür
    /// </summary>
    public void Release(GameObject obj)
    {
        var poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            string poolKey = poolable.GetPoolKey();
            Release(poolKey, obj);
        }
        else
        {
            Debug.LogWarning($"Object {obj.name} is not poolable!");
            Destroy(obj);
        }
    }
    
    /// <summary>
    /// Objeyi belirli pool'a geri döndür
    /// </summary>
    public void Release(string poolKey, GameObject obj)
    {
        if (!pools.TryGetValue(poolKey, out var pool))
        {
            Debug.LogError($"Pool not found: {poolKey}");
            Destroy(obj);
            return;
        }
        
        pool.Release(obj);
        
        if (enableDebugLogs)
        {
            Debug.Log($"Object returned to pool: {poolKey}");
        }
    }
    
    /// <summary>
    /// Runtime'da yeni pool ekle
    /// </summary>
    public void AddPool(PoolConfig config)
    {
        if (pools.ContainsKey(config.poolKey))
        {
            Debug.LogWarning($"Pool already exists: {config.poolKey}");
            return;
        }
        
        CreatePool(config);
    }
    
    /// <summary>
    /// Pool istatistikleri
    /// </summary>
    public void GetPoolStats(string poolKey, out int activeCount, out int inactiveCount)
    {
        activeCount = 0;
        inactiveCount = 0;
        
        if (pools.TryGetValue(poolKey, out var pool))
        {
            // Unity'nin ObjectPool'u CountAll ve CountActive property'leri sağlar
            activeCount = pool.CountActive;
            inactiveCount = pool.CountInactive;
        }
    }
    
    #endregion
    
    #region Pool Callbacks
    
    private GameObject CreatePooledObject(PoolConfig config)
    {
        GameObject obj = Instantiate(config.prefab, transform);
        
        // IPoolable component kontrolü
        var poolable = obj.GetComponent<IPoolable>();
        if (poolable == null)
        {
            // Generic poolable component ekle
            var genericPoolable = obj.AddComponent<PoolableObject>();
            genericPoolable.Initialize(config.poolKey);
        }
        
        return obj;
    }
    
    private void OnGetFromPool(GameObject obj, string poolKey)
    {
        obj.SetActive(true);
        
        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawned();
    }
    
    private void OnReleaseToPool(GameObject obj, string poolKey)
    {
        obj.SetActive(false);
        
        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnDespawned();
    }
    
    private void OnDestroyPoolObject(GameObject obj)
    {
        if (obj != null)
        {
            Destroy(obj);
        }
    }
    
    #endregion
    
    #region Debug
    
    [ContextMenu("Print Pool Stats")]
    private void PrintPoolStats()
    {
        foreach (var kvp in pools)
        {
            GetPoolStats(kvp.Key, out int active, out int inactive);
            Debug.Log($"Pool: {kvp.Key} | Active: {active} | Inactive: {inactive}");
        }
    }
    
    #endregion
}