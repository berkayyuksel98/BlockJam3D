using System.Linq;
using UnityEngine;

public class BarrelCharacter : Character
{
    private ColorType colorType;
    private CharacterLocationStatus locationStatus;
    public ColorType ColorType => colorType;

    public void Initialize(ColorType colorType, string gridID)
    {
        this.colorType = colorType;
        base.gridID = gridID;
        SubscribeToLevelLoaded();
        locationStatus = CharacterLocationStatus.Gameplay;
    }

    protected override void ActivateCharacter()
    {
        base.ActivateCharacter();
    }

    protected override void OnGridChanged(OnGridChangedEvent gridEvent)
    {
        if (!isActivated) return;
        CheckAdjacentCellIsEmpty();
    }

    private void CheckAdjacentCellIsEmpty()
    {
        var gridSystem = GridManager.Instance.GetGrid(gridID);
        //komşu celleri bulalım.
        var checkGridPositions = new Vector2Int[]
        {
            new Vector2Int(currentGridPos.x + 1, currentGridPos.y),
            new Vector2Int(currentGridPos.x - 1, currentGridPos.y),
            new Vector2Int(currentGridPos.x, currentGridPos.y + 1),
            new Vector2Int(currentGridPos.x, currentGridPos.y - 1),
        };

        var hasEmptyCell = false;
        for (int i = 0; i < checkGridPositions.Count(); i++)
        {
            hasEmptyCell = !gridSystem.IsCellOccupied(checkGridPositions[i]);
            if (hasEmptyCell)
            {
                Debug.LogError("Adjacent cell is empty at position: " + checkGridPositions[i]);
                break;
            }
        }
        if (hasEmptyCell)
        {
            OnAdjacentCellEmpty();
        }
    }
    private void OnAdjacentCellEmpty()
    {
        UnsubscribeEvents();
        CreateBasicCharacter();
        Destroy(this.gameObject);
    }

    private void CreateBasicCharacter()
    {
        var simpleCharacterData = new SimpleCharacterData
        {
            characterColorType = this.colorType
        };
        var simpleCharacter = CharacterFactory.Instance.CreateCharacter(simpleCharacterData, transform.position, transform.rotation, gridID);
        GridManager.Instance.GetGrid(gridID).RemoveCharacterFromGrid(this);
        GridManager.Instance.GetGrid(gridID).SetObjectAtPosition(currentGridPos, simpleCharacter);
        simpleCharacter.GetComponent<Character>().SetGridPosition(currentGridPos, gridID);
    }
}
