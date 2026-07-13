using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Logs validation issues on ScriptableObject assets once when entering play mode, mirroring the
    /// scene validator. Editor only, since it enumerates assets through the AssetDatabase.
    /// </summary>
    [InitializeOnLoad]
    public static class ScriptableObjectValidationRunner
    {
        private const string AssetFilter = "t:ScriptableObject";

        static ScriptableObjectValidationRunner()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
                Validate();
        }

        private static void Validate()
        {
            List<ReferenceIssue> buffer = new();

            foreach (string guid in AssetDatabase.FindAssets(AssetFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null)
                    continue;

                buffer.Clear();
                ReferenceValidationScanner.Collect(asset, buffer);

                foreach (ReferenceIssue issue in buffer)
                    CustomLogger.LogError(ValidationLog.Build(issue), issue.Owner);
            }
        }
    }
}