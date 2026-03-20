using Vuforia;
using UnityEngine;

public class VuforiaTargetHandler : MonoBehaviour
{
    private ObserverBehaviour observerBehaviour;
    [SerializeField] private Collider modelCollider;

    void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();
        if (observerBehaviour)
            observerBehaviour.OnTargetStatusChanged += OnStatusChanged;
    }

    private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool isTracked = status.Status == Status.TRACKED || 
                         status.Status == Status.EXTENDED_TRACKED;

        // Enable/disable collider based on tracking
        if (modelCollider != null)
            modelCollider.enabled = isTracked;
    }

    void OnDestroy()
    {
        if (observerBehaviour)
            observerBehaviour.OnTargetStatusChanged -= OnStatusChanged;
    }
}