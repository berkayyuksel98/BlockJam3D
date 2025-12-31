using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum CharacterLocationStatus
{
    Gameplay,
    Slot
}

public class SimpleCharacter : Character
{
    public List<Transform> colorTypeCharacters;
    private Transform currentVisualTransform;
    private CharacterLocationStatus locationStatus;
    private ColorType colorType;

    public ColorType ColorType => colorType;

    //Karakteri belirtilen renk tipi ve grid ID ile baslat
    public void Initialize(ColorType colorType, string gridID)
    {
        this.colorType = colorType;
        base.gridID = gridID;
        colorTypeCharacters.ForEach(t => t.gameObject.SetActive(false));
        int index = (int)colorType;
        if (index >= 0 && index < colorTypeCharacters.Count)
        {
            colorTypeCharacters[index].gameObject.SetActive(true);
            currentVisualTransform = colorTypeCharacters[index];
        }
        SubscribeToLevelLoaded();
        locationStatus = CharacterLocationStatus.Gameplay;
    }

    protected override void ActivateCharacter()
    {
        base.ActivateCharacter();
        CheckCharacterExitCondition();
    }

    //Karakteri cikis noktasina hareket ettir
    public override void MoveToExit(List<Vector3> path, string gridID, Vector2Int currentGridPos, System.Action action = null)
    {
        UnsubscribeEvents();
        this.gridID = gridID;
        this.currentGridPos = currentGridPos;
        locationStatus = CharacterLocationStatus.Slot;
        var pathTween = transform.DOPath(path.ToArray(), 0.4f, PathType.CatmullRom)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                action?.Invoke();
            });
    }
    protected override void OnGridChanged(OnGridChangedEvent gridEvent)
    {
        if (!isActivated) return;
        CheckCharacterExitCondition();
    }

    //Karakterin cikis yapip yapamayacagini kontrol et
    protected override void CheckCharacterExitCondition()
    {
        var canIExit = GameController.Instance.CanCharacterExit(this);
        if (canIExit)
        {
            CharacterExitConfirmed();
        }
        else
        {
            CharacterExitNotConfirmed();
        }
    }

    //Karakter cikis yapabildiginde gorsel geri bildirim ver
    protected override void CharacterExitConfirmed()
    {
        currentVisualTransform.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        currentVisualTransform.transform.localPosition = new Vector3(0f, 0.75f / 2f, 0f);
    }

    //Karakter cikis yapamadiginda gorsel geri bildirim ver
    protected override void CharacterExitNotConfirmed()
    {
        currentVisualTransform.transform.localScale = new Vector3(0.4f, 0.5f, 0.4f);
        currentVisualTransform.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        // UnityEditor.EditorApplication.isPaused = true; // Debug için durdurma kaldırıldı
    }

    protected override void OnMouseDown()
    {
        if (locationStatus != CharacterLocationStatus.Gameplay)
            return;


        Debug.Log($"Character clicked: {gameObject.name} at position {transform.position}");
        OnCharacterClickEvent clickEvent = new OnCharacterClickEvent
        (
            this,
            transform.position,
            currentGridPos
        );

        EventBus.Instance.Publish(clickEvent);
    }
}
