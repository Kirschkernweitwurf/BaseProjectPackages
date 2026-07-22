using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Base.MemoryProfiler.Editor
{
    /// <summary>
    /// Bakes the absolute snapshot folder into the config for development builds, so a build
    /// resolves a project-relative path like "./MemoryCaptures" to the editor project folder.
    /// The baked value is cleared after the build to keep the committed asset machine independent.
    /// </summary>
    internal sealed class MemoryProfilerBuildHook : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            MemoryProfilerConfigSo config = Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);
            if (config == null)
                return;

            WriteBakedPath(config, string.Empty);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!EditorUserBuildSettings.development)
                return;

            MemoryProfilerConfigSo config = Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);
            if (config == null)
                return;

            WriteBakedPath(config, MemoryProfilerRunner.ResolveStorageDirectory(config));
        }

        private static void WriteBakedPath(MemoryProfilerConfigSo config, string value)
        {
            SerializedObject serialized = new(config);
            serialized.FindProperty(MemoryProfilerConfigSo.BakedStoragePathField).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(config);
        }
    }
}