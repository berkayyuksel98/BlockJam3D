using UnityEngine;
using System.Collections.Generic;

public class GridManager : Singleton<GridManager>
{
    private Dictionary<string, GridSystem> grids = new Dictionary<string, GridSystem>();

    //Yeni bir grid olustur veya mevcutu dondur
    public GridSystem CreateGrid(string gridID, Vector3 position = default)
    {
        if (grids.TryGetValue(gridID, out GridSystem existingGrid))
        {
            Debug.Log($"Grid '{gridID}' already exists, returning existing grid.");
            return existingGrid;
        }

        GameObject gridObject = new GameObject($"Grid_{gridID}");
        gridObject.transform.position = position;
        gridObject.transform.SetParent(this.transform);
        
        GridSystem newGrid = gridObject.AddComponent<GridSystem>();
        
        var gridIDField = typeof(GridSystem).GetField("gridID", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (gridIDField != null)
        {
            gridIDField.SetValue(newGrid, gridID);
        }
        
        RegisterGrid(newGrid);
        
        Debug.Log($"Grid '{gridID}' created successfully.");
        return newGrid;
    }

    //Yeni bir gridi sisteme kaydet
    public void RegisterGrid(GridSystem gridSystem)
    {
        if (gridSystem == null)
        {
            Debug.LogError("Cannot register null GridSystem!");
            return;
        }

        string gridID = gridSystem.GridID;

        if (grids.ContainsKey(gridID))
        {
            Debug.LogWarning($"Grid with ID '{gridID}' already registered! Replacing...");
            grids[gridID] = gridSystem;
        }
        else
        {
            grids.Add(gridID, gridSystem);
            Debug.Log($"Grid '{gridID}' registered successfully.");
        }
    }

    public void UnregisterGrid(GridSystem gridSystem)
    {
        if (gridSystem == null) return;

        string gridID = gridSystem.GridID;

        if (grids.ContainsKey(gridID))
        {
            grids.Remove(gridID);
            Debug.Log($"Grid '{gridID}' unregistered.");
        }
    }

    public GridSystem GetGrid(string gridID)
    {
        if (grids.TryGetValue(gridID, out GridSystem grid))
        {
            return grid;
        }

        Debug.LogWarning($"Grid with ID '{gridID}' not found!");
        return null;
    }

    public bool HasGrid(string gridID)
    {
        return grids.ContainsKey(gridID);
    }

    public Dictionary<string, GridSystem> GetAllGrids()
    {
        return new Dictionary<string, GridSystem>(grids);
    }

    public int GetGridCount()
    {
        return grids.Count;
    }

    public void ClearAllGrids(CharacterFactory characterFactory = null)
    {
        foreach (var grid in grids.Values)
        {
            grid.ClearGrid(characterFactory);
        }
    }

    #region Debug Methods

    [ContextMenu("Debug All Grids")]
    public void DebugAllGrids()
    {
        Debug.Log($"=== GridManager Debug - Total Grids: {grids.Count} ===");
        
        foreach (var kvp in grids)
        {
            var gridInfo = kvp.Value.GetGridInfo();
            Debug.Log($"Grid '{kvp.Key}': {gridInfo.width}x{gridInfo.height}, CellSize: {gridInfo.cellSize}");
        }
    }

    #endregion
}
