using System.Collections.Generic;
using DG.Tweening;
using GridAStar;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Oyunun genel akışını ve kullanıcı etkileşimlerini yöneten ana kontrol sınıfı.
/// </summary>
public class GameController : Singleton<GameController>
{
    /// <summary>
    /// Oyun başladığında gerekli event aboneliklerini başlatır.
    /// </summary>
    private void Awake()
    {
        EventBus.Instance.Subscribe<OnCharacterClickEvent>(OnCharacterClicked);
    }

    /// <summary>
    /// Karakter tıklandığında tetiklenen olay işleyicisi.
    /// </summary>
    /// <param name="clickEvent">Tıklama olayı verileri.</param>
    private void OnCharacterClicked(OnCharacterClickEvent clickEvent)
    {
        CheckCharacterClick(clickEvent);
    }

    /// <summary>
    /// Nesne yok edildiğinde event aboneliklerini sonlandırır.
    /// </summary>
    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe<OnCharacterClickEvent>(OnCharacterClicked);
    }

    /// <summary>
    /// Tıklanan karakterin hareket edip edemeyeceğini ve nereye gideceğini kontrol eder.
    /// </summary>
    /// <param name="clickEvent">Tıklama olayı verileri.</param>
    private void CheckCharacterClick(OnCharacterClickEvent clickEvent)
    {
        if (!(clickEvent.character is SimpleCharacter simpleCharacter))
        {
            Debug.Log("Not a SimpleCharacter clicked. No pathfinding needed.");
            return;
        }

        GridSystem gameplayGrid = GridManager.Instance.GetGrid("gameplay");

        if (gameplayGrid == null)
        {
            Debug.LogError("Gameplay grid not found!");
            return;
        }

        // 1. Combine Zone'da yer var mı kontrol et
        if (!CombineZoneManager.Instance.HasEmptySlot())
        {
            Debug.Log("Combine Zone is FULL!");
            return;
        }

        // 2. Gameplay grid'den çıkış yolu bul
        var gridInfo = gameplayGrid.GetGridInfo();
        int gridWidth = gridInfo.width;
        List<Point> path = null;
        Vector2Int startPos = new Vector2Int(clickEvent.gridPosition.x, clickEvent.gridPosition.y);

        for (int x = 0; x < gridWidth; x++)
        {
            var tempPath = gameplayGrid.FindPath(startPos, new Vector2Int(x, 0), ignoreStartBlocked: true);
            if (tempPath != null)
            {
                path = tempPath;
                break;
            }
        }

        if (path == null)
        {
            Debug.Log($"No valid exit path found from ({clickEvent.gridPosition.x}, {clickEvent.gridPosition.y})");
            return;
        }

        // 3. Gameplay grid'den karakteri kaldır
        gameplayGrid.SetObjectAtPosition(startPos, null);

        // 4. Çıkış yolunu oluştur
        List<Vector3> worldPath = new List<Vector3>();
        foreach (var point in path)
        {
            worldPath.Add(gameplayGrid.GetWorldPosition(new Vector2Int(point.x, point.y)));
        }

        // 5. Combine Zone'daki hedef pozisyonu ekle
        Vector3 targetWorldPos = CombineZoneManager.Instance.GetTargetWorldPosition(simpleCharacter);
        worldPath.Add(targetWorldPos);

        // 6. Karakteri hareket ettir ve Combine Zone'a yerleştir
        simpleCharacter.MoveToExit(worldPath, "combinezone", Vector2Int.zero, () =>
        {
            CombineZoneManager.Instance.PlaceCharacter(simpleCharacter, () =>
            {
                Debug.Log("Character placed and match check complete!");
            });
        });
    }

    public bool CanCharacterExit(Character character)
    {
        GridSystem gameplayGrid = GridManager.Instance.GetGrid("gameplay");
        if (gameplayGrid == null) return false;

        Vector2Int startPos = character.GetGridPosition();

        return gameplayGrid.HasPathToExit(startPos);
    }
}