using System;
using Base.ToolPackage.Editor.AudioRules.Model;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// Turns the numbers behind a plan into the short strings the table shows. Kept apart from the
    /// views so the same wording is used in the table, the details pane and the status bar.
    /// </summary>
    internal static class AudioRulesFormat
    {
        private const string ByteSuffix = " B";
        private const string DecibelFormat = "0.0";
        private const string DecibelSuffix = " dBFS";
        private const float Kilobyte = 1024f;
        private const string KilobyteSuffix = " KB";
        private const float KilohertzFactor = 1000f;
        private const string KilohertzFormat = "0.#";
        private const string KilohertzSuffix = " kHz";
        private const float Megabyte = Kilobyte * Kilobyte;
        private const string MegabyteSuffix = " MB";
        private const string SecondsSuffix = " s";
        private const string ShortNumber = "0.0";
        private const string SilenceLabel = "-inf";

        /// <summary>Formats a size, picking the unit that keeps the number short.</summary>
        /// <param name="bytes">The size in bytes.</param>
        /// <returns>The size with its unit.</returns>
        internal static string Size(long bytes)
        {
            if (bytes >= Megabyte)
                return (bytes / Megabyte).ToString(ShortNumber) + MegabyteSuffix;

            if (bytes >= Kilobyte)
                return (bytes / Kilobyte).ToString(ShortNumber) + KilobyteSuffix;

            return bytes + ByteSuffix;
        }

        /// <summary>Formats a size difference, with a sign so a cost reads differently to a saving.</summary>
        /// <param name="bytes">The difference in bytes, positive when it saves.</param>
        /// <returns>The signed difference, or a dash when it is zero.</returns>
        internal static string Delta(long bytes)
        {
            if (bytes == 0L)
                return "-";

            return bytes > 0L
                ? "-" + Size(bytes)
                : "+" + Size(-bytes);
        }

        /// <summary>
        /// Formats a linear sample level as decibels below full scale, which is the unit every
        /// meter and every audio tool uses. A linear 0.25 means nothing at a glance, -12 dBFS does.
        /// </summary>
        /// <param name="linear">The level, 1 being full scale.</param>
        /// <returns>The level in dBFS.</returns>
        internal static string Decibels(float linear)
        {
            if (linear <= 0f)
                return SilenceLabel + DecibelSuffix;

            return (20f * (float)Math.Log10(linear)).ToString(DecibelFormat) + DecibelSuffix;
        }

        /// <summary>Formats a sample rate the way it is spoken about.</summary>
        /// <param name="hertz">The rate in Hz.</param>
        /// <returns>The rate in kHz.</returns>
        internal static string Kilohertz(int hertz)
            => (hertz / KilohertzFactor).ToString(KilohertzFormat) + KilohertzSuffix;

        /// <summary>Formats a duration in seconds.</summary>
        /// <param name="seconds">The duration.</param>
        /// <returns>The duration with its unit.</returns>
        internal static string Seconds(float seconds) => seconds.ToString(ShortNumber) + SecondsSuffix;

        /// <summary>The short form of what a clip is imported as today.</summary>
        /// <param name="values">The settings to describe.</param>
        /// <returns>Codec and load type in one line.</returns>
        internal static string Summary(AudioSettingValues values) => $"{values.CompressionFormat} / {Short(values)}";

        /// <summary>Picks the singular or plural word for an amount.</summary>
        /// <param name="amount">The amount the word describes.</param>
        /// <param name="singular">The word used for exactly one.</param>
        /// <param name="plural">The word used for every other amount.</param>
        /// <returns>The word matching the amount.</returns>
        internal static string Plural(int amount, string singular, string plural) => amount == 1
            ? singular
            : plural;

        // The load type names are long enough to push every other column off screen.
        private static string Short(AudioSettingValues values) => values.LoadType switch
        {
            AudioClipLoadType.DecompressOnLoad => "Decompress",
            AudioClipLoadType.CompressedInMemory => "In memory",
            _ => "Streaming"
        };
    }
}