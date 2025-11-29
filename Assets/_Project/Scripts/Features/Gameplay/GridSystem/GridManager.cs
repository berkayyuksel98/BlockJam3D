using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Merkezi Grid yönetim sistemi - Tüm GridSystem'leri yönetir.
/// </summary>
public class GridManager : Singleton<GridManager>
{
    private Dictionary<string, GridSystem> grids = new Dictionary<string, GridSystem>();

    /// <summary>
    /// Yeni bir grid oluşturur veya mevcut olanı döndürür.
    /// </summary>
    /// <param name="gridID">Grid ID'si.</param>
    /// <param name="position">Grid pozisyonu.</param>
    /// <returns>Oluşturulan veya bulunan GridSystem.</returns>
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

    /// <summary>
    /// Yeni bir grid'i sisteme kaydeder.
    /// </summary>
    /// <param name="gridSystem">Kaydedilecek GridSystem.</param>
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

    /// <summary>
    /// Grid'i sistemden kaldırır.
    /// </summary>
    /// <param name="gridSystem">Kaldırılacak GridSystem.</param>
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

    /// <summary>
    /// ID'ye göre grid döndürür.
    /// </summary>
    /// <param name="gridID">Grid ID'si.</param>
    /// <returns>Bulunan GridSystem veya null.</returns>
    public GridSystem GetGrid(string gridID)
    {
        if (grids.TryGetValue(gridID, out GridSystem grid))
        {
            return grid;
        }

        Debug.LogWarning($"Grid with ID '{gridID}' not found!");
        return null;
    }

    /// <summary>
    /// Grid'in var olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="gridID">Grid ID'si.</param>
    /// <returns>Varsa true döner.</returns>
    public bool HasGrid(string gridID)
    {
        return grids.ContainsKey(gridID);
    }

    /// <summary>
    /// Tüm kayıtlı gridleri döndürür.
    /// </summary>
    /// <returns>Grid sözlüğü.</returns>
    public Dictionary<string, GridSystem> GetAllGrids()
    {
        return new Dictionary<string, GridSystem>(grids);
    }

    /// <summary>
    /// Kayıtlı grid sayısını döndürür.
    /// </summary>
    /// <returns>Grid sayısı.</returns>
    public int GetGridCount()
    {
        return grids.Count;
    }

    /// <summary>
    /// Tüm gridleri temizler.
    /// </summary>
    /// <param name="characterFactory">Karakter fabrikası (opsiyonel).</param>
    public void ClearAllGrids(CharacterFactory characterFactory = null)
    {
        foreach (var grid in grids.Values)
        {
            grid.ClearGrid(characterFactory);
        }
    }

    #region Debug Methods

    /// <summary>
    /// Tüm gridlerin bilgilerini debug loguna yazar.
    /// </summary>
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
