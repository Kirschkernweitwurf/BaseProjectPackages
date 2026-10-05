# Memory Profiler Automation

🔧 **Tool** · programmers only

**Menu:** `Tools > Base Packages > Unity Editor > Memory Profiler Automation`

**For:** Programmers, and anyone chasing a memory problem

Captures memory snapshots (`.snap` files) on a timer or whenever a scene finishes loading.

## Setup

1. Open the window.
2. If there is no config yet, press **Create Config Asset**. It is created in a `Resources` folder so it also works in development builds.

## Settings

| Field | What it does |
| --- | --- |
| **Enabled** | Master switch for all automatic captures |
| **Capture On Interval** | Take a snapshot on a repeating timer while playing |
| **Interval (seconds)** | How often that timer fires |
| **Capture On Scene Load** | Take a snapshot every time a scene finishes loading |
| **Snapshot Storage Path** | Where the files go. `./` paths start at the project folder. Default `./MemoryCaptures`. |
| **File Name Prefix** | Start of each file name, for example `Snapshot_2026-07-22_14-30-05.snap` |
| **Capture Flags** | Which kinds of memory go into each snapshot |

**Capture Now** takes a snapshot right away. **Open Captures Folder** shows the files. The status section shows whether automation is running and which snapshot was last.

## Good to know

- Use the same storage path as Unity's Memory Profiler (`Preferences > Analysis > Memory Profiler`), so manual and automatic captures end up together.
- Captures run in the editor and in development builds. Release builds leave it out, so leaving it enabled costs nothing there.
