using System.IO;
using Base.ToolsPackage.Editor.AudioRules.Apply;
using Base.ToolsPackage.Editor.AudioRules.Data;
using Base.ToolsPackage.Editor.AudioRules.Model;
using Base.ToolsPackage.Editor.AudioRules.Scanning;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AudioRules.Import
{
    /// <summary>
    /// Applies the rules while clips are imported, so a file dropped into the project arrives
    /// correct and never shows up in the window at all.
    /// <para>
    /// The work happens after the import rather than before it, because length, channel count and
    /// sample rate are only known once the clip exists, and those are exactly what the length
    /// bands test. Writing the settings needs a second import, which is requested once the current
    /// one has finished. That does not loop: the second pass resolves to the settings the clip now
    /// has, finds nothing to change and stops.
    /// </para>
    /// <para>
    /// Category and loop conditions do not apply here. Which container references a clip is not
    /// known during its import, and building that index per imported file would make every import
    /// slow. Those rules land in the window instead.
    /// </para>
    /// </summary>
    internal sealed class AudioRulePostprocessor : AssetPostprocessor
    {
#region Unity Callbacks
        private void OnPostprocessAudio(AudioClip clip)
        {
            AudioRuleSet ruleSet = AudioRuleSet.Load();

            if (assetImporter is not AudioImporter importer
                || IsSkipped(ruleSet, assetImporter, assetPath))
                return;

            if (!ApplyAllTargets(ruleSet, importer, clip))
                return;

            string path = assetPath;

            EditorApplication.delayCall += () => Reimport(path);
        }
#endregion

        /// <summary>Raise this when the tool changes what it writes, so Unity reimports.</summary>
        /// <returns>The version of this postprocessor.</returns>
        public override uint GetVersion() => 1u;

        private static void Reimport(string assetPath)
        {
            if (AssetImporter.GetAtPath(assetPath) is AudioImporter importer)
                importer.SaveAndReimport();
        }

        private static bool IsSkipped(AudioRuleSet ruleSet, AssetImporter importer, string assetPath)
        {
            if (ruleSet == null
                || ruleSet.ImportEnforcement == EImportEnforcement.Never)
                return true;

            // A reimport of a clip somebody tuned by hand would silently undo that work.
            if (ruleSet.ImportEnforcement == EImportEnforcement.FirstImportOnly
                && !importer.importSettingsMissing)
                return true;

            return ruleSet.IsIgnoredPath(assetPath);
        }

        private static long ReadFileSize(string path)
        {
            FileInfo file = new(Path.GetFullPath(path));

            return file.Exists
                ? file.Length
                : 0L;
        }

        private bool ApplyAllTargets(AudioRuleSet ruleSet, AudioImporter importer, AudioClip clip)
        {
            bool changed = ApplyTarget(ruleSet, importer, clip, string.Empty);

            foreach (string platform in ruleSet.Platforms)
            {
                if (!string.IsNullOrWhiteSpace(platform))
                    changed |= ApplyTarget(ruleSet, importer, clip, platform);
            }

            return changed;
        }

        private bool ApplyTarget(AudioRuleSet ruleSet, AudioImporter importer, AudioClip clip, string platform)
        {
            AudioClipInfo info = BuildInfo(importer, clip, platform);
            AudioClipPlan plan = AudioRuleResolver.Resolve(info, ruleSet, platform, null);

            if (!plan.HasChanges)
                return false;

            AudioSettingsApplier.Write(importer, platform, plan.Target);

            return true;
        }

        private AudioClipInfo BuildInfo(AudioImporter importer, AudioClip clip, string platform) => new(assetPath,
            AssetDatabase.AssetPathToGUID(assetPath),
            Path.GetFileNameWithoutExtension(assetPath), clip.length, clip.channels, clip.frequency,
            ReadFileSize(assetPath), string.Empty, false, false,
            AudioClipCollector.ReadCurrent(importer, platform));
    }
}