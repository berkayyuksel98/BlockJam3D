using System.Collections.Generic;
using UnityEngine;

public class PipeCharacter : Character
{
    private Queue<ColorType> colorQueue;
    private CharacterLocationStatus locationStatus;
    private PipeDirection pipeDirection;
    private Vector2Int targetSpawnPos;
    
    public void Initialize(List<ColorType> colorTypes, string gridID, PipeDirection pipeDirection)
    {
        this.colorQueue = new Queue<ColorType>(colorTypes);
        base.gridID = gridID;
        this.pipeDirection = pipeDirection;
        SubscribeToLevelLoaded();
        locationStatus = CharacterLocationStatus.Gameplay;
    }

    protected override void ActivateCharacter()
    {
        base.ActivateCharacter();
        // Pipe yönüne göre spawn pozisyonunu hesapla
        
        var visualTransform = transform.GetChild(0);
        switch (pipeDirection)
        {
            case PipeDirection.Forward:
                visualTransform.forward = Vector3.forward;
                targetSpawnPos = new Vector2Int(currentGridPos.x, currentGridPos.y + 1);
                break;
            case PipeDirection.Back:
                visualTransform.forward = Vector3.back;
                targetSpawnPos = new Vector2Int(currentGridPos.x, currentGridPos.y - 1);
                break;
            case PipeDirection.Right:
                visualTransform.forward = Vector3.right;
                targetSpawnPos = new Vector2Int(currentGridPos.x + 1, currentGridPos.y);
                break;
            case PipeDirection.Left:
                visualTransform.forward = Vector3.left;
                targetSpawnPos = new Vector2Int(currentGridPos.x - 1, currentGridPos.y);
                break;
        }
    }

    protected override void OnGridChanged(OnGridChangedEvent gridEvent)
    {
        if (!isActivated) return;
        CheckPipeDirectionIsEmpty();
    }

    //Pipein baktigi yondeki hucrenin bos olup olmadigini kontrol et
    private void CheckPipeDirectionIsEmpty()
    {
        var gridSystem = GridManager.Instance.GetGrid(gridID);

        if (!gridSystem.IsCellOccupied(targetSpawnPos))
        {
            Debug.Log($"Pipe yönündeki hücre boş: {targetSpawnPos}");
            OnTargetCellEmpty();
        }
    }

    //Pipe yonundeki hucre bosaldiginda siradaki karakteri spawn et
    private void OnTargetCellEmpty()
    {
        if (colorQueue == null || colorQueue.Count == 0)
        {
            // Tüm karakterler spawn edildi, pipe'ı yok et
            DestroyPipe();
            return;
        }
        
        SpawnNextCharacter();
    }

    private void SpawnNextCharacter()
    {
        var colorType = colorQueue.Dequeue();
        var simpleCharacterData = new SimpleCharacterData
        {
            characterColorType = colorType
        };
        
        var gridSystem = GridManager.Instance.GetGrid(gridID);
        var spawnWorldPos = gridSystem.GetWorldPosition(targetSpawnPos);
        
        var simpleCharacter = CharacterFactory.Instance.CreateCharacter(simpleCharacterData, spawnWorldPos, Quaternion.identity, gridID);
        gridSystem.SetObjectAtPosition(targetSpawnPos, simpleCharacter);
        simpleCharacter.GetComponent<Character>().SetGridPosition(targetSpawnPos, gridID);
        
        Debug.Log($"Pipe'dan karakter spawn edildi. Kalan: {colorQueue.Count}");
    }

    /// <summary>
    /// Pipe'ı yok eder ve grid'den kaldırır.
    /// </summary>
    private void DestroyPipe()
    {
        UnsubscribeEvents();
        GridManager.Instance.GetGrid(gridID).RemoveCharacterFromGrid(this);
        Destroy(this.gameObject);
    }
}
