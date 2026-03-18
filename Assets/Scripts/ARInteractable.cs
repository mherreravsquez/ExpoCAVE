using UnityEngine;
using UnityEngine.Events;

public class ARInteractable : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation Triggers")]
    [SerializeField] private string animationTriggerName = "Interact";

    [Header("Unity Events — wire these in the Inspector")]
    public UnityEvent onTap;
    public UnityEvent<RaycastHit> onTapWithHitInfo;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 1f;
    private float lastHitTime = -Mathf.Infinity;

    private void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    /// Called by ARInteractionManager when this object is hit
    public void OnRaycastHit(RaycastHit hit)
    {
        if (Time.time - lastHitTime < cooldown) return;
        lastHitTime = Time.time;

        // 1. Trigger animation
        TriggerAnimation();

        // 2. Fire Unity Events (wire anything in the Inspector)
        onTap?.Invoke();
        onTapWithHitInfo?.Invoke(hit);
    }

    public void TriggerAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(animationTriggerName))
            animator.SetTrigger(animationTriggerName);
    }

    // Convenience methods you can also call from Inspector Events
    public void PlayAnimation(string triggerName) => animator?.SetTrigger(triggerName);
    public void SetBool(string paramName) => animator?.SetBool(paramName, true);
}