using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Combine Zone'daki karakterlerin yerleşimini, kaydırmasını ve eşleşmesini yöneten sınıf.
/// </summary>
public class CombineZoneManager : Singleton<CombineZoneManager>
{
    [Header("Match Settings")]
    [SerializeField] private int matchCount = 3;
    [SerializeField] private float slideAnimationDuration = 0.15f;
    [SerializeField] private float mergeAnimationDuration = 0.2f;

    private GridSystem combineGrid;
    private List<SimpleCharacter> slotCharacters = new List<SimpleCharacter>();

    /// <summary>
    /// Combine Zone grid'ini ayarlar.
    /// </summary>
    public void Initialize(GridSystem grid)
    {
        combineGrid = grid;
        slotCharacters.Clear();
    }

    /// <summary>
    /// Yeni bir karakteri Combine Zone'a yerleştirir.
    /// Aynı renklerin yanına yerleştirir ve gerekirse diğerlerini kaydırır.
    /// </summary>
    /// <param name="newCharacter">Yerleştirilecek karakter.</param>
    /// <param name="onPlacementComplete">Yerleştirme tamamlandığında çağrılacak aksiyon.</param>
    /// <returns>Yerleştirme başarılı ise hedef pozisyon, değilse null.</returns>
    public Vector2Int? PlaceCharacter(SimpleCharacter newCharacter, System.Action onPlacementComplete = null)
    {
        if (combineGrid == null)
        {
            combineGrid = GridManager.Instance.GetGrid("combinezone");
        }

        if (combineGrid == null)
        {
            Debug.LogError("Combine Zone grid not found!");
            return null;
        }

        ColorType newColor = newCharacter.ColorType;
        int insertIndex = FindInsertIndex(newColor);

        // Eğer yer yoksa
        var gridInfo = combineGrid.GetGridInfo();
        if (slotCharacters.Count >= gridInfo.width)
        {
            Debug.LogWarning("Combine Zone is FULL!");
            return null;
        }

        // Karakterleri kaydır (insertIndex'ten sonraki tüm karakterler sağa kayacak)
        ShiftCharactersRight(insertIndex, () =>
        {
            // Yeni karakteri listeye ekle
            slotCharacters.Insert(insertIndex, newCharacter);

            // Grid pozisyonunu güncelle
            Vector2Int targetPos = new Vector2Int(insertIndex, 0);
            combineGrid.SetObjectAtPosition(targetPos, newCharacter.gameObject);
            newCharacter.SetGridPosition(targetPos, "combinezone");

            // Eşleşme kontrolü
            CheckForMatches(() =>
            {
                onPlacementComplete?.Invoke();
            });
        });

        return new Vector2Int(insertIndex, 0);
    }

    /// <summary>
    /// Yeni karakterin yerleştirileceği index'i bulur.
    /// Aynı renkteki karakterlerin yanına yerleştirir.
    /// </summary>
    private int FindInsertIndex(ColorType color)
    {
        // Aynı renkteki son karakterin index'ini bul
        int lastSameColorIndex = -1;
        for (int i = 0; i < slotCharacters.Count; i++)
        {
            if (slotCharacters[i].ColorType == color)
            {
                lastSameColorIndex = i;
            }
        }

        // Aynı renk varsa onun yanına, yoksa sona ekle
        if (lastSameColorIndex >= 0)
        {
            return lastSameColorIndex + 1;
        }
        return slotCharacters.Count;
    }

    /// <summary>
    /// Belirtilen index'ten itibaren karakterleri sağa kaydırır.
    /// </summary>
    private void ShiftCharactersRight(int fromIndex, System.Action onComplete)
    {
        if (fromIndex >= slotCharacters.Count)
        {
            onComplete?.Invoke();
            return;
        }

        int shiftCount = slotCharacters.Count - fromIndex;
        int completedShifts = 0;

        // Sondan başa doğru kaydır (çakışma olmaması için)
        for (int i = slotCharacters.Count - 1; i >= fromIndex; i--)
        {
            var character = slotCharacters[i];
            int newX = i + 1;
            Vector2Int newPos = new Vector2Int(newX, 0);

            // Grid'i güncelle
            Vector2Int oldPos = character.GetGridPosition();
            combineGrid.SetObjectAtPosition(oldPos, null);
            combineGrid.SetObjectAtPosition(newPos, character.gameObject);
            character.SetGridPosition(newPos, "combinezone");

            // Animasyonlu hareket
            Vector3 targetWorldPos = combineGrid.GetWorldPosition(newPos);
            character.transform.DOMove(targetWorldPos, slideAnimationDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    completedShifts++;
                    if (completedShifts >= shiftCount)
                    {
                        onComplete?.Invoke();
                    }
                });
        }
    }

    /// <summary>
    /// 3 veya daha fazla aynı renk karakter yan yana mı kontrol eder ve eşleşirse birleştirir.
    /// </summary>
    private void CheckForMatches(System.Action onComplete)
    {
        List<int> matchIndices = FindMatchIndices();

        if (matchIndices.Count >= matchCount)
        {
            MergeCharacters(matchIndices, () =>
            {
                // Birleştirmeden sonra tekrar kontrol et (zincirleme eşleşmeler için)
                CheckForMatches(onComplete);
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Eşleşen karakterlerin index'lerini bulur.
    /// </summary>
    private List<int> FindMatchIndices()
    {
        if (slotCharacters.Count < matchCount) return new List<int>();

        for (int i = 0; i <= slotCharacters.Count - matchCount; i++)
        {
            ColorType checkColor = slotCharacters[i].ColorType;
            List<int> consecutive = new List<int> { i };

            for (int j = i + 1; j < slotCharacters.Count; j++)
            {
                if (slotCharacters[j].ColorType == checkColor)
                {
                    consecutive.Add(j);
                }
                else
                {
                    break;
                }
            }

            if (consecutive.Count >= matchCount)
            {
                // İlk 3'ü döndür (veya matchCount kadar)
                return consecutive.Take(matchCount).ToList();
            }
        }

        return new List<int>();
    }

    /// <summary>
    /// Eşleşen karakterleri orta noktada birleştirir ve yok eder.
    /// </summary>
    private void MergeCharacters(List<int> indices, System.Action onComplete)
    {
        if (indices.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // Orta noktayı hesapla
        Vector3 centerPoint = Vector3.zero;
        List<SimpleCharacter> charactersToMerge = new List<SimpleCharacter>();

        foreach (int index in indices)
        {
            var character = slotCharacters[index];
            charactersToMerge.Add(character);
            centerPoint += character.transform.position;
        }
        centerPoint /= indices.Count;

        int completedMerges = 0;

        // Tüm karakterleri merkeze çek
        foreach (var character in charactersToMerge)
        {
            // Grid'den temizle
            Vector2Int gridPos = character.GetGridPosition();
            combineGrid.SetObjectAtPosition(gridPos, null);

            character.transform.DOMove(centerPoint, mergeAnimationDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    completedMerges++;
                    if (completedMerges >= charactersToMerge.Count)
                    {
                        // Tüm karakterleri yok et
                        foreach (var c in charactersToMerge)
                        {
                            // Scale animasyonu ile kaybolma
                            c.transform.DOScale(Vector3.zero, 0.1f)
                                .OnComplete(() =>
                                {
                                    GameObject.Destroy(c.gameObject);
                                });
                        }

                        // Listeden kaldır (büyükten küçüğe sıralı şekilde)
                        foreach (int index in indices.OrderByDescending(x => x))
                        {
                            slotCharacters.RemoveAt(index);
                        }

                        // Kalan karakterleri sola kaydır
                        DOVirtual.DelayedCall(0.15f, () =>
                        {
                            ReorganizeSlots(() =>
                            {
                                onComplete?.Invoke();
                            });
                        });
                    }
                });
        }
    }

    /// <summary>
    /// Birleştirme sonrası kalan karakterleri yeniden düzenler.
    /// </summary>
    private void ReorganizeSlots(System.Action onComplete)
    {
        if (slotCharacters.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        int completedMoves = 0;
        int totalMoves = slotCharacters.Count;

        for (int i = 0; i < slotCharacters.Count; i++)
        {
            var character = slotCharacters[i];
            Vector2Int newPos = new Vector2Int(i, 0);
            Vector2Int oldPos = character.GetGridPosition();

            if (oldPos != newPos)
            {
                combineGrid.SetObjectAtPosition(oldPos, null);
                combineGrid.SetObjectAtPosition(newPos, character.gameObject);
                character.SetGridPosition(newPos, "combinezone");

                Vector3 targetWorldPos = combineGrid.GetWorldPosition(newPos);
                character.transform.DOMove(targetWorldPos, slideAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        completedMoves++;
                        if (completedMoves >= totalMoves)
                        {
                            onComplete?.Invoke();
                        }
                    });
            }
            else
            {
                completedMoves++;
                if (completedMoves >= totalMoves)
                {
                    onComplete?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// Combine Zone'da yer olup olmadığını kontrol eder.
    /// </summary>
    public bool HasEmptySlot()
    {
        if (combineGrid == null)
        {
            combineGrid = GridManager.Instance.GetGrid("combinezone");
        }

        if (combineGrid == null) return false;

        var gridInfo = combineGrid.GetGridInfo();
        return slotCharacters.Count < gridInfo.width;
    }

    /// <summary>
    /// Yeni karakterin yerleşeceği hedef world pozisyonunu döndürür.
    /// </summary>
    public Vector3 GetTargetWorldPosition(SimpleCharacter character)
    {
        if (combineGrid == null)
        {
            combineGrid = GridManager.Instance.GetGrid("combinezone");
        }

        int insertIndex = FindInsertIndex(character.ColorType);
        return combineGrid.GetWorldPosition(new Vector2Int(insertIndex, 0));
    }

    /// <summary>
    /// Mevcut slot sayısını döndürür.
    /// </summary>
    public int GetCurrentSlotCount()
    {
        return slotCharacters.Count;
    }
}
