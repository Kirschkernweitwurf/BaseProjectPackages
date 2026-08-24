# Base Content

Every prefab and configured asset the other Base packages are wired together with. This package holds no code at all.

It exists because composition and code pull in opposite directions: a prefab carrying an `AudioManager`, a `MenuManager` and a save button needs three packages installed, while each of those packages on its own should install into an empty project and compile. Keeping the wiring here is what lets that stay true.

## Requirements

- Unity `6000.3` or newer
- `Base.ControllerSupportPackage`, `Base.SaveSystemPackage`, `Base.SettingsPackage` and `Base.UiPackage`

Those pull in Core, Services, Tweening, Attributes and Utility in turn. Install through the Git Package Manager rather than by pasting the Git URL: the prefabs here reference components from the other Base packages, so installing this one on its own leaves them with missing scripts.

## Layout

```
Assets/
  Audio/
    Mixers/            AudioMixer with its Master, SFX, Ambience, UI and Music groups
    UI/Click/          sample click clips
    UI/Hover/          sample hover clips
  Prefabs/
    Audio/             pooled audio source prefabs, one per EAudioType
    Bootstrap/
      Resources/       Bootstrapper, loadable by name from a build
    Managers/          persistent, scene and gameplay manager roots
    UI/
      Buttons/         basic text and image buttons
      Canvases/        persistent, gameplay overlay and gameplay world canvas
      DebugMenu/       debug menu with its cheat console and log console
      Menus/           confirmation menu and loading screen
      Tooltip/         tooltip view
      Widgets/         small standalone readouts such as the FPS text
  ScriptableObjects/
    AudioContainers/   clip sets the OnEvent audio components play
    MenuIdentifiers/   menu identity assets the MenuManager resolves menus by
  Sprites/             shared UI sprites
```

## Managers and bootstrap

**Bootstrapper** instantiates the three manager prefabs below. It sits in a `Resources` folder so it can be pulled into a scene or a build without a direct reference.

- **PersistentManagers** is instantiated once per session and survives scene loads. It carries the services that outlive a scene.
- **SceneManagers** is instantiated for every scene.
- **GameplayManagers** is instantiated only while one of the scenes configured on the `Bootstrapper` is loaded.

## UI prefabs

**PersistentCanvas**, **GameplayOverlayCanvas** and **GameplayWorldCanvas** are the screen space and world space canvas roots. **ConfirmationMenu** backs the awaitable confirmation dialog, **LoadingScreen** is shown by the `SceneLoadingManager` while a scene loads, and **Tooltip** is the view the `TooltipService` positions.

**DebugMenu** hosts the **CheatConsole** and the **LogConsole**, with **Suggestion** as the row the cheat console builds its autocomplete from. **BasicTextButton** and **BasicImageButton** are the starting points for menu buttons, with the click and hover audio containers already assigned. **FpsText (TMP)** is the frames per second readout.

## Audio

The four pooled source prefabs (2D, 3D, Music and UI) are what the `AudioManager` plays through, one per `EAudioType`, each routed to its own group on the **AudioMixer**. `AudioVolumeSetting` from the Settings package pushes the player's volume into that mixer's exposed parameters.

The UI click and hover clips are samples meant to be replaced. What the prefabs reference are the **AUC_Click** and **AUC_Hover** audio containers, so swapping the clips inside them keeps every prefab wired.

## Menu identifiers

Identifier assets for the confirmation menu, loading screen, debug menu, cheat console, log console, main menu, pause menu and credits menu. Menus are resolved by identifier rather than by name, so an identifier has to exist before a menu can be opened.