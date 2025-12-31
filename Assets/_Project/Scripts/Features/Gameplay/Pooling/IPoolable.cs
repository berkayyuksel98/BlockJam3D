using UnityEngine;

public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
    string GetPoolKey();
}