using UnityEngine;

/// <summary>
/// Dokunma girişlerini yöneten sınıf
/// </summary>
public class TouchInputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private LayerMask touchableLayerMask = -1;
    [SerializeField] private float maxTouchDistance = 100f;
    
    private Camera playerCamera;
    
    void Start()
    {
        // Ana kamerayı bul
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Mouse input (Editor ve PC için)
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            ProcessTouch(mousePosition);
        }
        
        // Touch input (Mobil için)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessTouch(touch.position);
            }
        }
    }
    
    private void ProcessTouch(Vector3 screenPosition)
    {
        if (playerCamera == null) return;
        
        // Ekran pozisyonundan ray oluştur
        Ray ray = playerCamera.ScreenPointToRay(screenPosition);
        
        // Raycast yap
        if (Physics.Raycast(ray, out RaycastHit hit, maxTouchDistance, touchableLayerMask))
        {
            // ITouchable interface'i var mı kontrol et
            ITouchable touchable = hit.collider.GetComponent<ITouchable>();
            if (touchable != null && touchable.CanBeTouched)
            {
                touchable.OnTouch();
            }
        }
    }
    
    #region Debug
    void OnDrawGizmos()
    {
        // Debug için mouse pozisyonundan ray çiz
        if (playerCamera != null && Application.isPlaying)
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = playerCamera.ScreenPointToRay(mousePos);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(ray.origin, ray.direction * maxTouchDistance);
        }
    }
    #endregion
}