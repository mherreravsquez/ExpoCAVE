using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows;
using Vuforia;
using Input = UnityEngine.Input;

public class ARInteractionManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera arCamera;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float raycastDistance = 100f;

    [Header("Visual Feedback (optional)")]
    [SerializeField] private GameObject hitIndicatorPrefab;

    void Start()
    {
        // Auto-assign Vuforia's AR camera if not set
        if (arCamera == null)
            arCamera = Camera.main;
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Supports both touch (mobile/AR) and mouse (editor testing)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            PerformRaycast(touchPosition);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            PerformRaycast(Input.mousePosition);
        }
    }

    private void PerformRaycast(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayer))
        {
            Debug.Log($"Hit: {hit.collider.gameObject.name}");

            // Spawn visual indicator at hit point (optional)
            if (hitIndicatorPrefab != null)
                Instantiate(hitIndicatorPrefab, hit.point, Quaternion.identity);

            // Try to get the interactable component on the hit object or its parents
            ARInteractable interactable = hit.collider.GetComponentInParent<ARInteractable>();
            if (interactable != null)
                interactable.OnRaycastHit(hit);
        }
    }
}