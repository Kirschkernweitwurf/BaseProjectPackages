using Unity.Profiling.Memory;
using UnityEngine;

namespace Base.MemoryProfiler
{
    /// <summary>
    /// Runtime configuration for automated memory profiling.
    /// Loaded from a Resources folder so it ships in development builds.
    /// </summary>
    [CreateAssetMenu(fileName = ConfigName, menuName = "ScriptableObjects/Base/Memory Profiler/Memory Profiler Config")]
    public class MemoryProfilerConfigSo : ScriptableObject
    {
        /// <summary>Asset file name (without extension).</summary>
        public const string ConfigName = "MemoryProfilerConfig";

        private const CaptureFlags DefaultCaptureFlags = CaptureFlags.ManagedObjects
            | CaptureFlags.NativeObjects
            | CaptureFlags.NativeAllocations
            | CaptureFlags.NativeAllocationSites
            | CaptureFlags.NativeStackTraces;

        /// <summary>Path used by Resources.Load, relative to a Resources folder.</summary>
        public const string ResourcePath = ResourceSubFolder + "/" + ConfigName;

        private const string ResourceSubFolder = "MemoryProfilerConfig";

        [SerializeField] private bool isEnabled;
        [SerializeField] private bool captureOnInterval = true;
        [SerializeField] private bool captureOnSceneLoad = true;
        [SerializeField] private float intervalSeconds = 30f;
        [SerializeField] private string outputFolder = "MemoryCaptures";
        [SerializeField] private string fileNamePrefix = "Snapshot";
        [SerializeField] private CaptureFlags captureFlags = DefaultCaptureFlags;

        /// <summary>Master switch for all automated captures.</summary>
        public bool IsEnabled => isEnabled;

        /// <summary>Capture on a repeating timer while playing.</summary>
        public bool CaptureOnInterval => captureOnInterval;

        /// <summary>Capture every time a scene finishes loading.</summary>
        public bool CaptureOnSceneLoad => captureOnSceneLoad;

        /// <summary>Seconds between interval captures.</summary>
        public float IntervalSeconds => intervalSeconds;

        /// <summary>Output folder, relative to the project root in editor or persistent data in builds.</summary>
        public string OutputFolder => outputFolder;

        /// <summary>Prefix used for every snapshot file name.</summary>
        public string FileNamePrefix => fileNamePrefix;

        /// <summary>Which memory categories to include in each snapshot.</summary>
        public CaptureFlags SnapshotFlags => captureFlags;
    }
}