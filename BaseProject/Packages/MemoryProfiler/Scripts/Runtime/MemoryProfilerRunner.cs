using System;
using System.IO;
using Base.SystemsCorePackage.SceneManagement;
using Base.SystemsCorePackage.Timers;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityMemoryProfiler = Unity.Profiling.Memory.MemoryProfiler;

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

        private static MemoryProfilerConfigSo config;
        private static Timer intervalTimer;

        /// <summary>Takes a snapshot immediately. Works in the editor and in development builds.</summary>
        public static void CaptureNow()
        {
            config ??= Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);

            if (config == null)
            {
                CustomLogger.LogError("Memory profiler config not found in a Resources folder.", null);
                return;
            }

            Capture(config);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            Stop();

            config = Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);
            if (config == null)
                return;

            Application.quitting -= Stop;
            Application.quitting += Stop;
            Begin();
        }
#endif

        private static void Begin()
        {
            if (!config.IsEnabled)
                return;

            if (config.CaptureOnInterval)
                StartIntervalTimer(config.IntervalSeconds);

            if (config.CaptureOnSceneLoad)
                SceneLoadEvents.OnSceneLoadCompleted += HandleSceneLoadCompleted;

            IsActive = true;
        }

        private static void Stop()
        {
            intervalTimer?.Stop();
            intervalTimer = null;
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

            intervalTimer = new Timer(intervalSeconds, true);
            intervalTimer.Completed += CaptureNow;
            intervalTimer.Start();
        }

        private static void HandleSceneLoadCompleted(string sceneName, bool success)
        {
            if (!success)
                return;

            CaptureNow();
        }

        private static void Capture(MemoryProfilerConfigSo activeConfig)
        {
            string directory = ResolveOutputDirectory(activeConfig.OutputFolder);
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

        private static string ResolveOutputDirectory(string relativeFolder)
        {
#if UNITY_EDITOR
            string root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
#else
            string root = Application.persistentDataPath;
#endif
            return Path.Combine(root, relativeFolder);
        }
    }
}