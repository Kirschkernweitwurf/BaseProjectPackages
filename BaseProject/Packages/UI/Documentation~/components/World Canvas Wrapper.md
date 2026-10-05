# World Canvas Wrapper

🗞️ **Component** · programmers only

Hooks a world-space canvas up to the main camera. Without it, clicks and hovers on that canvas often do not register.

## Where to put it

On the GameObject that has the **Canvas** component, with Render Mode set to **World Space**.

## Fields

| Field | Required | What it does |
| --- | --- | --- |
| Canvas | Auto | Filled in automatically. |

## Good to know

- Only needed for **World Space** canvases. Screen-space canvases work without it.
- The scene needs a camera tagged **MainCamera**.
- Add this whenever a world-space button "does not react" to the mouse. It is the usual cause.
