---
type: Tool
package: Data Hub
audience: Everyone
tags:
  - data
  - designer
summary: "Tables of every data asset with inline editing, checks, and create, duplicate, rename and delete that keep names and collections right."
menu: Tools > Base Packages > Assets > Data Hub
---
# Data Hub Window

🔧 **Tool** · for everyone

**Tags:** #data #designer

**Menu:** `Tools > Base Packages > Assets > Data Hub`

**For:** Designers editing data, and programmers checking it

Tables of every data asset with inline editing, checks, and create, duplicate, rename and delete that keep names and collections right.

## How to use it

1. Pick a type in the tree on the left. The number next to it is how many assets it has. Click the arrow
   next to a group to fold it.
2. Edit values right in the table. A cell turns into a field when the mouse is on it. Click a header to
   sort, drag its edge to resize. The name and check columns stay put when you scroll sideways.
3. Select several rows with Ctrl or Shift and change one of their cells: every selected row gets the value.
4. Fields the table cannot edit, like effect lists, are in the **Inspector** tab on the right.
5. Use **New**, **Duplicate**, **Rename** and **Delete** above the table, or right click a row.
6. **Actions** runs a button of the selected assets on every one of them.
7. Click the small icon in front of a name to find the asset in the Project window.
8. Right click an asset in the Project window and pick **Show in Data Hub** to jump to its row.
9. Not sure what something is? Hover it. Every column title, button, tab and filter explains itself, and a
   column title shows the field's own tooltip from the code when it has one.

## Shortcuts

| Key | What it does |
| --- | --- |
| Ctrl+D | Duplicate the selected rows |
| Delete | Delete the selected rows |
| F2 or double click a name | Rename |
| Ctrl+A | Select every row |
| Ctrl+F | Jump to the search |
| Arrow keys, Home, End | Move the selection, with Shift to extend it |

## Editing decks and collections

- **Contents tab:** select a deck or a collection. Every list shows each card once with its count. Use **-** and **+**, type a count, or the trash button to remove all copies. Pick a card in the last line, type how many, press **Add**. You can also drag rows from the table onto a list.
- **Used In tab:** select one or more cards. You see every deck and collection that holds them, and below that every one that could. **+** adds one of each selected card, **-** removes one, a typed number sets the count.

## Changing many rows at once

- **Bulk edit:** select rows, right click a number cell and pick **Adjust**. Add, multiply or set, check the
  preview, press Enter. Right click a column title to adjust all shown rows, so search for "Gift" first to
  change every gift.
- **Replace Everywhere:** right click a card, pick the new one. You see every deck and list before anything
  changes. Leave the field empty to take the card out everywhere.
- **Sheet:** **Copy Table** and paste into Google Sheets or Excel. Edit, copy the rows including the title
  row, and use **Paste into Table** to see every change before applying it. **Export CSV** and **Import CSV**
  do the same with a file.

## The check column

- **A check mark** means nothing is wrong.
- **A red number** is the number of problems. Hover it to read them. The row also gets a red edge.
- **An amber number** is a warning: worth a look, nothing broken, like a card that is in no deck yet.
- **Problems** and **Warnings** next to the search show only those rows.
- "Not in ..." means the asset is missing from its collection. Right click the row and pick **Add to Collection**.

## Good to know

- New assets get the right folder, name and collection on their own.
- Delete takes the asset out of every list first, so no empty slot is left behind. The file goes to the trash, not straight away.
- Creating an asset cannot be undone with Ctrl+Z. Delete it instead.