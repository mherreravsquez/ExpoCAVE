using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Serialization;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARInteractionManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera arCamera;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float raycastDistance = 100f;

    [SerializeField] private GameObject ARButtons;

    void OnEnable()
    {
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
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                PerformRaycast(touch.screenPosition);
            }
        }
        // For testing with webcam
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

            if (ARButtons != null)
                Instantiate(ARButtons, hit.point, Quaternion.identity);

            ARInteractable interactable = hit.collider.GetComponentInParent<ARInteractable>();
            if (interactable != null)
                interactable.OnRaycastHit(hit);
        }
    }
}