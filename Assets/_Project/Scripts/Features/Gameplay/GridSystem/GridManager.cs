using UnityEngine;


[System.Serializable]
public class Grid
{
    public int x, y;
    public GameObject gameObject;
    public Grid(int _x, int _y, GameObject _gameObject)
    {
        x = _x;
        y = _y;
        gameObject = _gameObject;
    }
}
public class GridManager : Singleton<GridManager>
{
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 gridOffset = Vector3.zero;

    [Header("Visual Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color emptyGridColor = Color.green;
    [SerializeField] private Color occupiedGridColor = Color.red;
    [SerializeField] private Color gridLineColor = Color.white;

    private int gridWidth;
    private int gridHeight;
    private Grid[,] gridData;

    public void SetupGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        gridData = new Grid[width, height];

        // Initialize grid data
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridData[x, y] = new Grid(x, y, null);
            }
        }
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        // Grid'in tam merkez noktasını hesapla
        float gridCenterX = gridWidth * cellSize * 0.5f;

        return new Vector3(
            (gridPosition.x * cellSize) - gridCenterX + (cellSize * 0.5f) + gridOffset.x,
            gridOffset.y,
            gridPosition.y * cellSize + gridOffset.z
        );
    }

    public void SetObjectAtPosition(Vector2Int gridPosition, GameObject obj)
    {
        if (IsValidPosition(gridPosition) && gridData != null)
        {
            gridData[gridPosition.x, gridPosition.y].gameObject = obj;
        }
    }

    public GameObject GetObjectAtPosition(Vector2Int gridPosition)
    {
        if (IsValidPosition(gridPosition) && gridData != null)
        {
            return gridData[gridPosition.x, gridPosition.y].gameObject;
        }
        return null;
    }

    public Grid GetGridAtPosition(Vector2Int gridPosition)
    {
        if (IsValidPosition(gridPosition) && gridData != null)
        {
            return gridData[gridPosition.x, gridPosition.y];
        }
        return null;
    }

    public void ClearGrid(CharacterFactory characterFactory = null)
    {
        if (gridData != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridData[x, y].gameObject != null)
                    {
                        if (characterFactory != null)
                        {
                            characterFactory.ReturnCharacter(gridData[x, y].gameObject);
                        }
                        else
                        {
                            Destroy(gridData[x, y].gameObject);
                        }
                        gridData[x, y].gameObject = null;
                    }
                }
            }
        }
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth &&
               position.y >= 0 && position.y < gridHeight;
    }

    #region Gizmo Drawing

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        DrawGrid();
    }

    private void DrawGrid()
    {
        if (gridWidth <= 0 || gridHeight <= 0) return;

        // Grid çizgilerini çiz
        DrawGridLines();

        // Grid hücrelerini çiz
        DrawGridCells();
    }

    private void DrawGridLines()
    {
        Gizmos.color = gridLineColor;

        // Grid'in sol alt köşesi ve sağ üst köşesi pozisyonlarını hesapla
        Vector3 bottomLeft = GetWorldPosition(new Vector2Int(0, 0)) - new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
        Vector3 topRight = GetWorldPosition(new Vector2Int(gridWidth - 1, gridHeight - 1)) + new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);

        // Yatay çizgiler (Z ekseni boyunca)
        for (int y = 0; y <= gridHeight; y++)
        {
            float zPos = bottomLeft.z + (y * cellSize);
            Vector3 start = new Vector3(bottomLeft.x, gridOffset.y, zPos);
            Vector3 end = new Vector3(topRight.x, gridOffset.y, zPos);
            Gizmos.DrawLine(start, end);
        }

        // Dikey çizgiler (X ekseni boyunca)
        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = bottomLeft.x + (x * cellSize);
            Vector3 start = new Vector3(xPos, gridOffset.y, bottomLeft.z);
            Vector3 end = new Vector3(xPos, gridOffset.y, topRight.z);
            Gizmos.DrawLine(start, end);
        }
    }

    private void DrawGridCells()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Her hücrenin world pozisyonunu GetWorldPosition ile al
                Vector3 cellCenter = GetWorldPosition(new Vector2Int(x, y));
                cellCenter.y += 0.01f; // Biraz yüksek çiz

                // Hücre dolu mu boş mu kontrol et
                bool isOccupied = false;
                if (gridData != null && gridData[x, y] != null)
                {
                    isOccupied = gridData[x, y].gameObject != null;
                }

                // Rengi ayarla
                Gizmos.color = isOccupied ? occupiedGridColor : emptyGridColor;

                // Hücreyi çiz (küp olarak)
                Vector3 cubeSize = new Vector3(cellSize * 0.8f, 0.02f, cellSize * 0.8f);
                Gizmos.DrawCube(cellCenter, cubeSize);

                // Hücre koordinatlarını göster (selected iken)
                if (Application.isEditor && UnityEditor.Selection.activeGameObject == this.gameObject)
                {
                    UnityEditor.Handles.Label(cellCenter + Vector3.up * 0.1f, $"({x},{y})");
                }
            }
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Debug için grid pozisyonlarını yazdır
    /// </summary>
    [ContextMenu("Debug Grid Positions")]
    public void DebugGridPositions()
    {
        if (gridWidth <= 0 || gridHeight <= 0)
        {
            Debug.Log("Grid not initialized!");
            return;
        }

        Debug.Log($"=== Grid Debug ({gridWidth}x{gridHeight}) ===");
        for (int y = gridHeight - 1; y >= 0; y--) // Top to bottom
        {
            string row = $"Y={y}: ";
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 worldPos = GetWorldPosition(new Vector2Int(x, y));
                row += $"({x},{y})→({worldPos.x:F1},{worldPos.z:F1}) ";
            }
            Debug.Log(row);
        }
    }

    /// <summary>
    /// Grid'deki dolu hücre sayısını döner
    /// </summary>
    public int GetOccupiedCellCount()
    {
        if (gridData == null) return 0;

        int count = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridData[x, y].gameObject != null)
                    count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Grid'deki boş hücre sayısını döner
    /// </summary>
    public int GetEmptyCellCount()
    {
        return (gridWidth * gridHeight) - GetOccupiedCellCount();
    }

    /// <summary>
    /// Grid'in temel bilgilerini döner
    /// </summary>
    public (int width, int height, float cellSize) GetGridInfo()
    {
        return (gridWidth, gridHeight, cellSize);
    }

    #endregion
}
