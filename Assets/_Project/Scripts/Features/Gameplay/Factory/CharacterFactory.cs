using System.Collections.Generic;
using UnityEngine;

public enum CharacterType
{
    Simple,
    Barrel,
    Pipe
}

public class CharacterFactory : MonoBehaviour
{
    [Header("Character Data")]
    public List<CharacterSO> characterSOs;
    
    [Header("Pool Settings")]
    [SerializeField] private int defaultCapacity = 10;
    private void Start()
    {
        InitializeCharacterPools();
    }
    
    /// <summary>
    /// Karakter pool'larını PoolingManager'e ekler
    /// </summary>
    private void InitializeCharacterPools()
    {
        foreach (var characterSO in characterSOs)
        {
            if (characterSO != null && characterSO.characterPrefab != null)
            {
                string poolKey = GetPoolKey(characterSO.characterType);
                
                var poolConfig = new PoolConfig
                {
                    poolKey = poolKey,
                    prefab = characterSO.characterPrefab,
                    defaultCapacity = defaultCapacity,
                    prewarm = true,
                };
                
                PoolingManager.Instance.AddPool(poolConfig);
            }
        }
        
        Debug.Log($"CharacterFactory initialized {characterSOs.Count} character pools");
    }
    
    /// <summary>
    /// Karakter oluşturur
    /// </summary>
    public GameObject CreateCharacter(CharacterType characterType, Vector3 position, Quaternion rotation)
    {
        string poolKey = GetPoolKey(characterType);
        GameObject character = PoolingManager.Instance.Get(poolKey, position, rotation);
        
        if (character != null)
        {
            // Karaktere özel component'i ekle/güncelle
            var characterComponent = character.GetComponent<PoolableCharacter>();
            if (characterComponent == null)
            {
                characterComponent = character.AddComponent<PoolableCharacter>();
            }
            characterComponent.Initialize(characterType, this);
        }
        
        return character;
    }
    
    /// <summary>
    /// Karakteri pool'a geri döndürür
    /// </summary>
    public void ReturnCharacter(GameObject character)
    {
        PoolingManager.Instance.Release(character);
    }
    
    /// <summary>
    /// CharacterType'dan pool key'i oluşturur
    /// </summary>
    private string GetPoolKey(CharacterType characterType)
    {
        return $"Character_{characterType}";
    }
    
    /// <summary>
    /// Pool istatistikleri
    /// </summary>
    [ContextMenu("Print Character Pool Stats")]
    private void PrintPoolStats()
    {
        foreach (var characterSO in characterSOs)
        {
            if (characterSO != null)
            {
                string poolKey = GetPoolKey(characterSO.characterType);
                PoolingManager.Instance.GetPoolStats(poolKey, out int active, out int inactive);
                Debug.Log($"{characterSO.characterType}: Active={active}, Inactive={inactive}");
            }
        }
    }
}
