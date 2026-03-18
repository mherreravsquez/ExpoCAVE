using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ARInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float cooldown = 0.5f;

    [Header("Animation Steps — each click plays the next one")]
    [SerializeField] private List<AnimationStep> steps = new List<AnimationStep>();

    [Header("Cycle Behavior")]
    [SerializeField] private bool loopCycle = true; // when last step is reached, go back to 0

    private int currentStep = 0;
    private float lastHitTime = -Mathf.Infinity;
    private Sequence activeSequence;

    public void OnRaycastHit(RaycastHit hit)
    {
        if (Time.time - lastHitTime < cooldown) return;
        if (activeSequence != null && activeSequence.IsPlaying()) return;
        lastHitTime = Time.time;

        PlayStep(currentStep);

        currentStep++;
        if (currentStep >= steps.Count)
            currentStep = loopCycle ? 0 : steps.Count - 1;
    }

    private void PlayStep(int index)
    {
        if (steps == null || steps.Count == 0 || index >= steps.Count) return;

        activeSequence?.Kill(complete: false);
        activeSequence = DOTween.Sequence();

        foreach (TweenAnimation anim in steps[index].animations)
        {
            Tween t = anim.BuildTween(transform);
            if (t == null) continue;

            if (anim.sequenceMode == SequenceMode.Join)
                activeSequence.Join(t);
            else
                activeSequence.Append(t);
        }

        activeSequence.Play();
    }

    private void OnDestroy()
    {
        activeSequence?.Kill();
    }
}

// ─────────────────────────────────────────────────────────────
//  One "step" = one click. Each step holds any number of tweens.
// ─────────────────────────────────────────────────────────────

[System.Serializable]
public class AnimationStep
{
    public string label = "Step";
    public List<TweenAnimation> animations = new List<TweenAnimation>();
}

[System.Serializable]
public class TweenAnimation
{
    [Header("Type")]
    public AnimationType type = AnimationType.Move;
    public SequenceMode sequenceMode = SequenceMode.Append;

    [Header("Axis and Target")]
    public Axis axis = Axis.Y;
    public float targetValue = 1f;
    public bool relative = true;

    [Header("Timing")]
    public float duration = 0.5f;
    public float delay = 0f;
    public Ease easeType = Ease.OutQuad;

    [Header("Loop")]
    public int loops = 0;
    public LoopType loopType = LoopType.Yoyo;

    public Tween BuildTween(Transform t)
    {
        Vector3 vec = AxisToVector(axis, targetValue);
        Tween tween = null;

        switch (type)
        {
            case AnimationType.Move:
                tween = relative
                    ? t.DOLocalMove(t.localPosition + vec, duration)
                    : t.DOLocalMove(vec, duration);
                break;

            case AnimationType.Rotate:
                tween = relative
                    ? t.DOLocalRotate(t.localEulerAngles + vec, duration, RotateMode.FastBeyond360)
                    : t.DOLocalRotate(vec, duration, RotateMode.FastBeyond360);
                break;

            case AnimationType.Scale:
                tween = t.DOScale(relative ? t.localScale + vec : vec, duration);
                break;

            case AnimationType.PunchPosition:
                tween = t.DOPunchPosition(vec, duration, vibrato: 5, elasticity: 0.5f);
                break;

            case AnimationType.PunchRotation:
                tween = t.DOPunchRotation(vec, duration, vibrato: 5, elasticity: 0.5f);
                break;

            case AnimationType.PunchScale:
                tween = t.DOPunchScale(vec, duration, vibrato: 5, elasticity: 0.5f);
                break;

            case AnimationType.ShakePosition:
                tween = t.DOShakePosition(duration, strength: targetValue);
                break;

            case AnimationType.ShakeRotation:
                tween = t.DOShakeRotation(duration, strength: targetValue);
                break;
        }

        if (tween == null) return null;

        tween.SetDelay(delay).SetEase(easeType);

        if (loops != 0)
            tween.SetLoops(loops, loopType);

        return tween;
    }

    private static Vector3 AxisToVector(Axis axis, float value) => axis switch
    {
        Axis.X   => new Vector3(value, 0, 0),
        Axis.Y   => new Vector3(0, value, 0),
        Axis.Z   => new Vector3(0, 0, value),
        Axis.XY  => new Vector3(value, value, 0),
        Axis.XZ  => new Vector3(value, 0, value),
        Axis.YZ  => new Vector3(0, value, value),
        Axis.XYZ => new Vector3(value, value, value),
        _        => Vector3.zero
    };
}

public enum AnimationType { Move, Rotate, Scale, PunchPosition, PunchRotation, PunchScale, ShakePosition, ShakeRotation }
public enum Axis { X, Y, Z, XY, XZ, YZ, XYZ }
public enum SequenceMode { Append, Join }