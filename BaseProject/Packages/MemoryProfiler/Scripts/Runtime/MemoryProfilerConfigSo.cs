using Base.ToolPackage.MenuManagerWindow;
using Unity.Profiling.Memory;
using UnityEngine;

namespace Base.MemoryProfiler
{
    /// <summary>
    /// Runtime configuration for automated memory profiling.
    /// Loaded from a Resources folder so it ships in development builds.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Memory Profiler/New Memory Profiler Config", ConfigName)]
    public class MemoryProfilerConfigSo : ScriptableObject
    {
        /// <summary>Asset file name (without extension).</summary>
        public const string ConfigName = "MPC_MemoryProfilerConfig";

        /// <summary>Default storage path, matching the Memory Profiler preference default.</summary>
        public const string DefaultStoragePath = "./MemoryCaptures";

        /// <summary>Subfolder inside Resources that holds the config asset.</summary>
        public const string ResourceSubFolder = "MemoryProfilerConfig";

        /// <summary>Path used by Resources.Load, relative to a Resources folder.</summary>
        public const string ResourcePath = ResourceSubFolder + "/" + ConfigName;

        // Serialized field names for editor tooling (SerializedObject.FindProperty).
        public const string BakedStoragePathField = nameof(bakedStoragePath);
        public const string CaptureFlagsField = nameof(captureFlags);
        public const string CaptureOnIntervalField = nameof(captureOnInterval);
        public const string CaptureOnSceneLoadField = nameof(captureOnSceneLoad);
        public const string FileNamePrefixField = nameof(fileNamePrefix);
        public const string IntervalSecondsField = nameof(intervalSeconds);
        public const string IsEnabledField = nameof(isEnabled);
        public const string SnapshotStoragePathField = nameof(snapshotStoragePath);

        private const CaptureFlags DefaultCaptureFlags = CaptureFlags.ManagedObjects
            | CaptureFlags.NativeObjects
            | CaptureFlags.NativeAllocations
            | CaptureFlags.NativeAllocationSites
            | CaptureFlags.NativeStackTraces;

        [SerializeField] [Tooltip("Master switch for all automated captures.")]
        private bool isEnabled;

        [SerializeField] [Tooltip("Capture on a repeating timer while playing.")]
        private bool captureOnInterval = true;

        [SerializeField] [Tooltip("Capture every time a scene finishes loading.")]
        private bool captureOnSceneLoad = true;

        [SerializeField] [Tooltip("Seconds between interval captures.")]
        private float intervalSeconds = 30f;

        [SerializeField] [Tooltip("Mirror of the Memory Profiler 'Memory Snapshot Storage Path' "
            + "(Preferences > Analysis > Memory Profiler). Paths starting with ./ or ../ resolve "
            + "against the project root, in the editor and in builds. Absolute paths are used as is. "
            + "Copy the same value you set there to keep both in sync.")]
        private string snapshotStoragePath = DefaultStoragePath;

        [SerializeField] [Tooltip("Prefix used for every snapshot file name.")]
        private string fileNamePrefix = "Snapshot";

        [SerializeField] [Tooltip("Which memory categories to include in each snapshot.")]
        private CaptureFlags captureFlags = DefaultCaptureFlags;

        [SerializeField] [HideInInspector]
        private string bakedStoragePath;

        /// <summary>Master switch for all automated captures.</summary>
        public bool IsEnabled => isEnabled;

        /// <summary>Capture on a repeating timer while playing.</summary>
        public bool CaptureOnInterval => captureOnInterval;

        /// <summary>Capture every time a scene finishes loading.</summary>
        public bool CaptureOnSceneLoad => captureOnSceneLoad;

        /// <summary>Seconds between interval captures.</summary>
        public float IntervalSeconds => intervalSeconds;

        /// <summary>
        /// Snapshot storage path, mirroring the Memory Profiler preference. Relative paths
        /// resolve against the project root, absolute paths are used as is.
        /// </summary>
        public string SnapshotStoragePath => snapshotStoragePath;

        /// <summary>Prefix used for every snapshot file name.</summary>
        public string FileNamePrefix => fileNamePrefix;

        /// <summary>Which memory categories to include in each snapshot.</summary>
        public CaptureFlags SnapshotFlags => captureFlags;

        /// <summary>
        /// Absolute path baked at build time so a build resolves the project-relative path
        /// to the editor project folder. Empty in the editor and in committed assets.
        /// </summary>
        public string BakedStoragePath => bakedStoragePath;
    }
}
