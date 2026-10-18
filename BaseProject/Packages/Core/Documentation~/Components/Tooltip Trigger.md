---
type: Component
package: Core
audience: Everyone
tags:
  - designer
  - ui
summary: Shows a tooltip while the pointer hovers this object.
---
# Tooltip Trigger

🔖 **Component** · for everyone

**Tags:** #designer #ui

Shows a tooltip while the pointer hovers this object and hides it when the pointer leaves. The tooltip shows the text written here first, then the panels of every tooltip provider on the same GameObject. Providers are scripts that build their text at hover time, for live values or translated text.

**Add it:** Inspector › **Add Component** › search `Tooltip Trigger`. Only one per GameObject.

## How to use it

1. Add it to the UI element or object that should explain itself.
2. Write the **Tooltip Text**, or add a component that provides the content.
3. Make sure the UI has one **Tooltip View**.

## Fields

| Field | Type | Default | What it does |
| --- | --- | --- | --- |
| **Tooltip Title** | Text |  | Header above the text. Optional. |
| **Tooltip Text** | Text |  | The text to show. Several lines are fine. Optional when a provider adds the content. |
| **Priority** | Priority |  | When several tooltips overlap, the highest priority wins. |
| **Placement** | Tooltip Placement | Follow Cursor | Next to the cursor, or beside this object so the panels line up next to what they explain. |
| **Anchor** | Transform |  | What Beside Target places the tooltip next to, for example the visible part of a wide element. Empty uses this object. |
| **Show On Hover** | Toggle | On | Shows the tooltip while the pointer hovers this object. Off when a script calls Show and Hide. |

## Good to know

- Needs a Graphic with **Raycast Target** on and an Event System in the scene.
- A trigger without text and without provider content logs a warning and shows nothing.
- Text may use TextMeshPro rich text, including `<sprite name="...">` icons when the panel's text has a sprite asset.
- Code can call **Refresh** to rebuild a visible tooltip after its content changed, for example a countdown.
- If the object is disabled while hovered, the tooltip hides itself.
- Dragging something across the object shows no tooltip, so tooltips never cover a drop target.
- Without a mouse, Follow Cursor falls back to Beside Target. Beside Target measures UI elements by their rect, also on world space canvases, and 3D objects by all their renderers or their collider.
- Objects with hover rules of their own, like cards, turn **Show On Hover** off and call **Show** and **Hide** from code.

## Related

- [🔖 Tooltip View](Tooltip%20View.md)
- [🔖 Tooltip Panel](Tooltip%20Panel.md)
- [🗞️ Tooltip Service](Tooltip%20Service.md)
- [📖 Core utility guide](Utility.md)