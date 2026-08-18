using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Base.MemoryProfilerPackage.Editor
{
    /// <summary>
    /// Bakes the absolute snapshot folder into the config for development builds, so a build
    /// resolves a project-relative path like "./MemoryCaptures" to the editor project folder.
    /// The baked value is cleared after the build to keep the committed asset machine independent.
    /// </summary>
    internal sealed class MemoryProfilerBuildHook : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>Order of this hook among all build callbacks.</summary>
        public int callbackOrder => 0;

        /// <summary>Clears the baked path again, so the committed asset stays machine independent.</summary>
        /// <param name="report">Build report supplied by Unity.</param>
        public void OnPostprocessBuild(BuildReport report)
        {
            MemoryProfilerConfigSo config = LoadConfig();
            if (config == null)
                return;

            WriteBakedPath(config, string.Empty);
        }

        /// <summary>Bakes the resolved snapshot folder into the config before a development build.</summary>
        /// <param name="report">Build report supplied by Unity.</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!EditorUserBuildSettings.development)
                return;

            MemoryProfilerConfigSo config = LoadConfig();
            if (config == null)
                return;

            WriteBakedPath(config, MemoryProfilerRunner.ResolveStorageDirectory(config));
        }

        private static MemoryProfilerConfigSo LoadConfig()
            => Resources.Load<MemoryProfilerConfigSo>(MemoryProfilerConfigSo.ResourcePath);

        private static void WriteBakedPath(MemoryProfilerConfigSo config, string value)
        {
            SerializedObject serialized = new(config);
            serialized.FindProperty(MemoryProfilerConfigSo.BakedStoragePathField).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(config);
        }
    }
}