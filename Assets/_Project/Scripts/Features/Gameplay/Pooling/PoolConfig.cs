using System;
using UnityEngine;

/// <summary>
/// Pool edilecek objelerin konfigürasyonu
/// </summary>
[Serializable]
public class PoolConfig
{
    [Header("Pool Settings")]
    public string poolKey;
    public GameObject prefab;
    public int defaultCapacity = 10;
    public bool prewarm = true;
}