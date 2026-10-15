---
type: Tool
package: Utility
audience: Everyone
tags:
  - artist
  - designer
  - sound
  - workflow
summary: "Copies the docs of every installed Base package into the project's Docs/Packages folder."
menu: Help > Base Packages Documentation
---
# Documentation Sync

🪛 **Tool** · for everyone

**Tags:** #artist #designer #sound #workflow

**Menu:** `Help > Base Packages Documentation`, **Sync** button. Usually you never need it: it runs on its own.

Copies the docs of every installed Base package into the project's `Docs/Packages` folder. Open the `Docs` folder as one vault (for example in Obsidian) and you see the project docs and the docs of every package in one place.

## How to use it

1. Open the project's `Docs` folder in your Markdown app. In Obsidian: **Open folder as vault** and pick `Docs`.
2. Start Unity once. The package docs appear in `Docs/Packages`.
3. Start at `Docs/Packages/Base Packages.md`, or use the **Packages** link on the project's start page.

## When it runs

- When Unity starts.
- When a package is added, removed or updated.
- When you click **Sync** in `Help > Base Packages Documentation`.

Only changed files are written, so a sync is quick.

## Good to know

- It only runs in projects whose `Docs` folder has a start page. Projects without their own docs are never touched.
- A start page is the page in the root of a docs folder with `type: Start Page` in its properties, for example
  `Tweening Package.md` or `Hand Over Heels.md`. Older packages and projects with an `index.md` work too.
- `Docs/Packages` is generated. Do not edit files there: the next sync overwrites them. Fix the docs in the package instead.
- A package you remove disappears from `Docs/Packages` on the next sync.
- Links to the docs of another installed package open the local copy. Links to packages the project does not use still go to GitHub.
- `Docs/Packages` is added to the project's Plastic `ignore.conf` once, so the copies are never checked in. Each machine makes its own.