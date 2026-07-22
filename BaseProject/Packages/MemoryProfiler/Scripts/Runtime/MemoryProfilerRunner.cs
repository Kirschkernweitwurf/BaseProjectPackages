using System;
using System.IO;
using Base.CorePackage.SceneManagement;
using Base.CorePackage.Timers;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityMemoryProfiler = Unity.Profiling.Memory.MemoryProfiler;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.MemoryProfiler
{
    /// <summary>
    /// Runs automated memory snapshots in the editor and in development builds.
    /// Auto-start is compiled out of release builds, where capture is not supported.
    /// </summary>
    public static class MemoryProfilerRunner
    {
        private const string SnapshotExtension = ".snap";
        private const string TimestampFormat = "yyyy-MM-dd_HH-mm-ss";

        /// <summary>Path of the most recent snapshot, or null if none was taken.</summary>
        public static string LastSnapshotPath { get; private set; }

        /// <summary>True while automated captures are armed.</summary>
        public static bool IsActive { get; private set; }

        private static MemoryProfilerConfigSo _config;
        private static Timer _intervalTimer;

        /// <summary>Takes a snapshot immediately. Works in the editor and in development builds.</summary>
        public static void CaptureNow()
        {
            if (_config == null)
                _config = Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);

            if (_config == null)
            {
                CustomLogger.LogError("Memory profiler config not found in a Resources folder.", null);
                return;
            }

            Capture(_config);
        }

        /// <summary>
        /// Resolves the snapshot folder the same way the Memory Profiler does. Relative paths
        /// resolve against the project root, absolute paths are returned unchanged. Builds use
        /// the path baked at build time so the project root stays correct off the editor machine.
        /// </summary>
        public static string ResolveStorageDirectory(MemoryProfilerConfigSo config)
        {
#if !UNITY_EDITOR
            if (!string.IsNullOrEmpty(config.BakedStoragePath))
                return config.BakedStoragePath;
#endif
            string root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            return ResolveAbsolute(config.SnapshotStoragePath, root);
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics()
        {
            LastSnapshotPath = null;
            _config = null;
        }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            Stop();

            _config = Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);
            if (_config == null)
                return;

            Application.quitting -= Stop;
            Application.quitting += Stop;
            Begin();
        }
#endif

        private static void Begin()
        {
            if (!_config.IsEnabled)
                return;

            if (_config.CaptureOnInterval)
                StartIntervalTimer(_config.IntervalSeconds);

            if (_config.CaptureOnSceneLoad)
                SceneLoadEvents.OnSceneLoadCompleted += HandleSceneLoadCompleted;

            IsActive = true;
        }

        private static void Stop()
        {
            _intervalTimer?.Stop();
            _intervalTimer = null;
            SceneLoadEvents.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
            IsActive = false;
        }

        private static void StartIntervalTimer(float intervalSeconds)
        {
            if (intervalSeconds <= 0f)
            {
                CustomLogger.LogError($"Memory profiling interval must be positive, got {intervalSeconds}.", null);
                return;
            }

            _intervalTimer = new Timer(intervalSeconds, true);
            _intervalTimer.Completed += CaptureNow;
            _intervalTimer.Start();
        }

        private static void HandleSceneLoadCompleted(string sceneName, bool success)
        {
            if (!success)
                return;

            CaptureNow();
        }

        private static void Capture(MemoryProfilerConfigSo activeConfig)
        {
            string directory = ResolveStorageDirectory(activeConfig);
            Directory.CreateDirectory(directory);

            string timestamp = DateTime.Now.ToString(TimestampFormat);
            string fileName = $"{activeConfig.FileNamePrefix}_{timestamp}{SnapshotExtension}";
            string path = Path.Combine(directory, fileName);

            UnityMemoryProfiler.TakeSnapshot(path, OnSnapshotFinished, activeConfig.SnapshotFlags);
        }

        private static void OnSnapshotFinished(string path, bool success)
        {
            if (!success)
            {
                CustomLogger.LogError($"Memory snapshot failed: {path}", null);
                return;
            }

            LastSnapshotPath = path;
            CustomLogger.Log($"Memory snapshot saved: {path}", null);
        }

        private static string ResolveAbsolute(string storagePath, string root)
        {
            if (string.IsNullOrEmpty(storagePath))
                storagePath = MemoryProfilerConfigSo.DefaultStoragePath;

            if (Path.IsPathRooted(storagePath))
                return storagePath;

            return Path.GetFullPath(Path.Combine(root, storagePath));
        }
    }
}