using System;
using System.Collections.Generic;
using System.IO;
using Base.AttributePackage;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// The audio import conventions of a project. Rules cascade from top to bottom: every rule
    /// that matches applies, and a later one overwrites the settings it touches. That is why the
    /// seeded set reads as a base rule, then length bands, then a few targeted exceptions.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Audio Rules/New Rule Set", "ARS_AudioRuleSet")]
    public sealed class AudioRuleSet : ScriptableObject
    {
        private const string AssetFilter = "t:" + nameof(AudioRuleSet);
        private const float BandLongSeconds = 20f;
        private const float BandMediumSeconds = 5f;
        private const float BandShortSeconds = 0.5f;
        private const string DefaultAssetName = "ARS_AudioRuleSet.asset";
        private const string DefaultFolder = "Assets/Editor/AudioRules";
        private const int DefaultsVersion = 1;
        private const string FolderSeparator = "/";
        private const int MobileSampleRate = 22050;
        private const float QualityHigh = 0.7f;
        private const float QualityLow = 0.4f;
        private const float QualityMedium = 0.6f;
        private const string SpatialCategory = "Sfx3D";
        private const int VoiceSampleRate = 22050;

        private static readonly string[] DefaultIgnoredFragments =
        {
            "/Plugins/",
            "/ThirdParty/",
            "/TextMesh Pro/"
        };

        private static readonly string[] DefaultPlatforms =
        {
            "Standalone",
            "Android",
            "iOS",
            "WebGL"
        };

        [Title("Rules")]
        [Tooltip("Every rule that matches applies, top to bottom. A later rule overwrites the settings"
            + " it touches, so put the broad ones first and the exceptions last.")]
        [SerializeField] private List<AudioRule> rules = new();

        [Title("Import")]
        [Tooltip("Whether new clips are set up automatically as they are imported.")]
        [SerializeField] private EImportEnforcement importEnforcement = EImportEnforcement.FirstImportOnly;

        [Title("Analysis")]
        [Tooltip("Reads the sample data of every clip after a scan to find fake stereo, silence, clipping"
            + " and quiet clips. Runs in the background and caches per file, so turning it off only helps"
            + " on the very first scan of a huge project.")]
        [SerializeField] private bool analyzeSampleData = true;

        [Tooltip("What the analysis counts as silence, clipping, quiet or fake stereo.")]
        [SerializeField] private AudioAnalysisSettings analysis = new();

        [Title("Categories")]
        [Tooltip("Optional. Where the category and loop flag of a clip come from, so rules can use them"
            + " as conditions.")]
        [SerializeField] private List<AudioContainerBinding> containerBindings = new();

        [Title("Scope")]
        [Tooltip("If true, clips inside the Packages folder are scanned as well. You cannot change their"
            + " import settings, so this is off by default.")]
        [SerializeField] private bool includePackages;

        [Tooltip("Clips whose path contains one of these fragments are skipped.")]
        [Unique]
        [SerializeField] private List<string> ignoredPathFragments = new();

        [Tooltip("Platforms offered in the target dropdown, spelled the way the importer expects them.")]
        [Unique]
        [SerializeField] private List<string> platforms = new();

        [Tooltip("Set of defaults this asset was last filled with. Raised when new ones ship.")]
        [SerializeField] private int defaultsVersion;

        /// <summary>
        /// The rules in cascade order. The window edits this list in place and calls
        /// <see cref="Persist"/> afterwards, so reordering a rule is a plain list move.
        /// </summary>
        public List<AudioRule> Rules => rules;

        /// <summary>Where the category and loop flag of a clip come from.</summary>
        public IReadOnlyList<AudioContainerBinding> ContainerBindings => containerBindings;

        /// <summary>Platforms offered in the target dropdown.</summary>
        public IReadOnlyList<string> Platforms => platforms;

        /// <summary>What the analysis judges by.</summary>
        public AudioAnalysisSettings Analysis => analysis;

        /// <summary>If true, a scan reads sample data in the background once the settings are compared.</summary>
        public bool AnalyzeSampleData => analyzeSampleData;

        /// <summary>Whether new clips are set up automatically as they are imported.</summary>
        public EImportEnforcement ImportEnforcement => importEnforcement;

        /// <summary>If true, clips inside the Packages folder are scanned as well.</summary>
        public bool IncludePackages => includePackages;

        /// <summary>Returns the first rule set found in the project, or null when there is none.</summary>
        /// <returns>The rule set, ready to scan with.</returns>
        public static AudioRuleSet Load()
        {
            foreach (string guid in AssetDatabase.FindAssets(AssetFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioRuleSet found = AssetDatabase.LoadAssetAtPath<AudioRuleSet>(path);

                if (found == null)
                    continue;

                found.EnsureDefaults();

                return found;
            }

            return null;
        }

        /// <summary>Creates a rule set asset that is already filled with the default cascade.</summary>
        /// <returns>The created rule set.</returns>
        public static AudioRuleSet Create()
        {
            Directory.CreateDirectory(DefaultFolder);
            AssetDatabase.Refresh();

            AudioRuleSet instance = CreateInstance<AudioRuleSet>();
            instance.EnsureDefaults();

            string path = AssetDatabase.GenerateUniqueAssetPath(DefaultFolder + FolderSeparator + DefaultAssetName);

            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            CustomLogger.Log($"Created an audio rule set at {path}.", instance);

            return instance;
        }

        /// <summary>
        /// Fills the empty lists on first use and tops them up when the tool ships new defaults.
        /// Only missing entries are added, so an entry deleted on purpose stays gone.
        /// </summary>
        public void EnsureDefaults()
        {
            if (defaultsVersion >= DefaultsVersion)
                return;

            foreach (string fragment in DefaultIgnoredFragments)
            {
                if (!ignoredPathFragments.Contains(fragment))
                    ignoredPathFragments.Add(fragment);
            }

            foreach (string platform in DefaultPlatforms)
            {
                if (!platforms.Contains(platform))
                    platforms.Add(platform);
            }

            if (containerBindings.Count == 0)
                containerBindings.Add(new AudioContainerBinding());

            if (rules.Count == 0)
                rules.AddRange(CreateDefaultRules());

            defaultsVersion = DefaultsVersion;

            Persist();
        }

        /// <summary>Writes the asset to disk so the rules end up in source control.</summary>
        public void Persist()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        /// <summary>True when the clip is excluded from the scan.</summary>
        /// <param name="assetPath">Project relative path of the clip.</param>
        /// <returns>True when the path is ignored.</returns>
        public bool IsIgnoredPath(string assetPath)
        {
            foreach (string fragment in ignoredPathFragments)
            {
                if (string.IsNullOrWhiteSpace(fragment))
                    continue;

                if (assetPath.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        // Length decides how a clip should live in memory, so the bands carry the codec and the load
        // type. Everything after them is an exception that only touches what makes it special.
        private static AudioRule[] CreateDefaultRules()
        {
            AudioRule everything = new("Everything");

            everything.Notes = "The baseline every clip starts from. Keeps the source sample rate and loads on "
                + "the main thread, unless a later rule decides otherwise.";

            everything.Overrides.SetsSampleRate = true;
            everything.Overrides.SampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            everything.Overrides.SetsLoadInBackground = true;
            everything.Overrides.LoadInBackground = false;
            everything.Overrides.SetsPreloadAudioData = true;
            everything.Overrides.PreloadAudioData = true;

            AudioRule tiny = new($"Under {BandShortSeconds:0.0} s");

            tiny.Notes = "Very short one shots are cheapest uncompressed. Decoding them would cost more CPU "
                + "per play than the few kilobytes it would save.";

            tiny.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.LessThan, BandShortSeconds));
            tiny.Overrides.SetsLoadType = true;
            tiny.Overrides.LoadType = AudioClipLoadType.DecompressOnLoad;
            tiny.Overrides.SetsCompressionFormat = true;
            tiny.Overrides.CompressionFormat = AudioCompressionFormat.PCM;

            AudioRule shortClip = new($"{BandShortSeconds:0.0} s to {BandMediumSeconds:0} s");

            shortClip.Notes = "Short effects fire constantly, so they are decompressed once and stored as "
                + "ADPCM, which costs almost nothing to decode and is a third of the size of raw audio.";

            shortClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, BandShortSeconds));
            shortClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.LessThan, BandMediumSeconds));
            shortClip.Overrides.SetsLoadType = true;
            shortClip.Overrides.LoadType = AudioClipLoadType.DecompressOnLoad;
            shortClip.Overrides.SetsCompressionFormat = true;
            shortClip.Overrides.CompressionFormat = AudioCompressionFormat.ADPCM;

            AudioRule mediumClip = new($"{BandMediumSeconds:0} s to {BandLongSeconds:0} s");

            mediumClip.Notes = "Long enough that raw audio hurts, short enough to keep in memory. Stored "
                + "compressed and decoded per play.";

            mediumClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, BandMediumSeconds));
            mediumClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.LessThan, BandLongSeconds));
            mediumClip.Overrides.SetsLoadType = true;
            mediumClip.Overrides.LoadType = AudioClipLoadType.CompressedInMemory;
            mediumClip.Overrides.SetsCompressionFormat = true;
            mediumClip.Overrides.CompressionFormat = AudioCompressionFormat.Vorbis;
            mediumClip.Overrides.SetsQuality = true;
            mediumClip.Overrides.Quality = QualityHigh;

            AudioRule longClip = new($"Over {BandLongSeconds:0} s");

            longClip.Notes = "Music and long ambience stream from disk, so they cost a small buffer instead "
                + "of their full size, and load off the main thread so the scene does not stall.";

            longClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, BandLongSeconds));
            longClip.Overrides.SetsLoadType = true;
            longClip.Overrides.LoadType = AudioClipLoadType.Streaming;
            longClip.Overrides.SetsCompressionFormat = true;
            longClip.Overrides.CompressionFormat = AudioCompressionFormat.Vorbis;
            longClip.Overrides.SetsQuality = true;
            longClip.Overrides.Quality = QualityMedium;
            longClip.Overrides.SetsLoadInBackground = true;
            longClip.Overrides.LoadInBackground = true;
            longClip.Overrides.SetsPreloadAudioData = true;
            longClip.Overrides.PreloadAudioData = false;

            AudioRule spatial = new("Spatial sound effects");

            spatial.Notes = "A stereo clip cannot be spatialized. It plays flat and ignores where it is in "
                + "the world, so anything positional is forced to mono, which also halves its size.";

            spatial.Conditions.Add(new AudioRuleCondition(EConditionField.Category,
                EConditionOperator.Equals, SpatialCategory));
            spatial.Overrides.SetsForceToMono = true;
            spatial.Overrides.ForceToMono = true;

            AudioRule voice = new("Voice over");

            voice.Notes = "Speech carries no detail worth 44 kHz, and a voice line is never stereo, so both "
                + "halvings are free in quality terms.";

            voice.Conditions.Add(new AudioRuleCondition(EConditionField.Path,
                EConditionOperator.Contains, "/VO/"));
            voice.Overrides.SetsForceToMono = true;
            voice.Overrides.ForceToMono = true;
            voice.Overrides.SetsSampleRate = true;
            voice.Overrides.SampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            voice.Overrides.SampleRateOverride = VoiceSampleRate;

            AudioRule mobileLong = new("Long clips on mobile")
            {
                PlatformTarget = "Android",
                Notes = "Mobile ships on a tighter budget and phone speakers hide most of what the lower "
                    + "quality gives up."
            };

            mobileLong.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, BandLongSeconds));
            mobileLong.Overrides.SetsQuality = true;
            mobileLong.Overrides.Quality = QualityLow;
            mobileLong.Overrides.SetsSampleRate = true;
            mobileLong.Overrides.SampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            mobileLong.Overrides.SampleRateOverride = MobileSampleRate;

            return new[]
            {
                everything,
                tiny,
                shortClip,
                mediumClip,
                longClip,
                spatial,
                voice,
                mobileLong
            };
        }
    }
}