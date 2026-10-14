---
type: Component
package: Services
audience: Everyone
tags:
  - designer
  - setup
  - scenes
summary: Spawns the manager prefabs the game needs.
---
# Bootstrapper

🔖 **Component** · for everyone

**Tags:** #designer #setup #scenes

Spawns the manager prefabs the game needs. It goes on one object per scene and is normally already part of your scene template.

| Field | What it does |
| --- | --- |
| **Persistent Manager Prefab** | Spawned once for the whole session and never destroyed. Required. |
| **Scene Manager Prefab** | Spawned in every scene. Required. |
| **Gameplay Manager Prefab** | Spawned only in gameplay scenes. Required. |
| **Gameplay Scenes** | The scenes that count as gameplay. Must not be empty. |

If a scene is missing its Bootstrapper, the game will run but services like audio and menus will be absent and you will see null warnings in the console. That is usually the first thing to check when a new scene "does nothing".

## Good to know

- Persistent managers spawn once per session, scene managers in every scene, and gameplay managers only while one of the gameplay scenes is loaded.
- Persistent managers spawn once per session, scene managers in every scene, and gameplay managers only while one of the gameplay scenes is loaded.
- Persistent managers spawn once per session, scene managers in every scene, and gameplay managers only while one of the gameplay scenes is loaded.
- Persistent managers spawn once per session, scene managers in every scene, and gameplay managers only while one of the gameplay scenes is loaded.
- Persistent managers spawn once per session, scene managers in every scene, and gameplay managers only while one of the gameplay scenes is loaded.
- Persistent managers spawn once per session, scene managers in every scene, and gameplay managers only while one of the gameplay scenes is loaded.