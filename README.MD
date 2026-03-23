# ExpoCAVE

**An interactive Augmented Reality exhibition about the CAVE system, its evolution, and its contemporary applications.**

Developed by Antonia Benz and Marcelo Herrera. Published by Plants Path Collective.

Project documentation: [Overview (Coda)](https://coda.io/d/_dC5pWoFBqtU/Overview_sutEokes)

---

## Table of Contents

- [Project Overview](#project-overview)
- [Technical Sheet](#technical-sheet)
- [Development Team](#development-team)
- [Development Tools](#development-tools)
- [Project Structure](#project-structure)
- [Scene Architecture](#scene-architecture)
- [Installation and Setup](#installation-and-setup)
- [AR Interaction System](#ar-interaction-system)
- [Important Links](#important-links)

---

## Project Overview

ExpoCAVE is an Augmented Reality experience built as an interactive exhibition focused on the **CAVE** system (*Cave Automatic Virtual Environment*). The project covers the origin of the technology, its development over the decades, and the ways in which it is used across different industries today.

Users point their mobile device at physical markers to trigger interactive 3D content — animated models, visual timelines, and contextual information — layered directly over the real world through the device camera.

---

## Technical Sheet

| Field | Detail |
|---|---|
| Project Name | ExpoCAVE |
| Start Date | March 13, 2026 |
| End Date | March 27, 2026 |
| Published by | Plants Path Collective |
| Repository | [github.com/mherreravsquez/ExpoCAVE](https://github.com/mherreravsquez/ExpoCAVE) |

---

## Development Team

| Name | Role |
|---|---|
| Antonia Benz | 3D Artist |
| Marcelo Herrera | Programmer, 3D Unity Generalist |

---

## Development Tools

| Tool | Version |
|---|---|
| [Unity](https://unity.com/) | 6000.3.8f1 (Unity 6) |
| [Vuforia Engine](https://developer.vuforia.com/) | 1.1.4 |
| [DOTween](http://dotween.demigiant.com/) | latest |
| [Blender](https://www.blender.org/) | 4.3.2 |

---

## Project Structure

The following reflects the actual directory layout of the Unity project:

```
ExpoCAVE/
├── Assets/
│   ├── Editor/
│   │   └── Models/          # 3D models used within the Editor context
│   ├── Plugins/             # Third-party plugins (firstpass)
│   ├── References/          # Reference materials and assets
│   ├── Resources/           # Runtime-loaded assets
│   ├── Scenes/              # Unity scene files (.unity)
│   ├── Scripts/
│   │   ├── ARInteractable.cs
│   │   ├── ARInteractionManager.cs
│   │   └── VuforiaTargetHandler.cs
│   ├── Settings/            # Render Pipeline and project settings assets
│   ├── Sprites/             # 2D sprites and UI textures
│   └── StreamingAssets/     # Vuforia database files (.xml / .dat)
├── Packages/
│   └── manifest.json        # Package dependencies including Vuforia and DOTween
└── ProjectSettings/
```

---

## Scene Architecture

Each AR marker follows the same hierarchy. The `ImageTarget` owns a single child that Vuforia enables and disables on tracking. All interactable content lives inside that child.

```
ImageTarget
├── VuforiaTargetHandler.cs
│
└── [GameObject to show]              (Vuforia activates/deactivates this)
      │                                no scripts · no collider · no layer
      │
      └── [Collider to raycast and Scripts]
            ├── MeshCollider / BoxCollider
            ├── Layer: Interactable
            └── ARInteractable.cs
                  Steps        → DOTween animations targeting child meshes via Target Override
                  Audio Clips  → narration clips, one per tap
                  Feedback     → shrink/expand on the assigned Feedback Mesh
```

### GameObject Responsibilities

| Object | Layer | Collider | Script | Purpose |
|---|---|---|---|---|
| `ImageTarget` | — | — | `VuforiaTargetHandler.cs` | Vuforia marker root |
| `[GameObject to show]` | — | — | — | Vuforia visibility container |
| `[Collider to raycast and Scripts]` | `Interactable` | Yes | `ARInteractable.cs` | Tap target, drives all audio and animation |
| Visual mesh children | — | — | — | Visual only, referenced via Target Override |

### Interaction Flow

```
Model appears (OnEnable)
  state resets — model is silent, waiting for input

User taps
       |
       v
ARInteractionManager
  Camera.ScreenPointToRay()
       |
       v
Physics.Raycast -----> hit? -----> ARInteractable.OnRaycastHit()
                                           |
                              is audio playing?     --yes--> blocked
                              is animation running? --yes--> blocked
                                           |
                                           v
                                    PlayFeedback()
                                 (shrink + expand on feedbackMesh)
                                           |
                                           v
                                  Play audioClips[currentStep]
                                  (WaitWhile playing)
                                           |
                                           v
                                  Play DOTween step[currentStep]
                                  (animation fires as transition)
                                           |
                                  Advance step index
```

So for a three-state object (e.g. ice → water → cloud):

```
Tap 1  →  clip[0] plays  →  step[0] animates  (solid state)
Tap 2  →  clip[1] plays  →  step[1] animates  (liquid state)
Tap 3  →  clip[2] plays  →  step[2] animates  (gas state)
```

---

## Installation and Setup

### Prerequisites

- Unity **6000.3.8f1**
- Vuforia Engine **1.1.4**, installed via Unity Package Manager
- DOTween, installed via Unity Package Manager or the Asset Store

### Steps

1. **Clone the repository**

   ```bash
   git clone https://github.com/mherreravsquez/ExpoCAVE.git
   ```

2. **Open in Unity**

   Open Unity Hub, select *Add project from disk*, and point it to the cloned folder. Make sure to open it with version **6000.3.8f1**.

3. **Setup DOTween**

   After the project opens, go to **Tools > DOTween Utility Panel** and click **Setup DOTween**. This generates the required settings asset and registers the `DOTWEEN` scripting define symbol. The project will not compile without this step.

4. **Configure Vuforia**

   - Navigate to `Edit > Project Settings > Vuforia Configuration`
   - Enter your **Vuforia License Key**, available at [developer.vuforia.com](https://developer.vuforia.com/)
   - Upload and configure your **Image Targets** in the Vuforia Target Manager, then import the generated database into `Assets/StreamingAssets/`

5. **Build Settings**

   - Go to `File > Build Settings`
   - Select **Android** or **iOS** as the target platform
   - Enable **ARCore** (Android) or **ARKit** (iOS) under Player Settings
   - Grant **Camera** permissions in Player Settings

---

## AR Interaction System

Interaction is handled through touch-based raycasting from the AR camera. Animations are driven entirely by **DOTween** — there is no Animator Controller or Unity Animation system involved. A single script, `ARInteractable`, handles all tap input, audio narration, visual feedback, and DOTween animation.

The interaction model is **audio-first**: every tap plays the narration clip for that step first, and only once the clip finishes does the transition animation fire. This means the audio narrates what is about to happen, and the model physically transitions into the new state after the explanation ends. Input is blocked for the full duration of both the clip and the animation, so the user cannot skip ahead.

The model appears silently when the image target is detected. No audio plays automatically — the first clip only triggers on the first tap.

### Scripts

#### `ARInteractionManager.cs`

Handles touch and mouse input using Unity's Input System package, constructs a ray from the AR camera, and dispatches the hit to `ARInteractable`.

```csharp
private void PerformRaycast(Vector2 screenPosition)
{
    Ray ray = arCamera.ScreenPointToRay(screenPosition);

    if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayer))
        return;

    ARInteractable interactable = hit.collider.GetComponentInParent<ARInteractable>();
    if (interactable != null)
        interactable.OnRaycastHit(hit);
}
```

#### `ARInteractable.cs`

The single script that manages the entire interaction lifecycle for a model. On a valid tap it plays the shrink/expand feedback, then starts a coroutine that plays the audio clip and waits for it to finish before firing the DOTween animation step.

```csharp
public void OnRaycastHit(RaycastHit hit)
{
    if (isPlayingAudio) return;
    if (IsAnimating) return;
    if (Time.time - lastHitTime < cooldown) return;
    lastHitTime = Time.time;

    if (steps == null || steps.Count == 0) return;

    PlayFeedback();

    int clipIndex = currentStep;
    int stepIndex = currentStep;

    currentStep++;
    if (currentStep >= steps.Count)
        currentStep = loopCycle ? 0 : steps.Count - 1;

    StartCoroutine(PlayClipThenAnimate(clipIndex, stepIndex));
}
```

The coroutine enforces the audio-first order. `yield return StartCoroutine(PlayClipAndWait(...))` blocks until the clip finishes, then `PlayStep` fires immediately after:

```csharp
private IEnumerator PlayClipThenAnimate(int clipIndex, int stepIndex)
{
    yield return StartCoroutine(PlayClipAndWait(clipIndex));
    PlayStep(stepIndex);
}
```

The feedback animation runs on its own independent `feedbackSequence` so it plays immediately on tap without waiting for audio:

```csharp
private void PlayFeedback()
{
    feedbackSequence?.Kill(complete: false);
    Vector3 original = feedbackMesh.localScale;

    feedbackSequence = DOTween.Sequence();
    feedbackSequence.Append(
        feedbackMesh.DOScale(original * shrinkScale, shrinkDuration).SetEase(Ease.OutQuad)
    );
    feedbackSequence.Append(
        feedbackMesh.DOScale(original, expandDuration).SetEase(Ease.OutBack)
    );
    feedbackSequence.Play();
}
```

Inside `PlayStep`, each tween resolves its target. If `targetOverride` is assigned that transform is used, otherwise the script falls back to its own transform:

```csharp
Transform target = anim.targetOverride != null ? anim.targetOverride : transform;
Tween t = anim.BuildTween(target);
```

When the image target is lost, `OnDisable` kills all sequences, stops audio, and resets the `isPlayingAudio` flag so there are no stuck states on the next detection. `OnEnable` resets the step index and cooldown cleanly without playing anything.

#### `VuforiaTargetHandler.cs`

Attached to the `ImageTarget`. Enables or disables the child colliders depending on whether the marker is currently tracked.

```csharp
private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
{
    bool isTracked = status.Status == Status.TRACKED ||
                     status.Status == Status.EXTENDED_TRACKED;
    if (modelCollider != null)
        modelCollider.enabled = isTracked;
}
```

### Inspector Reference

#### ARInteractionManager

| Field | Recommended Value |
|---|---|
| AR Camera | Vuforia `ARCamera` object |
| Interactable Layer | Custom layer named `Interactable` |
| Raycast Distance | `100` |

#### ARInteractable

| Field | Description |
|---|---|
| Cooldown | Minimum seconds between taps. Default: `0.5` |
| Steps | List of animation steps. Each step plays as a transition after its clip ends |
| Loop Cycle | If enabled, the step index resets to 0 after the last step |
| Audio Clips | Clip at index N plays on tap N, before animation step N fires |
| Audio Volume | Playback volume for all clips. Range: 0 to 1 |
| Feedback Mesh | The transform that plays the shrink/expand animation on tap. Falls back to this transform if empty |
| Shrink Scale | Scale multiplier applied on tap. Default: `0.85` |
| Shrink Duration | Duration of the shrink phase in seconds. Default: `0.08` |
| Expand Duration | Duration of the expand phase in seconds. Default: `0.15` |

#### TweenAnimation fields (per entry inside a step)

| Field | Description |
|---|---|
| Target Override | Reference to the GameObject to animate. If left empty, the script's own transform is used |
| Type | `Move`, `Rotate`, `Scale`, `PunchPosition`, `PunchRotation`, `PunchScale`, `ShakePosition`, `ShakeRotation` |
| Sequence Mode | `Append` plays after the previous tween; `Join` plays at the same time |
| Axis | Local axis or combination to affect: `X`, `Y`, `Z`, `XY`, `XZ`, `YZ`, `XYZ` |
| Target Value | Distance in units, degrees of rotation, or scale amount |
| Relative | If enabled, the value is an offset from the current transform; otherwise it is an absolute target |
| Duration | Seconds to complete the tween |
| Delay | Seconds to wait before this tween starts |
| Ease Type | DOTween easing curve |
| Loops | `0` = play once, `-1` = infinite, any positive number = repeat count |
| Loop Type | `Yoyo` = ping-pong, `Restart` = reset and repeat |

### Example Configuration

A three-state model representing the water cycle:

```
[Collider to raycast and Scripts]
  Layer: Interactable
  ARInteractable.cs
    Cooldown:    0.5
    Loop Cycle:  false
    Feedback Mesh → [visual mesh child]

    Steps:
      Step 0 — "Ice to Water"
        Tween 0:
          Target Override → [IceMesh]
          Type            → Scale
          Axis            → XYZ
          Target Value    → 0.0
          Relative        → false
          Duration        → 0.6
          Ease Type       → InBack
        Tween 1:
          Target Override → [WaterMesh]
          Type            → Scale
          Axis            → XYZ
          Target Value    → 1.0
          Relative        → false
          Duration        → 0.6
          Ease Type       → OutBack
          Sequence Mode   → Join

      Step 1 — "Water to Cloud"
        Tween 0:
          Target Override → [WaterMesh]
          Type            → Scale
          Axis            → XYZ
          Target Value    → 0.0
          Relative        → false
          Duration        → 0.6
          Ease Type       → InBack
        Tween 1:
          Target Override → [CloudMesh]
          Type            → Scale
          Axis            → XYZ
          Target Value    → 1.0
          Relative        → false
          Duration        → 0.6
          Ease Type       → OutBack
          Sequence Mode   → Join

    Audio Clips:
      [0] → ice_narration.wav    (tap 1: explains solid state, then ice→water transition plays)
      [1] → water_narration.wav  (tap 2: explains liquid state, then water→cloud transition plays)
```

---

## Important Links

| Resource | URL |
|---|---|
| Project Documentation (Coda) | [Overview — ExpoCAVE](https://coda.io/d/_dC5pWoFBqtU/Overview_sutEokes) |
| GitHub Repository | [github.com/mherreravsquez/ExpoCAVE](https://github.com/mherreravsquez/ExpoCAVE) |
| Vuforia Developer Portal | [developer.vuforia.com](https://developer.vuforia.com/) |
| DOTween | [dotween.demigiant.com](http://dotween.demigiant.com/) |
| Unity Release Archive | [unity.com/releases/editor/archive](https://unity.com/releases/editor/archive) |
| Blender | [blender.org](https://www.blender.org/) |

---

### Disclaimer

This readme file was written by AI and can contain mistakes. Yeah, Im too lazy to write documentation, sorry.

---

*ExpoCAVE — Plants Path Collective, March 2026.*
*Antonia Benz & Marcelo Herrera*
