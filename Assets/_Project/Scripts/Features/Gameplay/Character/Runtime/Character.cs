using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Tüm karakterlerin türediği temel sınıf.
/// </summary>
public class Character : MonoBehaviour
{
    protected Vector2Int currentGridPos;
    protected string gridID;
    protected bool isActivated = false;

    /// <summary>
    /// Level yükleme event'ine abone olur.
    /// </summary>
    protected virtual void SubscribeToLevelLoaded()
    {
        EventBus.Instance.Subscribe<OnLevelLoadedEvent>(OnLevelLoaded);
    }

    /// <summary>
    /// Level yüklendiğinde çağrılır, karakteri aktif hale getirir.
    /// </summary>
    protected virtual void OnLevelLoaded(OnLevelLoadedEvent levelEvent)
    {
        EventBus.Instance.Unsubscribe<OnLevelLoadedEvent>(OnLevelLoaded);
        ActivateCharacter();
    }

    /// <summary>
    /// Karakteri aktif hale getirir ve grid event'lerine bağlar.
    /// </summary>
    protected virtual void ActivateCharacter()
    {
        if (isActivated) return;
        isActivated = true;
        SubscribeEvents();
    }

    protected virtual void SubscribeEvents()
    {
        EventBus.Instance.Subscribe<OnGridChangedEvent>(OnGridChanged);
    }

    protected virtual void UnsubscribeEvents()
    {
        EventBus.Instance.Unsubscribe<OnGridChangedEvent>(OnGridChanged);
        EventBus.Instance.Unsubscribe<OnLevelLoadedEvent>(OnLevelLoaded);
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    protected virtual void OnGridChanged(OnGridChangedEvent gridEvent)
    {
       
    }

    protected virtual void CheckCharacterExitCondition()
    {
        
    }

    protected virtual void CharacterExitConfirmed()
    {

    }

    protected virtual void CharacterExitNotConfirmed()
    {
    }

    public void SetGridPosition(Vector2Int gridPosition, string gridID)
    {
        currentGridPos = gridPosition;
        this.gridID = gridID;
    }

    public Vector2Int GetGridPosition()
    {
        return currentGridPos;
    }

    protected virtual void OnMouseDown()
    {
        Debug.Log($"Character clicked: {gameObject.name} at position {transform.position}");
    }

    public virtual void MoveToExit(System.Collections.Generic.List<Vector3> path, string targetGridID, Vector2Int targetGridPosition, System.Action onComplete = null)
    {
    }
}
