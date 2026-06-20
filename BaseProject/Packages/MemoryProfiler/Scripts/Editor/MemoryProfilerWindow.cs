using System.IO;
using UnityEditor;
using UnityEngine;

namespace Base.MemoryProfiler.Editor
{
    /// <summary>
    /// Editor window to edit the runtime config and trigger manual captures.
    /// </summary>
    public class MemoryProfilerWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Base Packages/Profiling/Memory Profiler Automation";
        private const float MinIntervalSeconds = 1f;
        private const string ResourcesFolder = "Assets/Resources/MemoryProfilerConfig";
        private const string WindowTitle = "Memory Profiler";

        private SerializedObject serializedConfig;

#region Unity Callbacks
        private void OnEnable() => RefreshConfigReference();

        private void OnGUI()
        {
            if (serializedConfig == null || serializedConfig.targetObject == null)
            {
                DrawMissingConfig();
                return;
            }

            serializedConfig.Update();

            SerializedProperty enabled = serializedConfig.FindProperty("isEnabled");
            SerializedProperty onInterval = serializedConfig.FindProperty("captureOnInterval");
            SerializedProperty interval = serializedConfig.FindProperty("intervalSeconds");
            SerializedProperty onSceneLoad = serializedConfig.FindProperty("captureOnSceneLoad");
            SerializedProperty folder = serializedConfig.FindProperty("outputFolder");
            SerializedProperty prefix = serializedConfig.FindProperty("fileNamePrefix");
            SerializedProperty flags = serializedConfig.FindProperty("captureFlags");

            EditorGUILayout.LabelField("Automation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enabled, new GUIContent("Enabled"));

            using (new EditorGUI.DisabledScope(!enabled.boolValue))
            {
                EditorGUILayout.PropertyField(onInterval, new GUIContent("Capture On Interval"));

                using (new EditorGUI.DisabledScope(!onInterval.boolValue))
                    EditorGUILayout.PropertyField(interval, new GUIContent("Interval (seconds)"));

                EditorGUILayout.PropertyField(onSceneLoad, new GUIContent("Capture On Scene Load"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(folder, new GUIContent("Folder (relative)"));
            EditorGUILayout.PropertyField(prefix, new GUIContent("File Name Prefix"));
            EditorGUILayout.PropertyField(flags, new GUIContent("Capture Flags"));

            if (interval.floatValue < MinIntervalSeconds)
                interval.floatValue = MinIntervalSeconds;

            serializedConfig.ApplyModifiedProperties();

            EditorGUILayout.Space();
            DrawActions();
            DrawStatus();
        }

        private void OnInspectorUpdate() => Repaint();
#endregion

        [MenuItem(MenuPath)]
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

            string root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            string directory = Path.Combine(root, asset.OutputFolder);
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
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.CreateFolder("Assets/Resources", "MemoryProfilerConfig");
            }

            MemoryProfilerConfigSo asset = CreateInstance<MemoryProfilerConfigSo>();
            string path = $"{ResourcesFolder}/{MemoryProfilerConfigSo.ConfigName}.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            RefreshConfigReference();
        }

        private void RefreshConfigReference()
        {
            MemoryProfilerConfigSo asset = Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);
            serializedConfig = asset != null
                ? new SerializedObject(asset)
                : null;
        }
    }
}