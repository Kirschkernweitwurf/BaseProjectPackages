# Audio Rules

🪛 **Tool** · for everyone

**Tags:** #sound #audio #assets

**Menu:** `Tools > Base Packages > Assets > Audio Rules`

**Create:** `Create > Scriptable Objects > Base > Audio Rules > New Rule Set` (default file name `ARS_AudioRuleSet`)

**For:** Everyone who imports audio

Sets the import settings of all audio files from rules, instead of clicking through every clip by hand.

## How to use it

1. Open the window. If there is no rule set yet, press **Create Rule Set**.
2. Add rules on the left. Each rule says which clips it applies to and which settings it wants.
3. Scan. The table on the right shows, per clip, what would change. Nothing is changed yet.
4. Select a row to see the details underneath, including which rule decided each setting.
5. Press **Apply** to write the changes to the import settings.

## What rules can set

Load type, compression format, quality, sample rate, force to mono, load in background and preload audio data. Rules can differ per platform.

## Findings

Besides settings, the scan points out problems in the audio itself, such as silence at the start of a clip, clipping, fake stereo or a DC offset. The thresholds for these are set in the rule set.

## Good to know

- A scan never changes anything. Only **Apply** does.
- Reading the audio data is the slow part, so the scan runs in the background and fills the table as it goes.
- The panes can be resized by dragging their edges.
