---
type: Tool
package: Attributes
audience: Everyone
tags:
  - designer
  - artist
  - sound
  - inspector
summary: A toggle that stops the Attributes package from drawing inspectors, without uninstalling it.
menu: Tools > Base Packages > Editor > Attribute Inspector
---
# Disable Attribute Inspector

🪛 **Tool** · for everyone

**Tags:** #designer #artist #sound #inspector

**Menu:** `Tools > Base Packages > Editor > Attribute Inspector`

**For:** Everyone, as an emergency switch

A toggle that stops the Attributes package from drawing inspectors, without uninstalling it. Unity's own inspector draws instead.

## When to use it

The Attributes package draws the inspector of every component and scriptable object in the project, third-party ones included. If something goes wrong in it, every inspector is affected at once. This switch gets you working again until it is fixed.

## What turns off

Titles, foldouts, tabs, horizontal rows, show/hide and enable/disable conditions, validation messages, auto-assignment and buttons. Your field values are not touched.

## Good to know

The menu entry is checked while the attribute inspector is on. Click it to turn the inspector off, and again to turn it back on.