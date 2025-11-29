using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Tüm karakterlerin türediği temel sınıf.
/// </summary>
public class Character : MonoBehaviour
{
    protected Vector2Int currentGridPos;
    protected string gridID;

    /// <summary>
    /// Event aboneliklerini başlatır.
    /// </summary>
    protected virtual void SubscribeEvents()
    {
        EventBus.Instance.Subscribe<OnGridChangedEvent>(OnGridChanged);
    }

    /// <summary>
    /// Event aboneliklerini sonlandırır.
    /// </summary>
    protected virtual void UnsubscribeEvents()
    {
        EventBus.Instance.Unsubscribe<OnGridChangedEvent>(OnGridChanged);
    }

    /// <summary>
    /// Nesne yok edildiğinde abonelikleri temizler.
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    /// <summary>
    /// Grid değiştiğinde tetiklenir.
    /// </summary>
    /// <param name="gridEvent">Grid değişim olayı.</param>
    protected void OnGridChanged(OnGridChangedEvent gridEvent)
    {
        CheckCharacterExitCondition();
    }

    /// <summary>
    /// Karakterin çıkış yapıp yapamayacağını kontrol eder.
    /// </summary>
    protected void CheckCharacterExitCondition()
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

    /// <summary>
    /// Karakter çıkış yapabildiğinde çalışır.
    /// </summary>
    protected virtual void CharacterExitConfirmed()
    {

    }

    /// <summary>
    /// Karakter çıkış yapamadığında çalışır.
    /// </summary>
    protected virtual void CharacterExitNotConfirmed()
    {
    }

    /// <summary>
    /// Karakterin grid pozisyonunu ve grid ID'sini günceller.
    /// </summary>
    /// <param name="gridPosition">Yeni grid pozisyonu.</param>
    /// <param name="gridID">Grid ID'si.</param>
    public void SetGridPosition(Vector2Int gridPosition, string gridID)
    {
        currentGridPos = gridPosition;
        this.gridID = gridID;
    }

    /// <summary>
    /// Karakterin mevcut grid pozisyonunu döndürür.
    /// </summary>
    /// <returns>Grid pozisyonu.</returns>
    public Vector2Int GetGridPosition()
    {
        return currentGridPos;
    }

    /// <summary>
    /// Karakter tıklandığında çalışır.
    /// </summary>
    protected virtual void OnMouseDown()
    {
        Debug.Log($"Character clicked: {gameObject.name} at position {transform.position}");
        OnCharacterClickEvent clickEvent = new OnCharacterClickEvent
        (
            this,
            transform.position,
            currentGridPos
        );

        EventBus.Instance.Publish(clickEvent);
    }

    /// <summary>
    /// Karakteri çıkış noktasına hareket ettirir.
    /// </summary>
    /// <param name="path">Takip edilecek yol.</param>
    /// <param name="targetGridID">Hedef grid ID'si.</param>
    /// <param name="targetGridPosition">Hedef grid pozisyonu.</param>
    /// <param name="onComplete">Hareket tamamlandığında çalışacak aksiyon.</param>
    public virtual void MoveToExit(System.Collections.Generic.List<Vector3> path, string targetGridID, Vector2Int targetGridPosition, System.Action onComplete = null)
    {
    }
}
