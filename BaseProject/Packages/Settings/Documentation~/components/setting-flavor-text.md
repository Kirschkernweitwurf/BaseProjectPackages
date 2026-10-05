# Setting Flavor Text

The panel that shows the name and explanation of whatever setting the player is currently hovering or has selected.

Put **one** somewhere in your settings menu. It listens to every setting element on its own, with no wiring needed.

## Inspector fields

| Field | What to put in it |
| --- | --- |
| **Title Text** | Text object for the setting's name. Optional. |
| **Description Text** | Text object for the longer explanation. Optional. |

Both are optional, so you can show only a description if that is your design.

## Where the text comes from

From the **Title** and **Description** fields on each UI element ([🪛 Setting Toggle](setting-toggle.md), [🪛 Setting Slider](setting-slider.md),[🪛 Setting Dropdown](setting-dropdown.md) and the pickers).

Those are localized, so the panel is translated automatically.

If an element's Title and Description are empty, the panel goes blank when that element is focused.

## Good to know

- It updates on **mouse hover** and on **gamepad or keyboard selection**, so both input methods work.
- Give the panel a fixed height. Descriptions vary in length and a resizing panel makes the whole menu jump around.
- Nothing clears the panel when the player leaves the menu. It keeps showing the last focused setting.

## Setup checklist

- Exactly one in the settings menu.
- Title Text and Description Text assigned.
- Panel has a fixed height.
- Every setting element has its Title and Description filled in.
