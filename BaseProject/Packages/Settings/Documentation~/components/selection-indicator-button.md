# Selection Indicator Button

One dot in the row underneath an arrow picker. It shows whether its option is the selected one, and clicking it jumps straight to that option.

You never place these in a scene. You build **one prefab**, and the arrow pickers spawn as many copies as they need.

Needs a Unity **Button** on the same GameObject.

## Inspector fields

| Field | What to put in it |
| --- | --- |
| **Tween Group** | The TweenGroup that shows and hides the selected look. Required. |
| **Button** | Filled in automatically from the same GameObject. |

## Building the prefab

1. Create a UI GameObject, for example an Image, and name it something like
  `SelectionIndicator`.
2. Add a **Button** component.
3. Add a **TweenGroup** and set up how the selected state fades or scales in.
4. Add **SelectionIndicatorButton** and drag the TweenGroup into it.
5. Save it as a prefab.
6. Drag that prefab into the **Selection Indicator Prefab** field on every arrow picker.

## Good to know

- The TweenGroup is shown when the dot is selected and hidden when it is not. Whatever you animate in that group is what "selected" looks like.
- Dots are pooled and reused. Do not put per-option data on the prefab.
- Make the clickable area comfortably large even if the visible dot is small.

## Setup checklist

- Prefab has Button, TweenGroup and SelectionIndicatorButton.
- TweenGroup assigned in the component.
- Prefab has a sensible click area.
