using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// The "then" half of a rule: the settings it writes and nothing else. A setting that is not
    /// switched on here is never written and never compared, which is what lets a base rule pin
    /// the codec while a later rule only flips the mono flag.
    /// </summary>
    [Serializable]
    public sealed class AudioSettingOverrides
    {
        private const float DefaultQuality = 0.7f;
        private const int DefaultSampleRate = 44100;
        private const int MaxSampleRate = 192000;
        private const int MinSampleRate = 8000;

        [field: Tooltip("Writes how the clip lives in memory at runtime.")]
        [field: SerializeField] public bool SetsLoadType { get; set; }

        [field: SerializeField] public AudioClipLoadType LoadType { get; set; } = AudioClipLoadType.DecompressOnLoad;

        [field: Tooltip("Writes the codec the clip is stored with.")]
        [field: SerializeField] public bool SetsCompressionFormat { get; set; }

        [field: SerializeField]
        public AudioCompressionFormat CompressionFormat { get; set; } = AudioCompressionFormat.Vorbis;

        [field: Tooltip("Writes the encoder quality. Only reaches the lossy formats.")]
        [field: SerializeField] public bool SetsQuality { get; set; }

        [field: Range(0f, 1f)]
        [field: SerializeField] public float Quality { get; set; } = DefaultQuality;

        [field: Tooltip("Writes how the sample rate is handled, including the forced rate.")]
        [field: SerializeField] public bool SetsSampleRate { get; set; }

        [field: SerializeField]
        public AudioSampleRateSetting SampleRateSetting { get; set; } = AudioSampleRateSetting.PreserveSampleRate;

        [field: Range(MinSampleRate, MaxSampleRate)]
        [field: SerializeField] public int SampleRateOverride { get; set; } = DefaultSampleRate;

        [field: Tooltip("Writes the downmix to one channel. Shared across platforms.")]
        [field: SerializeField] public bool SetsForceToMono { get; set; }

        [field: SerializeField] public bool ForceToMono { get; set; }

        [field: Tooltip("Writes the worker thread load flag. Shared across platforms.")]
        [field: SerializeField] public bool SetsLoadInBackground { get; set; }

        [field: SerializeField] public bool LoadInBackground { get; set; }

        [field: Tooltip("Writes whether the audio data is loaded together with its scene.")]
        [field: SerializeField] public bool SetsPreloadAudioData { get; set; }

        [field: SerializeField] public bool PreloadAudioData { get; set; }

        /// <summary>True when this rule writes nothing at all.</summary>
        public bool IsEmpty => !SetsCompressionFormat
            && !SetsForceToMono
            && !SetsLoadInBackground
            && !SetsLoadType
            && !SetsPreloadAudioData
            && !SetsQuality
            && !SetsSampleRate;

        /// <summary>Creates empty overrides. Needed by the serializer.</summary>
        public AudioSettingOverrides() { }

        /// <summary>True when this rule writes the given setting.</summary>
        /// <param name="setting">The setting to ask about.</param>
        /// <returns>True when the setting is written.</returns>
        public bool Sets(EAudioSetting setting) => setting switch
        {
            EAudioSetting.CompressionFormat => SetsCompressionFormat,
            EAudioSetting.ForceToMono => SetsForceToMono,
            EAudioSetting.LoadInBackground => SetsLoadInBackground,
            EAudioSetting.LoadType => SetsLoadType,
            EAudioSetting.PreloadAudioData => SetsPreloadAudioData,
            EAudioSetting.Quality => SetsQuality,
            EAudioSetting.SampleRate => SetsSampleRate,
            _ => false
        };

        /// <summary>Copies every value onto another set, used when a rule is duplicated.</summary>
        /// <param name="target">The overrides to fill.</param>
        public void CopyTo(AudioSettingOverrides target)
        {
            target.SetsLoadType = SetsLoadType;
            target.LoadType = LoadType;
            target.SetsCompressionFormat = SetsCompressionFormat;
            target.CompressionFormat = CompressionFormat;
            target.SetsQuality = SetsQuality;
            target.Quality = Quality;
            target.SetsSampleRate = SetsSampleRate;
            target.SampleRateSetting = SampleRateSetting;
            target.SampleRateOverride = SampleRateOverride;
            target.SetsForceToMono = SetsForceToMono;
            target.ForceToMono = ForceToMono;
            target.SetsLoadInBackground = SetsLoadInBackground;
            target.LoadInBackground = LoadInBackground;
            target.SetsPreloadAudioData = SetsPreloadAudioData;
            target.PreloadAudioData = PreloadAudioData;
        }

        /// <summary>
        /// Writes everything this rule sets onto the running target and records who decided it, so
        /// a later rule in the cascade can be told apart from an earlier one.
        /// </summary>
        /// <param name="target">The target settings built up so far.</param>
        /// <param name="decisions">The trace of which rule decided which setting.</param>
        /// <param name="ruleLabel">The label recorded in the trace.</param>
        public void ApplyTo(AudioSettingValues target, IDictionary<EAudioSetting, string> decisions,
            string ruleLabel)
        {
            if (SetsLoadType)
            {
                target.LoadType = LoadType;
                decisions[EAudioSetting.LoadType] = ruleLabel;
            }

            if (SetsCompressionFormat)
            {
                target.CompressionFormat = CompressionFormat;
                decisions[EAudioSetting.CompressionFormat] = ruleLabel;
            }

            if (SetsQuality)
            {
                target.Quality = Quality;
                decisions[EAudioSetting.Quality] = ruleLabel;
            }

            if (SetsSampleRate)
            {
                target.SampleRateSetting = SampleRateSetting;
                target.SampleRateOverride = SampleRateOverride;
                decisions[EAudioSetting.SampleRate] = ruleLabel;
            }

            if (SetsForceToMono)
            {
                target.ForceToMono = ForceToMono;
                decisions[EAudioSetting.ForceToMono] = ruleLabel;
            }

            if (SetsLoadInBackground)
            {
                target.LoadInBackground = LoadInBackground;
                decisions[EAudioSetting.LoadInBackground] = ruleLabel;
            }

            if (!SetsPreloadAudioData)
                return;

            target.PreloadAudioData = PreloadAudioData;
            decisions[EAudioSetting.PreloadAudioData] = ruleLabel;
        }
    }
}