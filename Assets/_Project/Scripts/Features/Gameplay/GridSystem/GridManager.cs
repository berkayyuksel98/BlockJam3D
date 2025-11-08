using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 gridOffset = Vector3.zero;
    
    private int gridWidth;
    private int gridHeight;
    private GameObject[,] gridObjects;
    
    public void SetupGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        gridObjects = new GameObject[width, height];
    }
    
    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * cellSize + gridOffset.x,
            gridOffset.y,
            gridPosition.y * cellSize + gridOffset.z
        );
    }
    
    public void SetObjectAtPosition(Vector2Int gridPosition, GameObject obj)
    {
        if (IsValidPosition(gridPosition))
        {
            gridObjects[gridPosition.x, gridPosition.y] = obj;
        }
    }
    
    public GameObject GetObjectAtPosition(Vector2Int gridPosition)
    {
        if (IsValidPosition(gridPosition))
        {
            return gridObjects[gridPosition.x, gridPosition.y];
        }
        return null;
    }
    
    public void ClearGrid(CharacterFactory characterFactory = null)
    {
        if (gridObjects != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridObjects[x, y] != null)
                    {
                        if (characterFactory != null)
                        {
                            characterFactory.ReturnCharacter(gridObjects[x, y]);
                        }
                        else
                        {
                            Destroy(gridObjects[x, y]);
                        }
                        gridObjects[x, y] = null;
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
}
