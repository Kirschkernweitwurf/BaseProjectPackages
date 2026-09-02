using System.Collections.Generic;
using Base.ToolsPackage.Editor.AudioRules.Model;
using Base.ToolsPackage.Editor.AudioRules.Scanning;
using UnityEditor;

namespace Base.ToolsPackage.Editor.AudioRules.Apply
{
    /// <summary>
    /// Writes resolved plans back onto the importers. This is the only place in the tool that
    /// changes anything, and it is only ever reached from an explicit Apply.
    /// <para>
    /// The mono and background load flags live on the importer rather than on a platform, so a
    /// plan resolved for a platform still writes those two to the shared importer. Everything else
    /// goes to the target the plan was resolved for.
    /// </para>
    /// </summary>
    internal static class AudioSettingsApplier
    {
        private const string ProgressTitle = "Applying audio rules";

        /// <summary>Applies every plan that has changes.</summary>
        /// <param name="plans">The plans to write.</param>
        /// <param name="platform">The import target the plans were resolved for.</param>
        /// <returns>How many clips were changed.</returns>
        internal static int Apply(IReadOnlyList<AudioClipPlan> plans, string platform)
        {
            if (plans == null
                || plans.Count == 0)
                return 0;

            int changed = 0;

            AssetDatabase.StartAssetEditing();

            try
            {
                for (int position = 0; position < plans.Count; position++)
                {
                    EditorUtility.DisplayProgressBar(ProgressTitle, plans[position].Info.Name,
                        position / (float)plans.Count);

                    if (ApplyOne(plans[position], platform))
                        changed++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();

            return changed;
        }

        /// <summary>Writes one set of values onto one importer without reimporting it.</summary>
        /// <param name="importer">The importer to write.</param>
        /// <param name="platform">The import target, empty for the default settings.</param>
        /// <param name="values">The values to write.</param>
        internal static void Write(AudioImporter importer, string platform, AudioSettingValues values)
        {
            AudioImporterSampleSettings settings = AudioPlatformTargets.Read(importer, platform);

            settings.loadType = values.LoadType;
            settings.compressionFormat = values.CompressionFormat;
            settings.quality = values.Quality;
            settings.sampleRateSetting = values.SampleRateSetting;
            settings.sampleRateOverride = (uint)values.SampleRateOverride;
            settings.preloadAudioData = values.PreloadAudioData;

            AudioPlatformTargets.Write(importer, platform, settings);

            importer.forceToMono = values.ForceToMono;
            importer.loadInBackground = values.LoadInBackground;
        }

        private static bool ApplyOne(AudioClipPlan plan, string platform)
        {
            if (!plan.HasChanges)
                return false;

            if (AssetImporter.GetAtPath(plan.Info.AssetPath) is not AudioImporter importer)
                return false;

            Write(importer, platform, plan.Target);
            importer.SaveAndReimport();

            return true;
        }
    }
}