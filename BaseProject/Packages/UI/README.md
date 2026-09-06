# Base UI

Reusable UI building blocks: click-driven button components, an awaitable confirmation dialog and a set of small utility components, so the same UI systems drop into any project without rewriting them.

Code only. The ready-made button prefabs and UI sprites live in `Base.ContentPackage`, together with the rest of the prefabs that wire the Base packages together.

## Requirements

- Unity `6000.3` or newer
- `com.unity.ugui` `2.0.0` and TextMeshPro
- `Base.CorePackage.MenuManaging` for the `MenuManager` and menu identifiers,
  `Base.CorePackage.SceneManagement` for the `SceneLoadingManager` and
  `Base.CorePackage.CameraUtility` for the `CameraProvider` the billboards resolve through
- `Base.ServicesPackage` for `ServiceLocator` and `GameServiceBehaviour`
- `Base.AttributesPackage` for `[Required]`, `[GetComponent]`, `[NotNullOrEmpty]` and `[SceneName]`
- `Base.UtilityPackage` for `CustomLogger` and `Platform`
- Assemblies: `Base.UIPackage` and `Base.UIPackage.Editor`

Installing `Base Core` brings the Service and Tweening packages with it, so the Git Package Manager selects those automatically.

## Buttons

Every button derives from `CustomButton`, an abstract MonoBehaviour that requires a `Button` and wires its own `OnClick` handler on `Awake`. Subclasses only implement the behavior they need.

| Component | What it does |
|---|---|
| `CustomButton` | Abstract base. Hooks and unhooks the `Button.onClick` listener |
| `OpenMenuButton` | Opens a target menu through the `MenuManager`, with an optional parent menu that stays registered as its owner |
| `CloseMenuButton` | Closes a target menu if it is currently open |
| `PauseMenuButton` | Toggles the pause menu and swaps its icon between play and pause, following `PauseMenu.OnPauseStateChanged` |
| `LoadSceneButton` | Unloads all scenes and additively loads a chosen scene through the `SceneLoadingManager` |
| `OpenLinkOnClick` | Opens a URL in the default browser |

## Confirmation

An asynchronous flow for actions that need the player to agree before they run, such as quitting or leaving a scene.

`ConfirmationService` is a `GameServiceBehaviour` exposing an awaitable `ShowConfirmationAsync`. Only one confirmation runs at a time; concurrent requests are denied rather than queued. `ConfirmationRequest` carries the message and the optional confirm and cancel labels, and `ConfirmationMenu` shows them, falling back to defaults when none are given.

`BaseConfirmationButton` is the abstract button that shows the prompt and calls `OnConfirm` or `OnCancel` based on the answer. `ConfirmedLoadSceneButton` and `ConfirmedQuitButton` are the two ready-made ones.

```csharp
if (!ServiceLocator.TryGet(out ConfirmationService confirmation))
    return;

bool confirmed = await confirmation.ShowConfirmationAsync(new ConfirmationRequest("Quit the game?"));
```

## Utility

| Component | What it does |
|---|---|
| `Billboard` | Rotates an object to face the main camera at runtime, optionally locked to the Y axis so it stays upright |
| `EditorBillboard` | Faces the viewing camera in play mode and the scene view, for authoring |
| `WorldCanvasWrapper` | Assigns the main camera as the world camera of a world-space `Canvas` |
| `FpsCounter` | Shows the current frames per second in a `TMP_Text`, hidden in release builds unless explicitly enabled |
| `BuildVersion` | Displays the version and build number read from `version.txt` in StreamingAssets |
| `BuildVersionFile` | Reads, counts and formats that file, apart from the component that shows it |
| `BuildVersionProcessor` | Build step that writes the date-version and increments the build number into that file before every build |

`SceneLoader` sits behind `LoadSceneButton` and `ConfirmedLoadSceneButton` as the shared resolving, awaiting and error-logging path, so both behave identically.