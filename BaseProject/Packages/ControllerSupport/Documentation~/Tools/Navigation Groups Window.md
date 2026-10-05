# Navigation Groups Window

🪛 **Tool** · for everyone

**Menu:** `Tools > Base Packages > Unity Editor > Controller Navigation Groups`

## What it does

Lists every [🔖 Navigable Group](<../Components/Navigable Group.md>) in the loaded scenes in one table. From here you can jump to a group, rebuild its navigation, and see at a glance which groups are set up wrong.

Nothing is ever changed automatically. Every rebuild and every fix needs a click, so your navigation wiring never changes behind your back.

## The table

| Column | Meaning |
| --- | --- |
| Group | Name of the GameObject the group sits on |
| Menu | Menu component on the same object, or `None` if the group manages itself |
| Scene | Scene the group lives in |
| Priority | Focus priority used by the [🗞️ Focus Watchdog](<../Components/Focus Watchdog.md>) |
| Elements | How many [🔖 Navigable Element](<../Components/Navigable Element.md>)s are below the group |

Rows are striped and highlight on hover. Every badge has a tooltip explaining it.

## Warnings

A row is tinted orange when a group sits on a menu but breaks one of the menu rules:

- **Auto Activate is on.** The menu already activates the group, so the group must not do it itself.
- **Priority does not match the menu.** The group should use the same priority as its menu.

An orange **Elements** badge means the group has no navigable elements at all.

## Row buttons

- **Fix**: Corrects the warnings above (turns Auto Activate off, copies the menu's priority). Only enabled when the row actually has an issue.
- **Go to**: Selects and pings the group in the hierarchy.
- **Rebuild**: Rewires navigation for that one group.

## Toolbar buttons

- **Refresh**: Rescans the loaded scenes. The list also refreshes on hierarchy changes.
- **Rebuild Scene**: Rebuilds every group in the currently loaded scenes, inactive ones included.
- **Rebuild Project**: Asks for confirmation, then opens every scene in the project, rebuilds all groups and saves the scenes. Prefabs that contain groups are rebuilt and saved too. Your original scene setup is restored afterwards.

Rebuilding also adds a missing [🔖 Navigable Element](<../Components/Navigable Element.md>) to any selectable that lacks one, and logs each fix.

## When to use it

- After building or reordering a menu layout.
- Before a build, to make sure no group is empty or misconfigured.
- When gamepad focus behaves oddly and you want to see which groups compete for it.

> [!NOTE]
> Since it builds on Unity’s own navigation, the links between the buttons are visualized by default and the links can be seen on the Buttons itself
