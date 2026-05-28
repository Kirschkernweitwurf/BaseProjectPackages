using UnityEngine;
using UnityEngine.Audio;

namespace Base.SettingsPackage.Base_Settings_Package.Scripts.Runtime
{
    /// <summary>
    /// Applies the volume settings to an audio mixer.
    /// </summary>
    public sealed class AudioSettingsApplier : SettingsApplier
    {
        private const float MinLinearVolume = 0.0001f;
        private const float DecibelMultiplier = 20f;

        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string mainVolumeParameter = "MainVolume";
        [SerializeField] private string musicVolumeParameter = "MusicVolume";
        [SerializeField] private string sfxVolumeParameter = "SfxVolume";

        protected override void OnInitialized(ISettingsService settings)
        {
            settings.MasterVolume.Subscribe(HandleMainVolumeChanged);
            settings.MusicVolume.Subscribe(HandleMusicVolumeChanged);
            settings.SfxVolume.Subscribe(HandleSfxVolumeChanged);
        }

        protected override void OnTeardown()
        {
            Settings.MasterVolume.Unsubscribe(HandleMainVolumeChanged);
            Settings.MusicVolume.Unsubscribe(HandleMusicVolumeChanged);
            Settings.SfxVolume.Unsubscribe(HandleSfxVolumeChanged);
        }

        private void ApplyMixerVolume(string parameterName, float linearVolume)
        {
            if (audioMixer == null)
                return;

            if (string.IsNullOrEmpty(parameterName))
                return;

            float clampedVolume = Mathf.Max(linearVolume, MinLinearVolume);
            float decibels = Mathf.Log10(clampedVolume) * DecibelMultiplier;
            audioMixer.SetFloat(parameterName, decibels);
        }

        private void HandleMainVolumeChanged(float value) => ApplyMixerVolume(mainVolumeParameter, value);

        private void HandleMusicVolumeChanged(float value) => ApplyMixerVolume(musicVolumeParameter, value);

        private void HandleSfxVolumeChanged(float value) => ApplyMixerVolume(sfxVolumeParameter, value);
    }
}