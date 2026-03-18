using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARInteractionManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera arCamera;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float raycastDistance = 100f;

    [Header("Visual Feedback (optional)")]
    [SerializeField] private GameObject hitIndicatorPrefab;

    void OnEnable()
    {
        // Enhanced Touch API must be explicitly enabled
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // --- Mobile: new Input System touch ---
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                PerformRaycast(touch.screenPosition);
            }
        }
        // --- Editor / Desktop: new Input System mouse ---
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PerformRaycast(Mouse.current.position.ReadValue());
        }
    }

    private void PerformRaycast(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayer))
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name}");

            if (hitIndicatorPrefab != null)
                Instantiate(hitIndicatorPrefab, hit.point, Quaternion.identity);

            ARInteractable interactable = hit.collider.GetComponentInParent<ARInteractable>();
            if (interactable != null)
                interactable.OnRaycastHit(hit);
        }
    }
}