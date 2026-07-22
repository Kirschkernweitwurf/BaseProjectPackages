# Base Memory Profiler

Automated memory snapshot capture for the Unity editor and development builds. The package
takes `.snap` files on a timer or on scene load so you build up a timeline of memory usage
without opening the Memory Profiler window and clicking Capture by hand.

Snapshots open in Unity's own Memory Profiler window, where you can inspect a single capture
or compare two of them to hunt leaks and track growth over a play session.

## Requirements

- Unity 6000.3 or newer
- `com.unity.memoryprofiler` 1.1.12 (pulled in as a dependency)

## Install

Add the package to your project through the Package Manager or your manifest, then open the
window at `Tools > Base Packages > Unity Editor > Memory Profiler Automation`.

If no config exists yet the window shows a **Create Config Asset** button. Press it to create
`MPC_MemoryProfilerConfig` under `Assets/Resources/MemoryProfilerConfig`. The asset lives in a
Resources folder so it loads at runtime and ships in development builds.

## Usage

Open the window and set up your captures:

- **Enabled** is the master switch for all automated captures.
- **Capture On Interval** takes a snapshot on a repeating timer while playing.
- **Interval (seconds)** is the time between interval captures (minimum 1, default 30).
- **Capture On Scene Load** takes a snapshot every time a scene finishes loading.
- **Snapshot Storage Path** is where `.snap` files are written.
- **File Name Prefix** is prepended to every file name.
- **Capture Flags** picks which memory categories go into each snapshot.

Two buttons let you work by hand as well:

- **Capture Now** takes a snapshot right away.
- **Open Captures Folder** reveals the output folder in your file browser.

The **Status** section shows whether automation is running and the name of the last snapshot.

Files are named `{prefix}_{timestamp}.snap`, for example `Snapshot_2026-07-22_14-30-05.snap`.

## Storage path

The storage path mirrors the Memory Profiler's own **Memory Snapshot Storage Path**
(`Preferences > Analysis > Memory Profiler`). Copy the same value into both so captures from
this tool and manual captures land in the same place.

- Paths starting with `./` or `../` resolve against the project root.
- Absolute paths are used as is.
- The default is `./MemoryCaptures`.

In a development build the tool bakes the resolved absolute path in at build time, so a build
running off a different machine still writes to the editor project folder. The baked value is
cleared right after the build so the committed asset stays machine independent.

## Builds

Auto-start runs in the editor and in development builds only. It is compiled out of release
builds, where Unity does not support snapshot capture. So enabling the tool has no cost in a
shipped release build.

## API

`MemoryProfilerRunner` is a static entry point if you want to trigger captures from your own
code:

- `MemoryProfilerRunner.CaptureNow()` takes a snapshot immediately.
- `MemoryProfilerRunner.IsActive` is true while automated captures are armed.
- `MemoryProfilerRunner.LastSnapshotPath` is the path of the most recent snapshot or null.

## Dependencies

This package builds on other Base packages for logging, timers and scene load events, plus the
Menu Manager for its window and config asset menu entries. Make sure those are present in the
project.
