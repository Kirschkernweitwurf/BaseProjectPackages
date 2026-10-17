---
type: Tool
package: Data Hub
audience: Everyone
tags:
  - data
  - designer
summary: "Tables of every data asset with inline editing, checks and actions that keep names and lists right."
menu: Tools > Base Packages > Data Hub
---
# Data Hub Window

🔧 **Tool** · for everyone

**Tags:** #data #designer

**Menu:** `Tools > Base Packages > Data Hub`

**For:** Designers editing data and programmers checking it

Tables of every data asset with inline editing, checks and actions that keep names and lists right.

## The window at a glance

| Part | Where | What it is for |
| --- | --- | --- |
| Tree | Left | Every kind of data, like cards or decks. The number is how many assets it has. |
| Table | Middle | One row per asset, one column per value. Edit right in the cells. |
| Toolbar | Above the table | New, Duplicate, Rename, Delete, Actions, Sheet and the search. |
| Side panel | Right | Inspector, Contents and Used In tabs for the selected rows. |
| Status line | Under the title | How many rows are shown and selected, and how many have problems. |

Not sure what something does? Hover it. Tooltips are kept to one short line.

## How to use it

1. Pick a kind of data in the tree. Click the arrow next to a group to fold it.
2. Click a cell to edit it. Numbers, text, yes or no and dropdowns all work right in the table.
3. To change many rows at once, select them with Ctrl or Shift and edit one of their cells. Every selected row
   gets the new value.
4. Things the table can't edit, like effect lists, are in the **Inspector** tab on the right.
5. Use **New**, **Duplicate**, **Rename** and **Delete** above the table, or right click a row.
6. Click the small icon in front of a name to find the asset in the Project window.
7. Right click an asset in the Project window and pick **Show in Data Hub** to jump to its row.

## Finding rows

- **Search** looks at names and every value. Upper and lower case don't matter.
- **All**, **Problems** and **Warnings** next to the search filter the rows. The numbers show how many there are.
- Click a column title to sort. Click again to reverse, a third time to go back to sorting by name.
- Drag the edge of a column title to make it wider or narrower.
- The name and check columns stay in place when you scroll sideways.
- Hover a column title to see what it holds. Some titles sit under a shared label, like the four mood scores.

## Shortcuts

| Key | What it does |
| --- | --- |
| Ctrl+D | Duplicate the selected rows |
| Delete | Delete the selected rows |
| F2 or double click a name | Rename |
| Ctrl+A | Select every row |
| Ctrl+F | Jump to the search |
| Ctrl+C | Copy the selected rows for a spreadsheet |
| Ctrl+V | Paste rows from a spreadsheet, with a preview first |
| Arrow keys, Home, End | Move the selection. Hold Shift to extend it. |

On a Mac use Cmd instead of Ctrl.

## Creating, renaming and deleting

- **New** makes an asset in the right folder, gives it the right name and adds it to its list. It is
  selected right away. When a table holds several kinds, pick the kind from the menu.
- **Duplicate** copies each selected asset next to the original with the next free number.
- **Rename** shows the final file name before you apply it. The naming rules may add a prefix or change the
  casing.
- **Delete** asks first and lists every deck or list the assets are taken out of. The files go to the trash.
  Scenes and prefabs are not changed.
- Creating an asset can't be undone with Ctrl+Z. Delete it instead. Every edit can be undone.

## The side panel

- **Inspector:** the normal Unity inspector of the selected rows, for things the table can't edit and for
  the buttons of the asset. With several rows selected, a change goes to all of them.
- **Contents:** select one deck or list. Every list inside it shows each asset once with its count.
  - Use **-** and **+** or type a count. The trash button removes every copy.
  - Pick an asset in the last line, type how many and press **Add**.
  - You can also drag rows from the table onto a list.
  - Some kinds show a live summary at the top, like the balance of a deck.
- **Used In:** select one or more rows. You see every deck and list that holds them, and below that every one
  that could.
  - **+** adds one of each selected asset, **-** removes one, a typed number sets the count.
  - A dash means the selected rows are in that list a different number of times.

## The check column

Every row is checked all the time. You don't need to run anything.

| You see | Meaning |
| --- | --- |
| A faint check mark | All good. |
| A red number | That many problems. The row also gets a red edge. |
| An amber number | Warnings. Nothing is broken, but it is worth a look. |

Hover the number to read what is wrong. The common ones:

- **A required field is empty**, for example a card without a name.
- **Not in ...** means the asset is missing from the list it belongs in. Right click the row and pick
  **Add to Collection**.
- **Not used anywhere yet** is a warning: the asset is only in its list, no deck or scene uses it.
- Projects add their own checks, for example a deck that holds a card too often.

## Changing many rows at once

- **Adjust:** select rows, right click a number cell and pick **Adjust**. Add, multiply or set, check the
  preview and press Enter. Right click a column title to adjust every shown row, so search for "Gift" first to
  change every gift. Whole number fields are rounded, and values stay within the field's limits.
- **Replace Everywhere:** right click an asset and pick the one that takes its place. You see every deck and
  list before anything changes. Leave the field empty to take the asset out everywhere instead.
- **Actions** runs a button of the selected assets on each of them at once. Only buttons all of them share are
  listed.

## Working in a spreadsheet

1. Open **Sheet** and pick **Copy Rows**, or press Ctrl+C. It copies the selected rows, or every shown row
   when none is selected. Paste them into Google Sheets or Excel.
2. Edit the values there. Leave the title row and the **Name** column as they are, rows are matched by name.
3. Copy the rows you changed together with the title row.
4. Back in the hub, pick **Paste into Table** or press Ctrl+V.
5. The preview lists every change, plus anything it could not use and why. Press **Apply** when it looks right.

**Export CSV** and **Import CSV** do the same with a file. Columns the table can't edit are left alone.

## Good to know

- The hub updates on its own when assets change. The refresh button is rarely needed.
- Drag a row by its name to drop the selected assets on a list, an object field or the Project window.
- Lists and some special fields show as read only text in the table. Edit asset lists in the Contents tab and
  everything else in the Inspector tab.
- Delete only cleans up data assets. A scene or prefab that points at a deleted asset keeps an empty field.
