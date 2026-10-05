# Assign GetComponents

🪛 **Tool** · for everyone

**Tags:** #designer #artist #inspector

**Menu:** `Tools > Base Packages > Unity Editor > References > Assign GetComponents`

## What it does

Fills in every empty `[GetComponent]` and `[GetComponentInParent]` field across the project in one go.

Normally these fields fill themselves the first time you open the object in the inspector. This tool does it for everything at once, so you do not have to click through hundreds of prefabs.

## How to use it

1. Open the window from the menu.
2. Choose what should be processed:
   - **Include prefab assets** - every prefab in the project
   - **Include open scenes** - every object in the scenes you currently have open
3. Press **Assign References**.
4. A progress bar runs through the prefabs. You can cancel it at any time.
5. When it is done, the window tells you how many references were filled in.

Changed prefabs are saved automatically.

## Good to know

- Only **empty** fields are touched. Anything you assigned by hand stays as it is.
- `[GetComponent]` looks on the same GameObject.
- `[GetComponentInParent]` looks upward through the parents, skipping the object itself.
- Scenes that are not open are not touched. Open them first if you want them included.

## When to run it

- After pulling changes that added new `[GetComponent]` fields
- Before a build, as a cleanup pass
- Whenever the **Required References** window shows a lot of empty component fields
