# Audio Container

💌 **Scriptable Object** · for everyone

**Tags:** #sound #designer #audio

**Menu path:** `Create > Scriptable Objects > Base > Audio > New Audio Container`

**Default file name:** `AUC_AudioContainer`

## What it does

A single "sound" as far as the game is concerned. Instead of referencing a raw audio clip everywhere, you reference an Audio Container. It holds one or more clips plus how they should be played.

If you put several clips in one container, a random one is picked each time it plays. This is how you avoid a footstep or button click sounding identical on every press.

## Fields

| Field | What it does |
| --- | --- |
| **Audio Type** | Which mixer group and pool the sound uses: `Sfx2D`, `Sfx3D`, `Music`, or `UI` |
| **Clips** | The clips to choose from. One is picked at random per play. Must not be empty. |
| **Delay** | Seconds to wait before the sound starts. `0` plays immediately. |
| **Ignore Pause** | Keep playing while the game is paused. Turn this on for UI sounds. |
| **Loop** | Repeat the clip until it is stopped |
| **Randomize Pitch** | Vary the pitch slightly on each play, so repeats sound less mechanical |
| **Volume** | Playback volume, from `0` to `1` |
| **Max Clips Playing** | How many copies of this container may play at once. `-1` means unlimited. |

## How to use it

1. Create the asset and drop your clips into **Clips**.
2. Pick the right **Audio Type**. This decides the mixer group, so getting it wrong makes the sound ignore the player's volume sliders.
3. Reference the container from a component such as Play Audio On Click, or hand it to a programmer to trigger from code.

## Tips

- **Max Clips Playing** is your friend for sounds that can spam, like card plays or hit sounds. A value of `3` or `4` keeps things from turning into noise.
- Turn on **Ignore Pause** for anything the player clicks in a pause menu, otherwise it will be silent.
- Containers stored under `Assets/ScriptableObjects/AudioContainer` are checked by the Unused Audio Clips tool. Keeping them there prevents clips from being reported as unused by mistake.
