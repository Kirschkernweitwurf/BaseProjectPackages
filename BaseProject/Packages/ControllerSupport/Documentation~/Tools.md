# Tools

🧰 **Tools** · menu items and editor windows

Editor tooling shipped with the Controller Support package. Everything here is opt-in: a rebuild only runs when you click it, so navigation wiring never changes on its own.

| Tool | Menu path | What it is for |
| --- | --- | --- |
| [🪛 NavigationGroupsWindow](Tools/Navigation%20Groups%20Window.md) | `Tools > Base Packages > Unity Editor > Controller Navigation Groups` | Overview of every navigable group in the loaded scenes, with per group rebuild, jump-to and one-click fixes |

There are no `CreateAssetMenu` entries outside the input prompt system.

## Where else rebuilds live

- **NavigableGroup inspector**: Rebuild and Rebuild Scene buttons.
- **Component context menu**: Rebuild Navigation.
- **Code**: `group.Rebuild()`.

> [!NOTE]
> Menu paths listed on the child pages are defaults. They can be moved with the Menu Item Manager in the Tools package.
