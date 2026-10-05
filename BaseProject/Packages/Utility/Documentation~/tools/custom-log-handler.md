# Custom Log Handler

**Menu:** `Tools > Base Packages > Unity Editor > Logging > Enable Custom Log Handler`

**For:** Programmers

A toggle. When it is on, plain `Debug.Log`, `Debug.LogWarning` and `Debug.LogError` calls get the same colored `[ClassName]` prefix that `CustomLogger` adds. That makes it easy to see where a message came from.

## How to use it

Click the menu entry. A check mark shows it is on. Click again to turn it off.

## Good to know

- Off by default, and stored per machine. Turning it on does not affect your teammates.
- It costs a stack trace lookup per message, so it only resolves the class name in the editor and in development builds.
- In a build, the handler is installed at startup automatically.
