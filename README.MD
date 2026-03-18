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

The scene is built around Vuforia **Image Targets**, each of which parents a 3D model with its own interactable component. A single manager object handles all raycasting from the AR camera.

```
ARScene
├── AR Camera  (Vuforia ARCamera prefab)
├── ImageTarget
│   └── 3D Model
│       ├── Collider  (MeshCollider or BoxCollider)
│       └── ARInteractable.cs
├── RaycastManager  (empty GameObject)
│   └── ARInteractionManager.cs
└── ImageTarget
    └── VuforiaTargetHandler.cs
```

### Interaction Flow

```
User touch input
       |
       v
ARInteractionManager
  Camera.ScreenPointToRay()
       |
       v
Physics.Raycast -----> hit? -----> ARInteractable.OnRaycastHit()
                                           |
                                           v
                                  Advance step index
                                           |
                                           v
                                DOTween Sequence
                             (plays all tweens in step)
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

Interaction is handled through touch-based raycasting from the AR camera onto the 3D models in the scene. Animations are driven entirely by **DOTween** — there is no Animator Controller or Unity Animation system involved.

Each model holds a list of **Animation Steps**. Every tap on the model advances to the next step and plays its configured tweens. When the last step is reached, the cycle restarts from the beginning (configurable).

### Scripts

#### `ARInteractionManager.cs`

Handles touch and mouse input using Unity's Input System package, constructs a ray from the AR camera, and dispatches the hit to the appropriate interactable object.

```csharp
private void PerformRaycast(Vector2 screenPosition)
{
    Ray ray = arCamera.ScreenPointToRay(screenPosition);

    if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactableLayer))
    {
        ARInteractable interactable = hit.collider.GetComponentInParent<ARInteractable>();
        if (interactable != null)
            interactable.OnRaycastHit(hit);
    }
}
```

#### `ARInteractable.cs`

Attached to each 3D model. Maintains a step index that advances on every valid tap and plays the corresponding DOTween sequence. A new tap is ignored if an animation is already running or the cooldown has not elapsed.

```csharp
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
```

Each step holds any number of `TweenAnimation` entries. Within a step, animations can be chained (`Append`) or played simultaneously (`Join`).

#### `VuforiaTargetHandler.cs`

Listens to Vuforia's `OnTargetStatusChanged` callback and enables or disables the model's collider depending on whether the Image Target is currently being tracked. This prevents interaction with models that are not visible in the scene.

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
| Loop Cycle | If enabled, the step index resets to 0 after the last step |
| Steps | List of animation steps. Each step plays on one tap |

#### TweenAnimation fields (per entry inside a step)

| Field | Description |
|---|---|
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

### Example Step Configurations

**Step 0 — lift and scale up:**
```
Tween 0 — Move  | Axis: Y | Target: 0.3 | Relative: true | Duration: 0.4 | Ease: OutBack  | SequenceMode: Append
Tween 1 — Scale | Axis: XYZ | Target: 1.2 | Relative: false | Duration: 0.3 | Ease: OutQuad | SequenceMode: Join
```

**Step 1 — spin and return to original scale:**
```
Tween 0 — Rotate | Axis: Y | Target: 360 | Relative: true | Duration: 0.6 | Ease: InOutSine | SequenceMode: Append
Tween 1 — Scale  | Axis: XYZ | Target: 1.0 | Relative: false | Duration: 0.3 | Ease: OutQuad  | SequenceMode: Join
```

**Step 2 — punch to signal end of cycle:**
```
Tween 0 — PunchScale | Axis: XYZ | Target: 0.3 | Duration: 0.5 | Ease: OutElastic | SequenceMode: Append
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
