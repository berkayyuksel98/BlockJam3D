using UnityEngine;

public class LevelLoader : Singleton<LevelLoader>
{
    private GridManager gridManager;
    private CharacterFactory characterFactory;
    
    private LevelInformation currentLevel;
    
    private void Start()
    {
        // GridManager ve CharacterFactory referanslarını otomatik bul
        if (gridManager == null)
            gridManager = GridManager.Instance;
            
        if (characterFactory == null)
            characterFactory = CharacterFactory.Instance;
    }
    
    public void LoadLevel(LevelInformation levelInformation)
    {
        if (levelInformation == null)
        {
            Debug.LogError("LevelInformation is null!");
            return;
        }
        
        if (gridManager == null)
        {
            Debug.LogError("GridManager reference is missing!");
            return;
        }
        
        if (characterFactory == null)
        {
            Debug.LogError("CharacterFactory reference is missing!");
            return;
        }
        
        // Önceki level'i temizle
        UnloadCurrentLevel();
        
        // Yeni level'i kaydet
        currentLevel = levelInformation;
        
        // 1. Önce gridi ayarlayalım
        SetupGrid(levelInformation);
        
        // 2. Ardından gridlerdeki pozisyonları alıp karakterleri spawn edelim
        SpawnCharacters(levelInformation);
        
        Debug.Log($"Level loaded: {levelInformation.name} ({levelInformation.GridWidth}x{levelInformation.GridHeight})");
    }
    
    private void SetupGrid(LevelInformation levelInfo)
    {
        // Grid boyutlarını ayarla
        gridManager.SetupGrid(levelInfo.GridWidth, levelInfo.GridHeight);
        Debug.Log($"Grid setup: {levelInfo.GridWidth}x{levelInfo.GridHeight}");
    }
    
    private void SpawnCharacters(LevelInformation levelInfo)
    {
        int spawnedCount = 0;
        
        // Grid'deki her pozisyonu kontrol et
        for (int x = 0; x < levelInfo.GridWidth; x++)
        {
            for (int y = 0; y < levelInfo.GridHeight; y++)
            {
                Vector2Int gridPosition = new Vector2Int(x, y);
                
                // Bu pozisyonda karakter var mı?
                CharacterInstanceData instanceData = levelInfo.GetObjectAtPosition(gridPosition);
                
                if (instanceData != null)
                {
                    // Dünya pozisyonunu hesapla
                    Vector3 worldPosition = gridManager.GetWorldPosition(gridPosition);
                    
                    // Karakteri spawn et
                    GameObject spawnedCharacter = characterFactory.CreateCharacter(
                        instanceData, 
                        worldPosition, 
                        Quaternion.identity
                    );
                    
                    if (spawnedCharacter != null)
                    {
                        // Grid'e karakter objesini kaydet
                        gridManager.SetObjectAtPosition(gridPosition, spawnedCharacter);
                        spawnedCount++;
                    }
                }
            }
        }
        
        Debug.Log($"Spawned {spawnedCount} characters");
    }
    
    public void UnloadCurrentLevel()
    {
        if (gridManager != null)
        {
            gridManager.ClearGrid(characterFactory);
        }
        
        currentLevel = null;
        Debug.Log("Current level unloaded");
    }
    
    public LevelInformation GetCurrentLevel()
    {
        return currentLevel;
    }
    
    public GameObject GetCharacterAtPosition(Vector2Int gridPosition)
    {
        if (gridManager != null)
        {
            return gridManager.GetObjectAtPosition(gridPosition);
        }
        return null;
    }
}
