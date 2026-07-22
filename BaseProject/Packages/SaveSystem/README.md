# Base Save System

Async, slot-based save and load for Unity. Each object owns its own save data and
registers itself at runtime, so gameplay code never touches files, JSON or encryption.
Drop one component into the scene, pick your settings in the inspector and you are done.

- **Async and non-blocking.** Everything runs on `Awaitable`. Encoding and decoding
  happen on a background thread so large saves never hitch the frame.
- **Crash-safe writes.** Metadata is written last as a commit marker. A crash mid-save
  can never look like a finished save.
- **Three slot models.** Fixed numbered slots, an appending list with optional auto-prune
  or unlimited named slots. Switch models without changing any calling code.
- **Optional AES encryption.** Off in the editor for readable JSON, on in builds. Reads
  always auto-detect, so plain and encrypted saves load through the same path.
- **Versioning and migrations.** Bump the schema version and add a migration step to
  upgrade old saves on load.
- **Rich metadata.** Display name, timestamps, app version, total play time and an
  optional screenshot thumbnail.
- **Ready-made UI.** Save, load, delete and select buttons plus a screenshot capturer
  and a play-time tracker.

## Requirements

- Unity 6000.3 or newer (uses `Awaitable`).
- Depends on the Base Core, Tool, Utility and Attribute packages.

## Install

Add the package to your project, then place a `SaveManager` component on a GameObject in
your first scene. It registers itself as a service and builds the whole system from its
inspector settings on `Awake`.

## Quick start

### 1. Make something savable

Implement `ISavable`. Give it a stable `PersistentKey` (never change it once shipped),
serialize your state to a string and read it back on load.

```csharp
public sealed class PlayerSaveHandler : MonoBehaviour, ISavable
{
    private static readonly PersistentKey Key = new("player");

    public PersistentKey PersistentKey => Key;
    public EPriority Priority => EPriority.High; // higher runs first on save and load

    private ISavableRegistry _registry;

    private void Start()
    {
        if (!ServiceLocator.TryGet(out SaveManager saveManager))
            return;

        _registry = saveManager.Savables;
        _registry.Register(this);
    }

    private void OnDestroy() => _registry?.Deregister(this);

    public string Serialize() => JsonUtility.ToJson(myState);

    public void Deserialize(string state)
    {
        if (string.IsNullOrEmpty(state))
            return; // null means this slot had no data for this key yet

        myState = JsonUtility.FromJson<MyState>(state);
    }
}
```

A full example lives in `Scripts/Runtime/Savable/Example`.

### 2. Save and load

You can drive the system from the ready-made buttons or call it directly.

```csharp
SaveManager manager = ...; // from ServiceLocator.TryGet
ISaveSystem saves = manager.SaveSystem;

// Save into a slot the current model resolves.
manager.Slots.TryResolveSaveTarget(manager.Selection.SelectedSlotId, out string slotId);
await saves.SaveAsync(new SaveRequest(slotId));

// Load it back. The result tells you what happened.
ESaveLoadResult result = await saves.LoadAsync(slotId);

// Before quitting, wait for any in-flight write.
await saves.FlushAsync();
```

`LoadAsync` returns `Success`, `NotFound`, `Corrupt` or `VersionTooNew` so the UI can
react instead of guessing.

### 3. Build a menu

`SaveManager.Slots.ListSlotsAsync()` returns every slot with its metadata for a load or
continue screen. `LoadScreenshotPngAsync(slotId)` gives you the thumbnail as PNG bytes;
turn it into a `Texture2D` in your UI with `tex.LoadImage(bytes)`.

## The building blocks

The system is split into small interfaces so you depend only on what you need.

| Interface | Job |
| --- | --- |
| `ISaveSystem` | The full read and write API. Splits into `ISaveReader` and `ISaveWriter`. |
| `ISavable` | An object that owns one piece of save data. |
| `ISavableRegistry` | Where savables register. Injected, not a global static. |
| `ISaveSlotProvider` | Owns slot bookkeeping for one slot model. |
| `ISaveStorage` | Raw byte storage. Swap this layer for a console save API. |
| `ISaveSerializer` | Turns objects into bytes and back. JSON by default. |
| `ISaveCodec` | Wraps serialize, encrypt and a header into one step. |
| `ISaveMigration` | Upgrades a save one version forward on load. |

### Composition

You rarely build these by hand. `SaveManager` reads its `SaveSystemSettings` and calls
`SaveSystemFactory.Create`, which picks the storage, codec, serializer, registry and slot
provider for you and hands back a `Bundle`. To add a console you add one branch inside the
factory and nothing else in the game has to change.

## Settings

All settings live on the `SaveManager` component:

- **Slot Model.** Fixed, Appending or Named, plus the slot count or save cap.
- **Encryption.** Auto (off in editor, on in build), On or Off, with a passphrase and salt.
- **Serialization.** Pretty-print JSON while developing.
- **Save Version.** Bump it when your data layout changes, then add a migration.

## How a save is stored

Each slot is a folder holding up to three files: the data, an optional screenshot and the
metadata. State is collected and applied on the main thread, while encode and decrypt work
runs on a background thread. Writes go through a gate so two saves can never interleave,
and `FlushAsync` waits for the current one to finish.