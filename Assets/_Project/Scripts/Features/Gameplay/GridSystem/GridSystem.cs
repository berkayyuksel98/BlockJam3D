using UnityEngine;
using GridAStar;
using System.Collections.Generic;

public enum GridCenter
{
    Centered,
    BottomCentered,
}


[System.Serializable]
public class GridCell
{
    public int x, y;
    public GameObject gameObject;

    public GridCell(int _x, int _y, GameObject _gameObject)
    {
        x = _x;
        y = _y;
        gameObject = _gameObject;
    }
}

public class GridSystem : MonoBehaviour
{
    [Header("Grid Identity")]
    [SerializeField] private string gridID = "DefaultGrid";

    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 gridOffset = Vector3.zero;

    [Header("Visual Settings")]
    [SerializeField] private bool showGizmos = true;
    [ColorUsage(true, true)]
    [SerializeField] private Color emptyGridColor = new Color(0.000f, 1.000f, 0.000f, 0.246f);
    [SerializeField] private Color occupiedGridColor = new Color(1.000f, 0.000f, 0.000f, 0.197f);
    [SerializeField] private Color gridLineColor = new Color(1.000f, 1.000f, 1.000f, 0.200f);

    [Header("Debug Settings")]
    [SerializeField] private bool showBlockedCells = false;
    [ColorUsage(true, true)]
    [SerializeField] private Color blockedCellColor = new Color(1.000f, 0.000f, 0.000f, 0.193f);

    private int gridWidth;
    private int gridHeight;
    private GridCenter gridCenter = GridCenter.Centered;
    private GridCell[,] gridData;
    private AStarGrid aStarGrid;

    public string GridID => gridID;
    public float CellSize => cellSize;
    public Vector3 GridOffset => gridOffset;
    public GridCenter GridCentering => gridCenter;

    private void OnEnable()
    {
        if (GridManager.Instance != null && !GridManager.Instance.HasGrid(gridID))
        {
            GridManager.Instance.RegisterGrid(this);
        }
        EventBus.Instance.Subscribe<OnLevelLoadedEvent>(OnLevelLoaded);
    }

    private void OnLevelLoaded(OnLevelLoadedEvent levelEvent)
    {
        EventBus.Instance.Unsubscribe<OnLevelLoadedEvent>(OnLevelLoaded);
        EventBus.Instance.Publish(new OnGridChangedEvent(gridID));
        Debug.Log($"[{gridID}] Level yüklendi, ilk OnGridChanged event'i tetiklendi.");
    }

    public void RemoveCharacterFromGrid(Character character)
    {
        if (gridData == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridData[x, y].gameObject == character.gameObject)
                {
                    gridData[x, y].gameObject = null;
                    if (aStarGrid != null)
                    {
                        aStarGrid.SetBlocked(x, y, false);
                    }
                    EventBus.Instance.Publish(new OnGridChangedEvent(gridID));
                    return;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterGrid(this);
        }
        EventBus.Instance.Unsubscribe<OnLevelLoadedEvent>(OnLevelLoaded);
    }

    //Gridi belirtilen boyutlarda ve merkezleme ayarinda olustur
    public void SetupGrid(int width, int height, GridCenter center)
    {
        gridCenter = center;
        gridWidth = width;
        gridHeight = height;
        gridData = new GridCell[width, height];
        aStarGrid = new AStarGrid(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridData[x, y] = new GridCell(x, y, null);
            }
        }
    }

    //Grid koordinatini dunya pozisyonuna cevir
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        Vector3 worldPos = transform.position + gridOffset;

        float gridCenterX = gridWidth * cellSize * 0.5f;
        float xPos = (gridPosition.x * cellSize) - gridCenterX + (cellSize * 0.5f);

        float zPos = 0f;

        switch (gridCenter)
        {
            case GridCenter.Centered:
                float gridCenterZ = gridHeight * cellSize * 0.5f;
                zPos = (gridPosition.y * cellSize) - gridCenterZ + (cellSize * 0.5f);
                break;

            case GridCenter.BottomCentered:
                zPos = gridPosition.y * cellSize + (cellSize * 0.5f);
                break;
        }

        return worldPos + new Vector3(xPos, 0, zPos);
    }

    //Belirtilen grid pozisyonuna bir obje yerlestir
    public void SetObjectAtPosition(Vector2Int gridPosition, GameObject obj)
    {
        if (IsValidPosition(gridPosition) && gridData != null)
        {
            gridData[gridPosition.x, gridPosition.y].gameObject = obj;
            if (aStarGrid != null)
            {
                aStarGrid.SetBlocked(gridPosition.x, gridPosition.y, obj != null);
            }
            EventBus.Instance.Publish(new OnGridChangedEvent(gridID));
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

    public bool IsCellOccupied(Vector2Int gridPosition)
    {
        if (IsValidPosition(gridPosition) && gridData != null)
        {
            return gridData[gridPosition.x, gridPosition.y].gameObject != null;
        }
        return true; //valid değilse dolu kabul et
    }

    public GridCell GetGridCellAtPosition(Vector2Int gridPosition)
    {
        if (IsValidPosition(gridPosition) && gridData != null)
        {
            return gridData[gridPosition.x, gridPosition.y];
        }
        return null;
    }

    //Grid uzerindeki tum objeleri temizle
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
        
        if (gridWidth > 0 && gridHeight > 0)
        {
            aStarGrid = new AStarGrid(gridWidth, gridHeight);
        }
        EventBus.Instance.Publish(new OnGridChangedEvent(gridID));
    }

    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth &&
               position.y >= 0 && position.y < gridHeight;
    }

    public (int width, int height, float cellSize) GetGridInfo()
    {
        return (gridWidth, gridHeight, cellSize);
    }

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

    public int GetEmptyCellCount()
    {
        return (gridWidth * gridHeight) - GetOccupiedCellCount();
    }

    public Vector3 GetFirstEmptySlot()
    {
        var gridPos = GetFirstEmptyGridPosition();
        if (gridPos.HasValue)
        {
            return GetWorldPosition(gridPos.Value);
        }
        return Vector3.zero;
    }

    public Vector2Int? GetFirstEmptyGridPosition()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (gridData[x, y].gameObject == null)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return null;
    }

    //Iki nokta arasinda yol bul
    public List<Point> FindPath(Vector2Int start, Vector2Int end, bool ignoreStartBlocked = false)
    {
        if (aStarGrid == null) return null;

        bool wasBlocked = !aStarGrid.IsWalkable(start.x, start.y);
        if (ignoreStartBlocked && wasBlocked)
        {
            aStarGrid.SetBlocked(start.x, start.y, false);
        }

        var path = aStarGrid.FindPath(new Point(start.x, start.y), new Point(end.x, end.y));

        if (ignoreStartBlocked && wasBlocked)
        {
            aStarGrid.SetBlocked(start.x, start.y, true);
        }

        return path;
    }

    //Verilen baslangic noktasindan cikisa yol var mi kontrol et
    public bool HasPathToExit(Vector2Int startPos)
    {
        if (aStarGrid == null) return false;

        bool wasBlocked = !aStarGrid.IsWalkable(startPos.x, startPos.y);
        if (wasBlocked) aStarGrid.SetBlocked(startPos.x, startPos.y, false);

        bool found = false;
        Point startPoint = new Point(startPos.x, startPos.y);

        for (int x = 0; x < gridWidth; x++)
        {
            if (aStarGrid.IsWalkable(x, 0))
            {
                var path = aStarGrid.FindPath(startPoint, new Point(x, 0));
                if (path != null)
                {
                    found = true;
                    break;
                }
            }
        }

        if (wasBlocked) aStarGrid.SetBlocked(startPos.x, startPos.y, true);

        return found;
    }

    //AStarGrid verisini mevcut GridData ile senkronize et
    public void SyncAStarGrid()
    {
        if (gridData == null) return;
        
        aStarGrid = new AStarGrid(gridWidth, gridHeight);
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridData[x, y].gameObject != null)
                {
                    aStarGrid.SetBlocked(x, y, true);
                }
            }
        }
        Debug.Log($"[{gridID}] AStarGrid synced with GridData.");
    }

    #region Gizmo Drawing

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        DrawGrid();
        
        if (showBlockedCells && aStarGrid != null)
        {
            Gizmos.color = blockedCellColor;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (!aStarGrid.IsWalkable(x, y))
                    {
                        Vector3 center = GetWorldPosition(new Vector2Int(x, y));
                        Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
                    }
                }
            }
        }
    }

    private void DrawGrid()
    {
        if (gridWidth <= 0 || gridHeight <= 0) return;

        DrawGridLines();
        DrawGridCells();
    }

    private void DrawGridLines()
    {
        Gizmos.color = gridLineColor;

        Vector3 bottomLeft = GetWorldPosition(new Vector2Int(0, 0)) - new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
        Vector3 topRight = GetWorldPosition(new Vector2Int(gridWidth - 1, gridHeight - 1)) + new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);

        for (int y = 0; y <= gridHeight; y++)
        {
            float zPos = bottomLeft.z + (y * cellSize);
            Vector3 start = new Vector3(bottomLeft.x, transform.position.y + gridOffset.y, zPos);
            Vector3 end = new Vector3(topRight.x, transform.position.y + gridOffset.y, zPos);
            Gizmos.DrawLine(start, end);
        }

        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = bottomLeft.x + (x * cellSize);
            Vector3 start = new Vector3(xPos, transform.position.y + gridOffset.y, bottomLeft.z);
            Vector3 end = new Vector3(xPos, transform.position.y + gridOffset.y, topRight.z);
            Gizmos.DrawLine(start, end);
        }
    }

    private void DrawGridCells()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 cellCenter = GetWorldPosition(new Vector2Int(x, y));
                cellCenter.y += 0.01f;

                bool isOccupied = false;
                if (gridData != null && gridData[x, y] != null)
                {
                    isOccupied = gridData[x, y].gameObject != null;
                }

                Gizmos.color = isOccupied ? occupiedGridColor : emptyGridColor;

                Vector3 cubeSize = new Vector3(cellSize * 0.8f, 0.02f, cellSize * 0.8f);
                Gizmos.DrawCube(cellCenter, cubeSize);

#if UNITY_EDITOR
                if (UnityEditor.Selection.activeGameObject == this.gameObject)
                {
                    UnityEditor.Handles.Label(cellCenter + Vector3.up * 0.1f, $"({x},{y})");
                }
#endif
            }
        }

        if (showBlockedCells && aStarGrid != null)
        {
            Gizmos.color = blockedCellColor;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (!aStarGrid.IsWalkable(x, y))
                    {
                        Vector3 blockedCellCenter = GetWorldPosition(new Vector2Int(x, y));
                        blockedCellCenter.y += 0.01f;
                        Gizmos.DrawCube(blockedCellCenter, new Vector3(cellSize * 0.8f, 0.02f, cellSize * 0.8f));
                    }
                }
            }
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Debug Grid Positions")]
    public void DebugGridPositions()
    {
        if (gridWidth <= 0 || gridHeight <= 0)
        {
            Debug.Log($"[{gridID}] Grid not initialized!");
            return;
        }

        Debug.Log($"=== [{gridID}] Grid Debug ({gridWidth}x{gridHeight}) ===");
        for (int y = gridHeight - 1; y >= 0; y--)
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

    #endregion
}


