---
type: component
package: Core Debug
audience: programmer
tags:
  - programmer
  - debugging
summary: "Runs cheat commands: Enter runs, Tab completes, arrow keys walk the history."
requires:
  - Cheat Console View
---
# Cheat Console Controller

🗞️ **Component** · programmers only

**Tags:** #programmer #debugging

Runs the cheat console. **Enter** runs the typed command, **Tab** completes it, **Up** and **Down** walk through earlier commands.

**Add it:** Inspector › **Add Component** › search `Cheat Console Controller`. Needs **Cheat Console View** on the same GameObject.

## How to use it

1. Put it on the same GameObject as the **Cheat Console View**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **View** | [Cheat Console View](Cheat%20Console%20View.md) |  | Filled in automatically. The cheat console view on the same GameObject. |

## Related

- [🗞️ Cheat Console View](Cheat%20Console%20View.md)
- [🗞️ Debug Menu Controller](Debug%20Menu%20Controller.md)