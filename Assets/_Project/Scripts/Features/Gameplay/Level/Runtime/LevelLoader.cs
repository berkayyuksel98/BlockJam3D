using UnityEngine;

public class LevelLoader : Singleton<LevelLoader>
{
    private CharacterFactory characterFactory;
    private GridSystem gameplayGrid, combineZoneGrid;
    private LevelInformation currentLevel;
    [SerializeField] private float combineZonePositionDelta = 2f;
    private void Start()
    {
        // CharacterFactory referansını otomatik bul
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

        SetCombineZoneGrid(levelInformation);

        gameplayGrid = GridManager.Instance.CreateGrid("gameplay");

        bool nullCheck = NullCheckControl();
        if (!nullCheck)
        {
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

        // 3. Tüm karakterler spawn edildikten sonra level loaded event'i yayınla
        EventBus.Instance.Publish(new OnLevelLoadedEvent(levelInformation.name));

        Debug.Log($"Level loaded: {levelInformation.name} ({levelInformation.GridWidth}x{levelInformation.GridHeight})");
    }

    private bool NullCheckControl()
    {
        if (gameplayGrid == null)
        {
            Debug.LogError($"Failed to create gameplay grid with ID 'gameplay'!");
            return false;
        }

        if (combineZoneGrid == null)
        {
            Debug.LogError("Failed to create combine zone grid!");
            return false;
        }

        if (characterFactory == null)
        {
            Debug.LogError("CharacterFactory reference is missing!");
            return false;
        }

        return true;
    }

    private void SetCombineZoneGrid(LevelInformation levelInformation)
    {
        combineZoneGrid = GridManager.Instance.CreateGrid("combinezone", new Vector3(0, 0, -combineZonePositionDelta));
        combineZoneGrid.SetupGrid(levelInformation.CombineZoneLength, 1, GridCenter.Centered);
    }

    private void SetupGrid(LevelInformation levelInfo)
    {
        // Grid boyutlarını ayarla
        gameplayGrid.SetupGrid(levelInfo.GridWidth, levelInfo.GridHeight, GridCenter.BottomCentered);
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
                    Vector3 worldPosition = gameplayGrid.GetWorldPosition(gridPosition);

                    // Karakteri spawn et
                    GameObject spawnedCharacter = characterFactory.CreateCharacter(
                        instanceData,
                        worldPosition,
                        Quaternion.identity,
                        "gameplay"
                    );

                    if (spawnedCharacter != null)
                    {
                        // Grid pozisyonunu karaktere set et
                        Character character = spawnedCharacter.GetComponent<Character>();
                        if (character != null)
                        {
                            character.SetGridPosition(gridPosition, "gameplay");
                        }

                        // Grid'e karakter objesini kaydet
                        gameplayGrid.SetObjectAtPosition(gridPosition, spawnedCharacter);
                        spawnedCount++;
                    }
                }
            }
        }

        Debug.Log($"Spawned {spawnedCount} characters");
    }

    public void UnloadCurrentLevel()
    {
        if (gameplayGrid != null)
        {
            gameplayGrid.ClearGrid(characterFactory);
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
        if (gameplayGrid != null)
        {
            return gameplayGrid.GetObjectAtPosition(gridPosition);
        }
        return null;
    }
}
