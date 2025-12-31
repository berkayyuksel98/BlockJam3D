using System;
using UnityEngine;

[Serializable]
public class PoolConfig
{
    [Header("Pool Settings")]
    public string poolKey;
    public GameObject prefab;
    public int defaultCapacity = 10;
    public bool prewarm = true;
}