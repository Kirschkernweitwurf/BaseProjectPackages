using UnityEditor;

namespace Base.ToolPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// Reads and writes the sample settings of one import target. The default settings and a
    /// platform override live behind different importer calls, so every place that touches
    /// settings goes through here instead of branching on its own.
    /// </summary>
    internal static class AudioPlatformTargets
    {
        /// <summary>Label the default settings are shown with in the target dropdown.</summary>
        internal const string DefaultLabel = "Default";

        /// <summary>
        /// The settings that are in effect for a target. A platform without its own override
        /// inherits the default settings, which is what the importer does at build time too.
        /// </summary>
        /// <param name="importer">The importer to read.</param>
        /// <param name="platform">The target, empty for the default settings.</param>
        /// <returns>The settings in effect.</returns>
        internal static AudioImporterSampleSettings Read(AudioImporter importer, string platform)
        {
            if (IsDefault(platform))
                return importer.defaultSampleSettings;

            return importer.ContainsSampleSettingsOverride(platform)
                ? importer.GetOverrideSampleSettings(platform)
                : importer.defaultSampleSettings;
        }

        /// <summary>Writes the settings back onto the target.</summary>
        /// <param name="importer">The importer to write.</param>
        /// <param name="platform">The target, empty for the default settings.</param>
        /// <param name="settings">The settings to write.</param>
        internal static void Write(AudioImporter importer, string platform, AudioImporterSampleSettings settings)
        {
            if (IsDefault(platform))
            {
                importer.defaultSampleSettings = settings;
                return;
            }

            importer.SetOverrideSampleSettings(platform, settings);
        }

        // An empty target name stands for the default settings, which live behind their own importer
        // property rather than behind the override calls every named platform goes through.
        private static bool IsDefault(string platform) => string.IsNullOrWhiteSpace(platform);
    }
}