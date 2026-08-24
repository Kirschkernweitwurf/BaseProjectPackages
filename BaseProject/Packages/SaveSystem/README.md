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
- **Versioning and migrations.** Bump the schema version and write a migration step; it is
  found and registered for you, and old saves are walked up one version at a time on load.
- **Damaged saves are survivable.** Every file carries a checksum, and each slot keeps a
  configurable number of previous versions. A save that no longer reads falls back to the
  newest intact backup instead of ending the playthrough.
- **Autosave with a cooldown.** A timer offers a save, a cooldown decides whether it is
  allowed, so a run of checkpoints cannot write five saves in as many seconds.
- **Rich metadata.** Display name, timestamps, app version, total play time and an
  optional screenshot thumbnail.
- **Ready-made UI.** Save, load, delete and select buttons plus a screenshot capturer
  and a play-time tracker.

## Requirements

- Unity 6000.3 or newer (uses `Awaitable`).
- Depends on the Base Service, Utility and Attribute packages. Not on the Core package: the
  save system needs the service locator and the shutdown pipeline, both of which live in the
  Service package.

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

A full example lives in `Runtime/Savable/Example`.

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

`LoadAsync` returns `Success`, `NotFound`, `Corrupt`, `VersionTooNew` or
`RecoveredFromBackup` so the UI can react instead of guessing.

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
| `ISaveBackups` | Keeps previous versions of a slot and restores one. |
| `AutosaveService` | Saves on a timer and on request, behind a cooldown. |

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
- **Auto Discover Migrations.** On by default. Off means you hand the steps to
  `SaveSystemFactory.Create` yourself.
- **Kept Backups.** How many previous versions of each slot to keep. 0 turns backups off.

## Versioning and migrations

Set **Save Version** to whatever your current data layout is. When you change that layout,
bump it and write one step per version:

```csharp
public sealed class MigrateV1ToV2 : ISaveMigration
{
    public int FromVersion => 1;

    public void Migrate(IDictionary<string, string> states)
    {
        if (!states.TryGetValue("player", out string json))
            return;

        states["player"] = RewriteToNewShape(json);
    }
}
```

That is all. Migrations are found automatically, so nothing has to reference the class. A
step needs a public parameterless constructor; a save is stepped up one version at a time
until it reaches the current one.

The chain is checked at startup rather than on the first old save a player happens to load.
Two steps starting at the same version, a step that can never run and a gap in the chain are
all reported to the console right away.

A save written by a **newer** build than the one reading it loads as `VersionTooNew`. There
is no path down, on purpose: the newer build knows about data this one does not.

## When a save goes bad

Two layers, and both are on by default.

**Detection.** Every file carries a checksum of its payload in the header. A truncated,
half-written or hand-edited file is caught while reading the header, so it surfaces as
`Corrupt` instead of as a strange parse error or, worse, as silently wrong state. Files
written before this existed are still read, they just cannot be checked.

**Backups.** Right before a slot is overwritten, its current files are copied aside into a
timestamped folder. That is the one moment a known-good copy is guaranteed to exist. Only
the previous save is ever copied, whatever the kept count is, so raising the count costs
disk space and not save time.

A load that cannot read the live save walks the backups newest first, uses the first one that
decodes and returns `RecoveredFromBackup`. Nothing else has to be wired up for this. A backup
counts as complete only once its metadata is copied, the same commit-marker rule the live save
follows, so a rotation interrupted halfway is skipped rather than trusted.

Metadata falls back the same way. A slot whose marker went bad still appears in a load menu,
using the newest backup's metadata, rather than vanishing from the menu while staying perfectly
loadable.

Deleting a slot deletes its backups too, so a deleted save cannot come back to life.

For a "restore an older version" menu, `SaveManager.Backups` lists the generations and puts
one back:

```csharp
IReadOnlyList<SaveBackupInfo> backups = await manager.Backups.ListAsync(slotId);

await manager.SaveSystem.FlushAsync();
await manager.Backups.RestoreAsync(slotId, backups[0].Id);
```

## Autosave

Create an `AutosaveConfig` asset, point an `AutosaveService` in the scene at it and put that
next to the `SaveManager`. The service registers itself, so gameplay code only ever asks:

```csharp
if (ServiceLocator.TryGet(out AutosaveService autosave))
    autosave.Request(); // reached a checkpoint
```

Two numbers, and both matter. The **interval** decides how often a save is offered. The
**cooldown** decides how often one is allowed. A request is remembered rather than dropped: it
runs as soon as the cooldown has passed, so a burst of checkpoints turns into exactly one save.

Everything else lives on the config asset:

- **Target.** A dedicated slot of its own, so the timer can never overwrite a save the player
  made, or the slot the player currently has selected. With nothing selected the request waits
  for a selection rather than minting a slot per interval.
- **Save On Focus Loss.** Saves when the app is backgrounded, ignoring the cooldown, because
  there may not be another frame.
- **Suspend / Resume.** For cutscenes and menus where a save would be wrong. Kept apart from
  the on/off switch below, so leaving a cutscene cannot re-enable an autosave the player
  turned off.

`Saved` and `Failed` fire with the slot id, for a small "Saving..." indicator.

### Player settings

With the Base Settings package installed, three ready-made setting components persist the
player's choices and push them into the service: `AutosaveEnabledSetting` (on/off),
`AutosaveIntervalSetting` (how often) and `AutosaveCooldownSetting` (the shortest gap, which
most projects keep as a developer decision and do not expose).

Point each of them at the same `AutosaveConfig` the service uses. The default a setting falls
back to then comes from that one asset, so the service and the menu cannot drift apart.

They live in their own assembly, `Base.SaveSystemPackage.Settings`, gated behind a version
define. Without the Settings package installed the assembly simply does not compile in, so the
save system keeps working and gains no dependency on it.

## How a save is stored

Each slot is a folder holding up to three files: the data, an optional screenshot and the
metadata, plus one `Backup_<timestamp>` folder per kept generation. State is collected and
applied on the main thread, while encode and decrypt work runs on a background thread. Writes
go through a gate so two saves can never interleave, and `FlushAsync` waits for the current
one to finish.