---
type: Overview
package: Core
tags:
  - menus
summary: Components you add to GameObjects in a scene or prefab.
---
# Core Components

🗳️ **Components** · things you add to a GameObject

**Tags:** #menus

Components you add to GameObjects in a scene or prefab. Grouped by area.

| Page | What is in it |
| --- | --- |
| [🔖 Menus](Components/Menus.md) | The Menu component, its five modules, Pause Menu and Loading Screen |
| [🔖 Utility](Components/Utility.md) | Activate After Time and Frames, and the tooltip components |

Looking for tweens, sounds or the Bootstrapper? See [Tweening](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Tweening/Documentation~/Components/Tweening.md), [Audio](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Audio/Documentation~/Components/Audio.md) and [Bootstrapper](https://github.com/Kirschkernweitwurf/BaseProjectPackages/blob/main/BaseProject/Packages/Services/Documentation~/Components/Bootstrapper.md).

Manager components such as Menu Manager live on prefabs that are already set up. You should not need to add those yourself.

<!-- all-components:start -->
## All components

Every component has its own page with all fields. Generated from the code; do not edit by hand.

### Menus

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Menu](Components/Menu.md) | Turns a UI root into a screen that opens and closes with an animation. | Everyone |
| [🔖 Pause Menu](Components/Pause%20Menu.md) | A Menu that also tracks whether the game is paused. | Everyone |
| [🔖 Loading Screen](Components/Loading%20Screen.md) | A Menu that shows itself while scenes load, with progress bar and spinner. | Everyone |

### Menu modules

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Menu Cursor Module](Components/Menu%20Cursor%20Module.md) | Shows, hides or locks the mouse cursor while the menu is open. | Everyone |
| [🔖 Menu Time Scale Module](Components/Menu%20Time%20Scale%20Module.md) | Changes the game speed while the menu is open. | Everyone |
| [🔖 Menu Input Map Module](Components/Menu%20Input%20Map%20Module.md) | Switches the active controls while the menu is open. | Everyone |
| [🔖 Menu Reset Module](Components/Menu%20Reset%20Module.md) | Resets the menu's contents when it closes, so it opens fresh. | Everyone |
| [🔖 Menu Group Module](Components/Menu%20Group%20Module.md) | Shows several menus inside this menu as tabs, one at a time. | Everyone |

### Helpers

| Component | What it does | For |
| --- | --- | --- |
| [🔖 Activate After Time](Components/Activate%20After%20Time.md) | Turns a GameObject on after a delay in seconds. | Everyone |
| [🔖 Activate After Frames](Components/Activate%20After%20Frames.md) | Turns a GameObject on after a number of frames. | Everyone |
| [🔖 Tooltip Panel](Components/Tooltip%20Panel.md) | Draws one tooltip panel: icon, title and text. | Everyone |
| [🔖 Tooltip Trigger](Components/Tooltip%20Trigger.md) | Shows a tooltip while the pointer hovers this object. | Everyone |
| [🔖 Tooltip View](Components/Tooltip%20View.md) | Draws the tooltip panels next to the cursor and keeps them on screen. | Everyone |

### Services and managers

| Component | What it does | For |
| --- | --- | --- |
| [🗞️ Camera Provider](Components/Camera%20Provider.md) | Gives code fast access to the main and the current camera. | Programmers |
| [🗞️ Cursor Manager](Components/Cursor%20Manager.md) | Decides how the mouse cursor looks, by priority. | Programmers |
| [🗞️ Event Bus](Components/Event%20Bus.md) | Lets systems send each other events without knowing each other. | Programmers |
| [🗞️ Input Manager](Components/Input%20Manager.md) | Decides which input action map is active, by priority. | Programmers |
| [🗞️ Menu Manager](Components/Menu%20Manager.md) | Keeps track of every menu and routes the Back button. | Programmers |
| [🗞️ Scene Loading Manager](Components/Scene%20Loading%20Manager.md) | Loads and unloads scenes and keeps the persistent scene loaded. | Programmers |
| [🗞️ Screenshot Manager](Components/Screenshot%20Manager.md) | Saves screenshots on F6 (normal size) and F7 (four times larger). | Programmers |
| [🗞️ Time Scale Manager](Components/Time%20Scale%20Manager.md) | Decides the game speed, by priority. | Programmers |
| [🗞️ Tooltip Service](Components/Tooltip%20Service.md) | Shows the most important requested tooltip. | Programmers |
<!-- all-components:end -->