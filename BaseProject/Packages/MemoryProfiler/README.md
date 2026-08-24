# Base Memory Profiler

Automated memory snapshot capture for the Unity editor and development builds. It takes `.snap` files on a timer or on scene load, so memory usage builds up as a timeline instead of a handful of captures somebody remembered to take.

Snapshots open in Unity's own Memory Profiler window, where a single capture can be inspected or two compared to hunt leaks and track growth over a play session.

## Requirements

- Unity `6000.3` or newer
- `com.unity.memoryprofiler` `1.1.12`
- `Base.CorePackage` for timers and scene load events
- `Base.UtilityPackage` for logging and the dynamic menu attributes
- Assemblies: `Base.MemoryProfilerPackage` and `Base.MemoryProfilerPackage.Editor`

## Setup

Open the window at `Tools > Base Packages > Unity Editor > Memory Profiler Automation`. If no config exists yet, press **Create Config Asset**. It writes `MPC_MemoryProfilerConfig` under `Assets/Resources/MemoryProfilerConfig`, in a Resources folder so it loads at runtime and ships in development builds.

## Configuration

Everything lives on that one asset, edited through the window:

- **Enabled** is the master switch for all automated captures.
- **Capture On Interval** and **Interval** take a snapshot on a repeating timer while playing.
- **Capture On Scene Load** takes one every time a scene finishes loading.
- **Snapshot Storage Path** and **File Name Prefix** decide where files land and what they are called.
- **Capture Flags** picks which memory categories go into each snapshot.

**Capture Now** takes a snapshot immediately and **Open Captures Folder** reveals the output folder. The status section reports whether automation is running and names the last snapshot.

Files are named `{prefix}_{timestamp}.snap`, for example `Snapshot_2026-07-22_14-30-05.snap`.

## Storage path

The path mirrors the Memory Profiler's own **Memory Snapshot Storage Path** (`Preferences > Analysis > Memory Profiler`). Copy the same value into both so captures from this tool and manual captures land in the same place.

Paths starting with `./` or `../` resolve against the project root, absolute paths are used as is, and the default is `./MemoryCaptures`.

In a development build the resolved absolute path is baked in at build time, so a build running off a different machine still writes to the editor project folder. The baked value is cleared right after the build, so the committed asset stays machine independent.

## Builds

Auto-start runs in the editor and in development builds only. It is compiled out of release builds, where Unity does not support snapshot capture, so leaving the tool enabled costs a shipped build nothing.

## API

`MemoryProfilerRunner` is the entry point for triggering captures from your own code:

- `CaptureNow()` takes a snapshot immediately.
- `IsActive` is true while automated captures are armed.
- `LastSnapshotPath` is the path of the most recent snapshot, or null.