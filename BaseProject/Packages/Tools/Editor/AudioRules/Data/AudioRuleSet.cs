using System;
using System.Collections.Generic;
using System.IO;
using Base.AttributesPackage;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// The audio import conventions of a project. Rules cascade from top to bottom: every rule
    /// that matches applies, and a later one overwrites the settings it touches. That is why the
    /// seeded set reads as a base rule, then length bands, then a few targeted exceptions.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Audio Rules/New Rule Set", "ARS_AudioRuleSet")]
    internal sealed class AudioRuleSet : ScriptableObject
    {
        private const string AssetFilter = "t:" + nameof(AudioRuleSet);
        private const float BandLongSeconds = 30f;
        private const float BandMediumSeconds = 10f;
        private const float BandShortSeconds = 1f;
        private const string DefaultAssetName = "ARS_AudioRuleSet.asset";
        private const string DefaultFolder = "Assets/Editor/AudioRules";
        private const int DefaultsVersion = 1;
        private const string FolderSeparator = "/";
        private const string MusicCategory = "Music";
        private const float QualityMobile = 0.5f;
        private const float QualityMobileMusic = 0.7f;
        private const float QualityMusic = 0.85f;
        private const float QualityStandard = 0.7f;
        private const string SpatialCategory = "Sfx3D";
        private const string VoicePathFragment = "/VO/";

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

        private static readonly string[] MobilePlatforms =
        {
            "Android",
            "iOS"
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
        // type. Everything after them is an exception that only touches what makes it special. The
        // values follow Unity's audio import docs and its published import recommendations.
        private static List<AudioRule> CreateDefaultRules()
        {
            List<AudioRule> rules = new()
            {
                CreateBaseline()
            };

            rules.AddRange(CreateLengthBands());
            rules.AddRange(CreateExceptions());

            foreach (string platform in MobilePlatforms)
                rules.AddRange(CreateMobileRules(platform));

            return rules;
        }

        private static AudioRule CreateBaseline()
        {
            AudioRule everything = new("Everything")
            {
                Notes = "The baseline every clip starts from. Keeps the source sample rate, because a lower "
                    + "one gets audible as soon as a sound is pitched down, and loads on the main thread, "
                    + "unless a later rule decides otherwise."
            };

            everything.Overrides.SetsSampleRate = true;
            everything.Overrides.SampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            everything.Overrides.SetsLoadInBackground = true;
            everything.Overrides.LoadInBackground = false;
            everything.Overrides.SetsPreloadAudioData = true;
            everything.Overrides.PreloadAudioData = true;

            return everything;
        }

        private static AudioRule[] CreateLengthBands()
        {
            AudioRule tiny = new($"Under {BandShortSeconds:0} s")
            {
                Notes = "Short one shots stay raw PCM. A second of 48 kHz stereo is under 200 KB, and "
                    + "playing PCM costs almost no CPU, about a tenth of a Vorbis voice. ADPCM is left out on "
                    + "purpose: it adds audible artifacts to clean, tonal sounds like UI chimes."
            };

            tiny.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.LessThan, BandShortSeconds));

            tiny.Overrides.SetsLoadType = true;
            tiny.Overrides.LoadType = AudioClipLoadType.DecompressOnLoad;
            tiny.Overrides.SetsCompressionFormat = true;
            tiny.Overrides.CompressionFormat = AudioCompressionFormat.PCM;

            AudioRule shortClip = new($"{BandShortSeconds:0} s to {BandMediumSeconds:0} s")
            {
                Notes = "Small on disk as Vorbis and decoded once while loading, so playing costs nothing "
                    + "extra. Decoded audio takes about ten times the memory of Vorbis, which is fine for a "
                    + "few seconds. At 70% most listeners cannot tell Vorbis from the original."
            };

            shortClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, BandShortSeconds));

            shortClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.LessThan, BandMediumSeconds));

            shortClip.Overrides.SetsLoadType = true;
            shortClip.Overrides.LoadType = AudioClipLoadType.DecompressOnLoad;
            SetVorbis(shortClip, QualityStandard);

            AudioRule mediumClip = new($"{BandMediumSeconds:0} s to {BandLongSeconds:0} s")
            {
                Notes = "Too long to decode up front, too short to be worth a stream. Stays compressed in "
                    + "memory and is decoded while it plays, for a small CPU cost per voice."
            };

            mediumClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, BandMediumSeconds));

            mediumClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.LessThan, BandLongSeconds));

            mediumClip.Overrides.SetsLoadType = true;
            mediumClip.Overrides.LoadType = AudioClipLoadType.CompressedInMemory;
            SetVorbis(mediumClip, QualityStandard);

            AudioRule longClip = new($"Over {BandLongSeconds:0} s")
            {
                Notes = "Long music and ambience are read from disk in small chunks, so they cost a buffer "
                    + "of about 200 KB instead of their full size. Every stream also costs disk and CPU time, "
                    + "so plan on no more than two playing at once."
            };

            longClip.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, BandLongSeconds));

            longClip.Overrides.SetsLoadType = true;
            longClip.Overrides.LoadType = AudioClipLoadType.Streaming;
            SetVorbis(longClip, QualityStandard);
            longClip.Overrides.SetsPreloadAudioData = true;
            longClip.Overrides.PreloadAudioData = false;

            return new[]
            {
                tiny,
                shortClip,
                mediumClip,
                longClip
            };
        }

        private static AudioRule[] CreateExceptions()
        {
            AudioRule music = new("Music")
            {
                Notes = "Music is listened to rather than heard in passing, so it keeps more detail than "
                    + "effects. Its length still decides whether it streams."
            };

            music.Conditions.Add(new AudioRuleCondition(EConditionField.Category,
                EConditionOperator.Equals, MusicCategory));

            music.Overrides.SetsQuality = true;
            music.Overrides.Quality = QualityMusic;

            AudioRule spatial = new("Spatial sound effects")
            {
                Notes = "A stereo clip cannot be spatialized. It plays flat and ignores where it is in "
                    + "the world, so anything positional is forced to mono, which also halves its size."
            };

            spatial.Conditions.Add(new AudioRuleCondition(EConditionField.Category,
                EConditionOperator.Equals, SpatialCategory));

            spatial.Overrides.SetsForceToMono = true;
            spatial.Overrides.ForceToMono = true;

            AudioRule voice = new("Voice over")
            {
                Notes = "Voice lines are recorded in mono, so mono loses nothing. A game has many of them, "
                    + "so they stay compressed in memory whatever their length."
            };

            voice.Conditions.Add(new AudioRuleCondition(EConditionField.Path,
                EConditionOperator.Contains, VoicePathFragment));

            voice.Overrides.SetsForceToMono = true;
            voice.Overrides.ForceToMono = true;
            voice.Overrides.SetsLoadType = true;
            voice.Overrides.LoadType = AudioClipLoadType.CompressedInMemory;
            SetVorbis(voice, QualityStandard);

            return new[]
            {
                music,
                spatial,
                voice
            };
        }

        // Phones ship on a tighter budget and their speakers hide most of what a lower quality gives
        // up. The sample rate stays, because a lowered one aliases and dulls pitched sounds.
        private static AudioRule[] CreateMobileRules(string platform)
        {
            AudioRule effects = new($"Compressed clips on {platform}")
            {
                PlatformTarget = platform,
                Notes = "Everything that is not raw PCM gets a lower Vorbis quality on phones."
            };

            effects.Conditions.Add(new AudioRuleCondition(EConditionField.DurationSeconds,
                EConditionOperator.GreaterOrEqual, BandShortSeconds));

            effects.Overrides.SetsQuality = true;
            effects.Overrides.Quality = QualityMobile;

            AudioRule music = new($"Music on {platform}")
            {
                PlatformTarget = platform,
                Notes = "Music keeps a little more quality than effects on phones too."
            };

            music.Conditions.Add(new AudioRuleCondition(EConditionField.Category,
                EConditionOperator.Equals, MusicCategory));

            music.Overrides.SetsQuality = true;
            music.Overrides.Quality = QualityMobileMusic;

            return new[]
            {
                effects,
                music
            };
        }

        private static void SetVorbis(AudioRule rule, float quality)
        {
            rule.Overrides.SetsCompressionFormat = true;
            rule.Overrides.CompressionFormat = AudioCompressionFormat.Vorbis;
            rule.Overrides.SetsQuality = true;
            rule.Overrides.Quality = quality;
            rule.Overrides.SetsLoadInBackground = true;
            rule.Overrides.LoadInBackground = true;
        }
    }
}