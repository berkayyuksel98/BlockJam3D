using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Level/Level Information", order = 1)]
public class LevelInformation : ScriptableObject
{
    [Header("Level Basic Info")]
    public string levelName = "New Level";
    public string levelDescription = "";
    public int levelIndex = 0;

    [Header("Grid Configuration")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private int combineZoneLength;
    public float cellSize = 1f;
    
    [Header("Exit Configuration")]
    [SerializeField] private Vector2Int exitPoint = new Vector2Int(0, 0);

    [Header("Level Grid Data")]
    [SerializeField] private List<LevelCellData> levelCells = new List<LevelCellData>();

    #region Properties
    /// <summary>
    /// Grid genişliği
    /// </summary>
    public int GridWidth => gridWidth;
    public int CombineZoneLength => combineZoneLength;
    /// <summary>
    /// Grid yüksekliği
    /// </summary>
    public int GridHeight => gridHeight;

    /// <summary>
    /// Grid boyutu Vector2Int formatında
    /// </summary>
    public Vector2Int GridSize => new Vector2Int(gridWidth, gridHeight);
    #endregion

    #region Grid Management
    public void SetGridSize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Grid size must be positive!");
            return;
        }

        gridWidth = width;
        gridHeight = height;

        CleanupOutOfBoundsCells();
        
        MarkDirty();
    }

    public void SetGridZoneLenght(int length)
    {
        if (length < 0)
        {
            Debug.LogError("Combine Zone Length cannot be negative!");
            return;
        }

        combineZoneLength = length;
        MarkDirty();
    }
    private void CleanupOutOfBoundsCells()
    {
        levelCells.RemoveAll(cell => !IsValidPosition(cell.position));
    }
    #endregion

    #region Cell Operations

    public bool SetObjectAtPosition(Vector2Int position, CharacterInstanceData instanceData)
    {
        if (!IsValidPosition(position))
        {
            Debug.LogWarning($"Position {position} is outside grid bounds ({gridWidth}x{gridHeight})");
            return false;
        }

        // Mevcut cell'i bul veya oluştur
        LevelCellData cellData = GetCellData(position);
        if (cellData == null)
        {
            cellData = new LevelCellData(position);
            levelCells.Add(cellData);
        }

        // Instance data'yı ayarla
        cellData.instanceData = instanceData;

        MarkDirty();
        return true;
    }

    /// <summary>
    /// Belirtilen pozisyondaki objeyi kaldırır
    /// </summary>
    /// <param name="position">Grid pozisyonu</param>
    /// <returns>Kaldırılan karakter verisi</returns>
    public CharacterInstanceData RemoveObjectAtPosition(Vector2Int position)
    {
        LevelCellData cellData = GetCellData(position);
        if (cellData == null || cellData.instanceData == null)
            return null;

        CharacterInstanceData removedData = cellData.instanceData;
        cellData.instanceData = null;

        MarkDirty();
        return removedData;
    }

    public CharacterInstanceData GetObjectAtPosition(Vector2Int position)
    {
        LevelCellData cellData = GetCellData(position);
        return cellData?.instanceData;
    }

    public CharacterInstanceData GetInstanceDataAtPosition(Vector2Int position)
    {
        LevelCellData cellData = GetCellData(position);
        return cellData?.instanceData;
    }

    public bool HasObjectAtPosition(Vector2Int position)
    {
        LevelCellData cellData = GetCellData(position);
        return cellData?.instanceData != null;
    }

    private LevelCellData GetCellData(Vector2Int position)
    {
        return levelCells.Find(cell => cell.position == position);
    }
    #endregion


    #region Validation

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth &&
               position.y >= 0 && position.y < gridHeight;
    }

    public (bool isValid, List<string> errors) ValidateLevel()
    {
        List<string> errors = new List<string>();

        // Grid boyutu kontrolü
        if (gridWidth <= 0 || gridHeight <= 0)
        {
            errors.Add("Grid size must be positive values");
        }

        return (errors.Count == 0, errors);
    }
    #endregion

    #region Utility Methods
    public void ClearAllObjects()
    {
        levelCells.Clear();
        MarkDirty();
    }

    public (Vector2Int size, float cellSize) GetGridBasicInfo()
    {
        return (GridSize, cellSize);
    }
    
    private void MarkDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
    
    /// <summary>
    /// Force serialization update - Unity Editor'da verilerin kaybolmasını önler
    /// </summary>
    public void ForceSerialize()
    {
        MarkDirty();
    }
    
    /// <summary>
    /// Unity validation - Inspector'da değer değiştiğinde çağrılır
    /// </summary>
    private void OnValidate()
    {
        // Grid boyutlarının pozitif olduğundan emin ol
        if (gridWidth < 1) gridWidth = 1;
        if (gridHeight < 1) gridHeight = 1;
        
        // Bounds dışındaki cell'leri temizle
        CleanupOutOfBoundsCells();
    }
    #endregion
}


[System.Serializable]
public class LevelCellData
{
    public Vector2Int position;
    [SerializeReference] public CharacterInstanceData instanceData;

    public LevelCellData(Vector2Int pos)
    {
        position = pos;
        instanceData = null; // Başlangıçta null bırak
    }
}
