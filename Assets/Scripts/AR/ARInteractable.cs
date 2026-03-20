using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class ARInteractable : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float cooldown = 0.5f;

    [Header("Animation Steps — each click plays the next one")]
    [SerializeField] private List<AnimationStep> steps = new List<AnimationStep>();

    [Header("Cycle Behavior")]
    [SerializeField] private bool loopCycle = true;

    [Header("Audio Steps (ARButton only)")]
    [Tooltip("Only used when this GameObject's tag is set to ARButton.")]
    [SerializeField] private List<AudioClip> audioSteps = new List<AudioClip>();
    [SerializeField] private bool loopAudioCycle = true;
    [SerializeField] [Range(0f, 1f)] private float audioVolume = 1f;

    private int currentStep = 0;
    private int currentAudioStep = 0;
    private float lastHitTime = -Mathf.Infinity;
    private Sequence activeSequence;
    private AudioSource audioSource;
    private bool isPlayingAudio = false;

    private void Awake()
    {
        if (CompareTag("ARButton"))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = audioVolume;
        }
    }

    public void OnRaycastHit(RaycastHit hit)
    {
        if (Time.time - lastHitTime < cooldown) return;
        if (activeSequence != null && activeSequence.IsPlaying()) return;
        if (isPlayingAudio) return;
        lastHitTime = Time.time;

        bool hasSteps = steps != null && steps.Count > 0;

        if (hasSteps)
        {
            PlayStep(currentStep);

            currentStep++;
            if (currentStep >= steps.Count)
                currentStep = loopCycle ? 0 : steps.Count - 1;
        }
        else if (CompareTag("ARButton"))
        {
            TryPlayNextAudio();
        }
    }

    private void PlayStep(int index)
    {
        if (steps == null || steps.Count == 0 || index >= steps.Count) return;

        activeSequence?.Kill(complete: false);
        activeSequence = DOTween.Sequence();

        foreach (TweenAnimation anim in steps[index].animations)
        {
            // Use the override target if assigned, otherwise fall back to this transform
            Transform target = anim.targetOverride != null ? anim.targetOverride : transform;

            Tween t = anim.BuildTween(target);
            if (t == null) continue;

            if (anim.sequenceMode == SequenceMode.Join)
                activeSequence.Join(t);
            else
                activeSequence.Append(t);
        }

        if (CompareTag("ARButton") && audioSteps != null && audioSteps.Count > 0)
            activeSequence.OnComplete(() => TryPlayNextAudio());

        activeSequence.Play();
    }

    private void TryPlayNextAudio()
    {
        if (audioSource == null) return;
        if (audioSteps == null || audioSteps.Count == 0) return;
        if (isPlayingAudio) return;

        AudioClip clip = audioSteps[currentAudioStep];

        if (clip != null)
            StartCoroutine(PlayAudioClip(clip));

        currentAudioStep++;
        if (currentAudioStep >= audioSteps.Count)
            currentAudioStep = loopAudioCycle ? 0 : audioSteps.Count - 1;
    }

    private IEnumerator PlayAudioClip(AudioClip clip)
    {
        isPlayingAudio = true;
        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitWhile(() => audioSource.isPlaying);

        isPlayingAudio = false;
    }

    private void OnDestroy()
    {
        activeSequence?.Kill();
        StopAllCoroutines();
    }
}

// ─────────────────────────────────────────────────────────────
//  Each animation plays after the old one finishes
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
    [Header("Target")]
    [Tooltip("GameObject to animate. If left empty, the GameObject with ARInteractable will be used.")]
    public Transform targetOverride;

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