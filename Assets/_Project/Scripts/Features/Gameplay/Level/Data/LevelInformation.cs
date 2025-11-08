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
    public float cellSize = 1f;

    [Header("Level Grid Data")]
    [SerializeField] private List<LevelCellData> levelCells = new List<LevelCellData>();

    #region Properties
    /// <summary>
    /// Grid genişliği
    /// </summary>
    public int GridWidth => gridWidth;

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

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void CleanupOutOfBoundsCells()
    {
        levelCells.RemoveAll(cell => !IsValidPosition(cell.position));
    }
    #endregion

    #region Cell Operations

    public bool SetObjectAtPosition(Vector2Int position, LevelObjectData objectData)
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

        // Obje verisini ayarla
        cellData.objectData = objectData;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        return true;
    }

    /// <summary>
    /// Belirtilen pozisyondaki objeyi kaldırır
    /// </summary>
    /// <param name="position">Grid pozisyonu</param>
    /// <returns>Kaldırılan obje verisi</returns>
    public LevelObjectData RemoveObjectAtPosition(Vector2Int position)
    {
        LevelCellData cellData = GetCellData(position);
        if (cellData == null || cellData.objectData == null)
            return null;

        LevelObjectData removedData = cellData.objectData;
        cellData.objectData = null;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        return removedData;
    }

    public LevelObjectData GetObjectAtPosition(Vector2Int position)
    {
        LevelCellData cellData = GetCellData(position);
        return cellData?.objectData;
    }

    public bool HasObjectAtPosition(Vector2Int position)
    {
        LevelCellData cellData = GetCellData(position);
        return cellData?.objectData != null;
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

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public (Vector2Int size, float cellSize) GetGridBasicInfo()
    {
        return (GridSize, cellSize);
    }
    #endregion
}


[System.Serializable]
public class LevelCellData
{
    public Vector2Int position;
    public LevelObjectData objectData;

    public LevelCellData(Vector2Int pos)
    {
        position = pos;
        objectData = null;
    }
}
