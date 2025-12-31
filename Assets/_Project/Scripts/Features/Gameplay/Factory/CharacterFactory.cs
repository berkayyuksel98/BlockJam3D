using System.Collections.Generic;
using UnityEngine;

public enum CharacterType
{
    Simple,
    Barrel,
    Pipe
}

public class CharacterFactory : Singleton<CharacterFactory>
{
    [Header("Character Data")]
    public List<CharacterSO> characterSOs;

    [Header("Pool Settings")]
    [SerializeField] private int defaultCapacity = 10;
    private void Start()
    {
        InitializeCharacterPools();
    }

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

    public GameObject CreateCharacter(CharacterInstanceData instanceData, Vector3 position, Quaternion rotation, string gridID)
    {
        if (instanceData is SimpleCharacterData)
        {
            return CreateSimpleCharacter((SimpleCharacterData)instanceData, position, rotation, gridID);
        }
        else if (instanceData is BarrelData)
        {
            return CreateBarrelCharacter((BarrelData)instanceData, position, rotation,gridID);
        }
        else if (instanceData is PipeData)
        {
            return CreatePipeCharacter((PipeData)instanceData, position, rotation,gridID);
        }
        else
        {
            Debug.LogError("Unknown CharacterInstanceData type");
            return null;
        }
    }

    private GameObject CreateSimpleCharacter(SimpleCharacterData data, Vector3 position, Quaternion rotation, string gridID)
    {
        string poolKey = GetPoolKey(CharacterType.Simple);
        GameObject character = PoolingManager.Instance.Get(poolKey, position, rotation);
        character.GetComponent<SimpleCharacter>().Initialize(data.characterColorType, gridID);
        return character;
    }
    private GameObject CreateBarrelCharacter(BarrelData data, Vector3 position, Quaternion rotation,string gridID)
    {
        string poolKey = GetPoolKey(CharacterType.Barrel);
        GameObject character = PoolingManager.Instance.Get(poolKey, position, rotation);
        character.GetComponent<BarrelCharacter>().Initialize(data.characterColorType, gridID);
        return character;
    }
    private GameObject CreatePipeCharacter(PipeData data, Vector3 position, Quaternion rotation, string gridID)
    {
        string poolKey = GetPoolKey(CharacterType.Pipe);
        GameObject character = PoolingManager.Instance.Get(poolKey, position, rotation);
        character.GetComponent<PipeCharacter>().Initialize(data.characterColorTypes,gridID,data.pipeDirection);
        return character;
    }

    public void ReturnCharacter(GameObject character)
    {
        PoolingManager.Instance.Release(character);
    }

    private string GetPoolKey(CharacterType characterType)
    {
        return $"Character_{characterType}";
    }

}
