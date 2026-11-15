---
type: Tool
package: Tools
audience: Everyone
tags:
  - sound
  - audio
  - assets
summary: Sets the import settings of all audio files from rules, instead of clicking through every clip by hand.
menu: Tools > Base Packages > Assets > Audio Rules
---
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
5. Press **Apply All** to write the changes to the import settings of every clip, on every target, in one click.

To apply only some clips, select their rows first. The button turns into **Apply Selected**, asks before it
writes, and only covers the target shown in the dropdown.

## What rules can set

Load type, compression format, quality, sample rate, force to mono, load in background and preload audio data. Rules can differ per platform.

## Default rules

A new rule set starts with these rules. They follow Unity's audio import docs and published import
recommendations, and every rule can be changed afterwards.

| Rule | Settings | Why |
| --- | --- | --- |
| Everything | Keep the sample rate, load on the main thread, preload | A lower sample rate gets audible as soon as a sound is pitched down. |
| Under 1 s | PCM, Decompress On Load | Tiny, and playing PCM costs almost no CPU. |
| 1 s to 10 s | Vorbis 70%, Decompress On Load, load in background | Small on disk, decoded once, cheap to play. |
| 10 s to 30 s | Vorbis 70%, Compressed In Memory, load in background | Decoding up front would take about ten times the memory. |
| Over 30 s | Vorbis 70%, Streaming, no preload | Costs a buffer of about 200 KB instead of the whole clip. Keep to two streams at once. |
| Music | Vorbis 85% | Music is listened to, so it keeps more detail. Uses the category of its Audio Container. |
| Spatial sound effects | Force to mono | A stereo clip cannot be placed in the world. |
| Voice over (path contains `/VO/`) | Mono, Vorbis 70%, Compressed In Memory | Voice is recorded mono, and a game has many lines. |
| Compressed clips on Android / iOS | Vorbis 50% | Phones have less room, and their speakers hide the difference. |
| Music on Android / iOS | Vorbis 70% | Music keeps a little more on phones too. |

ADPCM is not used by default: it adds audible artifacts to clean, tonal sounds like UI chimes. It suits noisy
sounds played often, like footsteps, so add a rule for those if the project needs it.

The Music and Spatial rules read the category from the Audio Container that uses a clip. That is only known in the
window, so a new clip gets these two rules when **Apply All** runs, not while it is imported.

## Findings

Besides settings, the scan points out problems in the audio itself, such as silence at the start of a clip, clipping, fake stereo or a DC offset. The thresholds for these are set in the rule set.

## Good to know

- A scan never changes anything. Only **Apply All** or **Apply Selected** does.
- **Apply All** ignores the target dropdown, the filters and the search. Its number counts the clips that differ on
  at least one target.
- A platform only gets its own import override where a rule for that platform wants something else than the
  default settings. Every other platform keeps using the defaults.
- Reading the audio data is the slow part, so the scan runs in the background and fills the table as it goes.
- The panes can be resized by dragging their edges.