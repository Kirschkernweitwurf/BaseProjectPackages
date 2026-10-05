# Memory Profiler Package

Takes memory snapshots automatically, on a timer or whenever a scene loads, so you get a timeline of memory use instead of a few snapshots someone remembered to take. Open the snapshots in Unity's own Memory Profiler window to compare them and hunt leaks.

## Tools

| Page | What it does | Menu path |
| --- | --- | --- |
| [🔧 Memory Profiler Automation](tools/memory-profiler-automation.md) | Sets up and runs the automatic captures | `Tools > Base Packages > Unity Editor > Memory Profiler Automation` |

## Scriptable Objects

| Asset | What it is | File |
| --- | --- | --- |
| Memory Profiler Config | All settings for the automatic captures. Created from the window. | `MPC_MemoryProfilerConfig` |

## Reading these docs

Every page says near the top who it is for. The emoji in front of a link says the same:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
