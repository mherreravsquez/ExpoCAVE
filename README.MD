# ExpoCAVE

**An interactive Augmented Reality exhibition about the CAVE system, its evolution, and its contemporary applications.**

Developed by Antonia Benz and Marcelo Herrera. Published by Plants Path Collective.

Project documentation: [Overview (Coda)](https://coda.io/d/_dC5pWoFBqtU/Overview_sutEokes)

---

## Table of Contents

- [Project Overview](#project-overview)
- [Technical Sheet](#technical-sheet)
- [Development Team](#development-team)
- [Tools and Technologies](#tools-and-technologies)
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

| Name | Role                            |
|---|---------------------------------|
| Antonia Benz | 3D Artist                       |
| Marcelo Herrera | Programmer, 3D Unity Generalist |

---

## Development tools

| Tool | Version |
|---|---|
| [Unity](https://unity.com/) | 6000.3.8f1 (Unity 6) |
| [Vuforia Engine](https://developer.vuforia.com/) | 1.1.4 |
| [Blender](https://www.blender.org/) | 4.3.2 |

---

## Project Structure

The following reflects the actual directory layout of the Unity project:

```
ExpoCAVE/
├── Assets/
│   ├── Animations/          # AnimationClips and AnimatorControllers
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
│   └── manifest.json        # Package dependencies including Vuforia
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
│       ├── Animator
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
                              ┌────────────┴────────────┐
                              v                         v
                    Animator.SetTrigger()          UnityEvents
                     (plays animation)       (audio, UI, particles, etc.)
```

---

## Installation and Setup

### Prerequisites

- Unity **6000.3.8f1**
- Vuforia Engine **1.1.4**, installed via Unity Package Manager

### Steps

1. **Clone the repository**

   ```bash
   git clone https://github.com/mherreravsquez/ExpoCAVE.git
   ```

2. **Open in Unity**

   Open Unity Hub, select *Add project from disk*, and point it to the cloned folder. Make sure to open it with version **6000.3.8f1**.

3. **Configure Vuforia**

    - Navigate to `Edit > Project Settings > Vuforia Configuration`
    - Enter your **Vuforia License Key**, available at [developer.vuforia.com](https://developer.vuforia.com/)
    - Upload and configure your **Image Targets** in the Vuforia Target Manager, then import the generated database into `Assets/StreamingAssets/`

4. **Build Settings**

    - Go to `File > Build Settings`
    - Select **Android** or **iOS** as the target platform
    - Enable **ARCore** (Android) or **ARKit** (iOS) under Player Settings
    - Grant **Camera** permissions in Player Settings

---

## AR Interaction System

Interaction is handled through touch-based raycasting from the AR camera onto the 3D models in the scene. When a user taps a model, the corresponding `ARInteractable` component receives the hit and fires its configured responses — animations, sounds, UI updates, or any other Unity Event.

### Scripts

#### `ARInteractionManager.cs`

Handles touch and mouse input, constructs a ray from the camera, and dispatches the hit to the appropriate interactable object.

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

Attached to each 3D model. Receives the raycast hit, triggers the Animator, and invokes the Unity Events configured in the Inspector.

```csharp
public UnityEvent onTap;
public UnityEvent<RaycastHit> onTapWithHitInfo;

public void OnRaycastHit(RaycastHit hit)
{
    if (Time.time - lastHitTime < cooldown) return;
    lastHitTime = Time.time;

    animator?.SetTrigger(animationTriggerName);
    onTap?.Invoke();
    onTapWithHitInfo?.Invoke(hit);
}
```

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

| Component | Field | Recommended Value |
|---|---|---|
| `ARInteractionManager` | AR Camera | Vuforia `ARCamera` object |
| `ARInteractionManager` | Interactable Layer | Custom layer named `Interactable` |
| `ARInteractionManager` | Raycast Distance | `100` |
| `ARInteractable` | Animation Trigger Name | Must match the Trigger parameter in the Animator |
| `ARInteractable` | Cooldown | `1` second |

### Animator Setup

Inside the **Animator Controller** assigned to the model:

1. Add a **Trigger** parameter — for example, `Interact`
2. Create a transition from **Any State** to the target animation clip
3. Set the trigger as the transition condition
4. Uncheck *Can Transition To Self* if the animation should not restart while already playing

---

## Important Links

| Resource | URL |
|---|---|
| Project Documentation (Coda) | [Overview — ExpoCAVE](https://coda.io/d/_dC5pWoFBqtU/Overview_sutEokes) |
| GitHub Repository | [github.com/mherreravsquez/ExpoCAVE](https://github.com/mherreravsquez/ExpoCAVE) |
| Vuforia Developer Portal | [developer.vuforia.com](https://developer.vuforia.com/) |
| Unity Release Archive | [unity.com/releases/editor/archive](https://unity.com/releases/editor/archive) |
| Blender | [blender.org](https://www.blender.org/) |

---

### Disclaimer

This readme file was written by AI and can contain mistakes. Yeah, Im too lazy to write documentation, sorry.

---

*ExpoCAVE — Plants Path Collective, March 2026.*
*Antonia Benz & Marcelo Herrera*