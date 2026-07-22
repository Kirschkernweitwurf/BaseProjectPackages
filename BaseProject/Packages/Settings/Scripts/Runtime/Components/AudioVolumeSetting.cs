using Base.AttributePackage;
using Base.ToolPackage.Identification;
using Base.UtilityPackage.Types;
using UnityEngine;
using UnityEngine.Audio;

namespace Base.SettingsPackage.Components
{
    /// <summary>
    /// Stores a normalized 0..1 volume and pushes it as decibels into an <see cref="AudioMixer"/> parameter.
    /// One instance per channel: master, music, sfx, and so on. The setting key matches the mixer parameter
    /// name so the two stay in sync from the inspector.
    /// </summary>
    public sealed class AudioVolumeSetting : FloatSettingComponent
    {
        private const float SilenceDecibels = -80f;
        private const float SilenceThreshold = 0.0001f;

        [Header("Audio Volume")]

        [SerializeField] [Required] private AudioMixer audioMixer;

        [Tooltip("Name of the exposed AudioMixer parameter. Also used as the setting's registry key.")]
        [SerializeField] [NotNullOrEmpty] [MixerParameter(nameof(audioMixer))]
        private string mixerParameter = "MasterVolume";

        [SerializeField] [Range(0f, 1f)] private float defaultVolume = 0.7f;

        /// <inheritdoc/>
        public override PersistentKey Key => new(mixerParameter);

        /// <inheritdoc/>
        protected override float DefaultValue => defaultVolume;

        /// <inheritdoc/>
        protected override void Apply(float linear)
        {
            float decibel = linear <= SilenceThreshold
                ? SilenceDecibels
                : AudioMathUtility.ConvertLinearToDecibel(linear);

            audioMixer.SetFloat(mixerParameter, decibel);
        }
    }
}