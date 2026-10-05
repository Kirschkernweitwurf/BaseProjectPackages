# Localization Package

📦 **Package** · reusable, works in any of our projects

**Tags:** #localization

Keeps Unity's String Tables and a Google Sheet in sync, so text can be edited in Unity or in the sheet. Both end up in the same place. Pick whichever is easier for you.

> [!NOTE]
> This page explains the package in general. Your project's own setup (which sheet, which account, the login details) belongs in the project's docs, not here.

## Components

| Page | What it does | For whom |
| --- | --- | --- |
| [🔖 Language Setting](Components/Language%20Setting.md) | Saves the game language and switches it | Everyone |

## Tools

| Menu entry | What it does |
| --- | --- |
| `Tools > Base Packages > Assets > Localization > Pull All String Tables` | Brings the latest text from Google Sheets into Unity, for every table |
| `Tools > Base Packages > Assets > Localization > Push All String Tables` | Sends your text from Unity to Google Sheets, for every table. Asks first. |
| `Tools > Base Packages > Assets > Localization > Open Sync Window` | A small window to pull or push one table at a time |

In the Sync Window you can:

- Press **Refresh** to reload the list of tables.
- Use **Pull All** or **Push All** for everything at once.
- Use the **Pull** or **Push** button next to a single table to sync only that one.

You can also open a table and use Unity's own Pull and Push buttons in the table view. Same result.

## One-time setup per machine

Unity talks to Google through a **Sheets Service Provider** asset in your project. To pull and push, that asset needs login credentials and you need to authorize once:

1. Open the Sheets Service Provider asset.
2. Fill in the Client ID and Client Secret. Your project's docs say where to get them.
3. Click **Authorize**. Google opens a browser window. Log in and allow access.

If text loads but pushing fails, the credentials are missing or the authorization ran out.

## Editing text

- **In Unity:** open the String Table Collection, edit the text in the table view, save.
- **In Google Sheets:** edit the text in the sheet. You can add new keys there too. Sheets saves on its own.

> [!IMPORTANT]
> Both sides hold the same content. Pull before you start working in Unity, push when you are done. In Google Sheets it is the other way around: push from Unity first, pull when you are done. Then commit.

**Pull** overwrites your local tables. **Push** overwrites the sheet. Pick the direction with care.

## Auto-translate in Google Sheets

Google Sheets can do a rough first pass so you do not translate everything by hand.

1. Fill in the English column first.
2. In the next language column, use this formula, pointing at the English cell:

   ```text
   =GOOGLETRANSLATE(B2, "en", "de")
   ```

   `"en"` is the source language, `"de"` the target. Use the code shown in the column header.

3. Drag the small handle in the bottom right corner of the cell down to fill the column.
4. Repeat for each language column.

> [!WARNING]
> Auto-translation is a starting point, not a final text. Have it checked before shipping.

## Quick recap

1. Pull or push to get the latest.
2. Edit in Unity or Google Sheets.
3. Use `=GOOGLETRANSLATE` to fill gaps fast.
4. Push or pull so the whole team has your changes.
5. Commit.

## Reading these docs

Every page shows its icon and who it is for right under the title. Links to it carry the same icon:

| Area | Everyone | Programmers only |
| --- | --- | --- |
| Components 🗳️ | 🔖 | 🗞️ |
| Scriptable Objects 📬 | 💌 | ✉️ |
| Tools 🧰 | 🪛 | 🔧 |

The **Tags** line under the badge says who needs a page (`#artist`, `#designer`, `#sound`, `#programmer`) and what it is about (`#ui`, `#animation`, `#audio`, ...). In Obsidian, click a tag to list every page with it. In any other app, search the folder for the tag, for example `#artist`. [All tags](Tags.md) lists every page of this package by tag.

Menu paths are defaults. They can be moved with the Menu Item Manager in the Tools package, so your project may differ.

Something wrong or missing? [Open an issue](https://github.com/Kirschkernweitwurf/BaseProjectPackages/issues). Half a sentence is enough.
