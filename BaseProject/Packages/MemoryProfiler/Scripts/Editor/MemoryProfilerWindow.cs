using System.IO;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.MemoryProfiler.Editor
{
    /// <summary>
    /// Editor window to edit the runtime config and trigger manual captures.
    /// </summary>
    public class MemoryProfilerWindow : EditorWindow
    {
        private const string AssetsFolder = "Assets";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Memory Profiler Automation";
        private const float MinIntervalSeconds = 1f;
        private const string ResourcesFolderName = "Resources";
        private const string ResourcesRoot = AssetsFolder + "/" + ResourcesFolderName;
        private const string ConfigFolder = ResourcesRoot + "/" + MemoryProfilerConfigSo.ResourceSubFolder;
        private const string WindowTitle = "Auto Memory Profiler";

        private static readonly GUIContent EnabledLabel = new("Enabled");
        private static readonly GUIContent OnIntervalLabel = new("Capture On Interval");
        private static readonly GUIContent IntervalLabel = new("Interval (seconds)");
        private static readonly GUIContent OnSceneLoadLabel = new("Capture On Scene Load");
        private static readonly GUIContent StoragePathLabel = new("Snapshot Storage Path");
        private static readonly GUIContent PrefixLabel = new("File Name Prefix");
        private static readonly GUIContent FlagsLabel = new("Capture Flags");

        private SerializedObject _serializedConfig;
        private SerializedProperty _isEnabled;
        private SerializedProperty _captureOnInterval;
        private SerializedProperty _intervalSeconds;
        private SerializedProperty _captureOnSceneLoad;
        private SerializedProperty _snapshotStoragePath;
        private SerializedProperty _fileNamePrefix;
        private SerializedProperty _captureFlags;

#region Unity Callbacks
        private void OnEnable() => RefreshConfigReference();

        private void OnGUI()
        {
            if (_serializedConfig == null || _serializedConfig.targetObject == null)
            {
                DrawMissingConfig();
                return;
            }

            _serializedConfig.Update();

            EditorGUILayout.LabelField("Automation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_isEnabled, EnabledLabel);

            using (new EditorGUI.DisabledScope(!_isEnabled.boolValue))
            {
                EditorGUILayout.PropertyField(_captureOnInterval, OnIntervalLabel);

                using (new EditorGUI.DisabledScope(!_captureOnInterval.boolValue))
                    EditorGUILayout.PropertyField(_intervalSeconds, IntervalLabel);

                EditorGUILayout.PropertyField(_captureOnSceneLoad, OnSceneLoadLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_snapshotStoragePath, StoragePathLabel);
            EditorGUILayout.PropertyField(_fileNamePrefix, PrefixLabel);
            EditorGUILayout.PropertyField(_captureFlags, FlagsLabel);

            if (_intervalSeconds.floatValue < MinIntervalSeconds)
                _intervalSeconds.floatValue = MinIntervalSeconds;

            _serializedConfig.ApplyModifiedProperties();

            EditorGUILayout.Space();
            DrawActions();
            DrawStatus();
        }

        private void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying)
                Repaint();
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            MemoryProfilerWindow window = GetWindow<MemoryProfilerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.Show();
        }

        private static void OpenOutputFolder()
        {
            MemoryProfilerConfigSo asset = Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);
            if (asset == null)
                return;

            string directory = MemoryProfilerRunner.ResolveStorageDirectory(asset);
            Directory.CreateDirectory(directory);
            EditorUtility.RevealInFinder(directory);
        }

        private static void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Capture Now"))
                    MemoryProfilerRunner.CaptureNow();

                if (GUILayout.Button("Open Captures Folder"))
                    OpenOutputFolder();
            }
        }

        private static void DrawStatus()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("State", MemoryProfilerRunner.IsActive
                ? "Running"
                : "Idle");

            string lastPath = MemoryProfilerRunner.LastSnapshotPath;
            EditorGUILayout.LabelField("Last Snapshot", string.IsNullOrEmpty(lastPath)
                ? "None"
                : Path.GetFileName(lastPath));
        }

        private void DrawMissingConfig()
        {
            EditorGUILayout.HelpBox("No config found in a Resources folder.", MessageType.Info);

            if (GUILayout.Button("Create Config Asset"))
                CreateConfig();
        }

        private void CreateConfig()
        {
            if (!AssetDatabase.IsValidFolder(ResourcesRoot))
                AssetDatabase.CreateFolder(AssetsFolder, ResourcesFolderName);

            if (!AssetDatabase.IsValidFolder(ConfigFolder))
                AssetDatabase.CreateFolder(ResourcesRoot, MemoryProfilerConfigSo.ResourceSubFolder);

            MemoryProfilerConfigSo asset = CreateInstance<MemoryProfilerConfigSo>();
            string path = $"{ConfigFolder}/{MemoryProfilerConfigSo.ConfigName}.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            RefreshConfigReference();
        }

        private void RefreshConfigReference()
        {
            MemoryProfilerConfigSo asset = Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);

            if (asset == null)
            {
                _serializedConfig = null;
                return;
            }

            _serializedConfig = new SerializedObject(asset);
            _isEnabled = _serializedConfig.FindProperty(MemoryProfilerConfigSo.IsEnabledField);
            _captureOnInterval = _serializedConfig.FindProperty(MemoryProfilerConfigSo.CaptureOnIntervalField);
            _intervalSeconds = _serializedConfig.FindProperty(MemoryProfilerConfigSo.IntervalSecondsField);
            _captureOnSceneLoad = _serializedConfig.FindProperty(MemoryProfilerConfigSo.CaptureOnSceneLoadField);
            _snapshotStoragePath = _serializedConfig.FindProperty(MemoryProfilerConfigSo.SnapshotStoragePathField);
            _fileNamePrefix = _serializedConfig.FindProperty(MemoryProfilerConfigSo.FileNamePrefixField);
            _captureFlags = _serializedConfig.FindProperty(MemoryProfilerConfigSo.CaptureFlagsField);
        }
    }
}
