using System.IO;
using Base.EditorUiPackage;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.MemoryProfilerPackage.Editor
{
    /// <summary>
    /// Editor window to edit the runtime config and trigger manual captures.
    /// </summary>
    public sealed class MemoryProfilerWindow : EditorWindow
    {
        private const string ActionsHeader = "Actions";
        private const string AssetsFolder = "Assets";
        private const string AutomationHeader = "Automation";
        private const string CaptureLabel = "Capture Now";
        private const float CaptureButtonHeight = 26f;
        private const string CreateConfigLabel = "Create Config Asset";
        private const string Description = "Captures memory snapshots on a timer or on scene load, and "
            + "writes them where the Memory Profiler package can open them.";
        private const string IdleState = "Idle";
        private const string LastSnapshotLabel = "Last snapshot";
        private const string MissingConfigMessage = "No config found in a Resources folder.";
        private const string NoSnapshot = "None";
        private const string OpenFolderLabel = "Open Captures Folder";
        private const string OutputHeader = "Output";
        private const float OpenFolderWidth = 150f;
        private const string RunningState = "Running";
        private const string StateLabel = "State";
        private const string ConfigFolder = ResourcesRoot + "/" + MemoryProfilerConfigSo.ResourceSubFolder;
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Memory Profiler Automation";
        private const string ResourcesFolderName = "Resources";
        private const string ResourcesRoot = AssetsFolder + "/" + ResourcesFolderName;
        private const string WindowTitle = "Auto Memory Profiler";

        private static readonly GUIContent EnabledLabel = new("Enabled");
        private static readonly GUIContent FlagsLabel = new("Capture Flags");
        private static readonly GUIContent IntervalLabel = new("Interval (seconds)");
        private static readonly Vector2 MinWindowSize = new(360f, 260f);
        private static readonly GUIContent OnIntervalLabel = new("Capture On Interval");
        private static readonly GUIContent OnSceneLoadLabel = new("Capture On Scene Load");
        private static readonly GUIContent PrefixLabel = new("File Name Prefix");
        private static readonly GUIContent StoragePathLabel = new("Snapshot Storage Path");

        private readonly EditorWindowStyles _styles = new();

        private SerializedObject _serializedConfig;
        private SerializedProperty _isEnabled;
        private SerializedProperty _captureOnInterval;
        private SerializedProperty _intervalSeconds;
        private SerializedProperty _captureOnSceneLoad;
        private SerializedProperty _snapshotStoragePath;
        private SerializedProperty _fileNamePrefix;
        private SerializedProperty _captureFlags;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            RefreshConfigReference();
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            if (_serializedConfig == null
                || _serializedConfig.targetObject == null)
            {
                DrawMissingConfig();
                return;
            }

            _serializedConfig.Update();

            DrawAutomation();
            DrawOutput();

            _serializedConfig.ApplyModifiedProperties();

            EditorGUILayout.Space(EditorMetrics.SectionGap);
            DrawActions();
            DrawStatus();
        }

        private void OnDisable() => _styles.Dispose();

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

            window.minSize = MinWindowSize;
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

        private static string BuildStatus()
        {
            string state = MemoryProfilerRunner.IsActive
                ? RunningState
                : IdleState;

            string lastPath = MemoryProfilerRunner.LastSnapshotPath;
            string snapshot = string.IsNullOrEmpty(lastPath)
                ? NoSnapshot
                : Path.GetFileName(lastPath);

            return $"{StateLabel}: {state}    {LastSnapshotLabel}: {snapshot}";
        }

        private void DrawActions()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, ActionsHeader);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (EditorWindowChrome.PrimaryButton(_styles, CaptureLabel,
                        GUILayout.Height(CaptureButtonHeight)))
                    MemoryProfilerRunner.CaptureNow();

                GUILayout.Space(EditorMetrics.TightGap);

                if (EditorWindowChrome.SecondaryButton(_styles, OpenFolderLabel,
                        GUILayout.Height(CaptureButtonHeight), GUILayout.Width(OpenFolderWidth)))
                    OpenOutputFolder();
            }
        }

        private void DrawStatus() => EditorWindowChrome.DrawFooter(_styles, BuildStatus());

        private void DrawAutomation()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, AutomationHeader);
            EditorWindowChrome.BeginCard(_styles);

            EditorGUILayout.PropertyField(_isEnabled, EnabledLabel);

            using (new EditorGUI.DisabledScope(!_isEnabled.boolValue))
            {
                EditorGUILayout.PropertyField(_captureOnInterval, OnIntervalLabel);

                using (new EditorGUI.DisabledScope(!_captureOnInterval.boolValue))
                    EditorGUILayout.PropertyField(_intervalSeconds, IntervalLabel);

                EditorGUILayout.PropertyField(_captureOnSceneLoad, OnSceneLoadLabel);
            }

            EditorWindowChrome.EndCard();
        }

        private void DrawOutput()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, OutputHeader);
            EditorWindowChrome.BeginCard(_styles);

            EditorGUILayout.PropertyField(_snapshotStoragePath, StoragePathLabel);
            EditorGUILayout.PropertyField(_fileNamePrefix, PrefixLabel);
            EditorGUILayout.PropertyField(_captureFlags, FlagsLabel);

            EditorWindowChrome.EndCard();
        }

        private void DrawMissingConfig()
        {
            EditorGUILayout.HelpBox(MissingConfigMessage, MessageType.Info);
            EditorGUILayout.Space(EditorMetrics.ItemGap);

            if (EditorWindowChrome.PrimaryButton(_styles, CreateConfigLabel,
                    GUILayout.Height(CaptureButtonHeight)))
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