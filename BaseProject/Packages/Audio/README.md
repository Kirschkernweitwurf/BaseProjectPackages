# Base Audio

The audio system of the Base packages. Split out of `Base Core` because it reads nothing from it:
this package installs on its own, without Core.

## Requirements

- Unity `6000.3` or newer
- `com.unity.ugui` `2.0.0`
- `Base.ServicesPackage` for `ServiceLocator` and `GameServiceBehaviour`
- `Base.UtilityPackage` for logging and the pooling helpers
- `Base.AttributesPackage` for inspector attributes such as `[Required]`
- `Base.EditorUIPackage.Editor` for the shared look of the unused clip window
- Assemblies: `Base.AudioPackage` and `Base.AudioPackage.Editor`

The audio mixer, its groups, the sample click and hover clips and their containers ship in
`Base.ContentPackage`.

## Systems

- `AudioManager` owns the play, stop and fade API.
- `AudioContainer` is a ScriptableObject holding clips and their playback settings.
- Pooled audio sources per `EAudioType` keep playback allocation-light, and `AudioFader` tweens
  source volume.
- `PlayAudioOnClick`, `PlayAudioOnHover`, `PlayAudioOnSelect` and `PlayAudioOnSubmit` play a
  container from UI events.

## Editor tools

`Find Unused Audio Clips` lists every clip in the project that no container references.