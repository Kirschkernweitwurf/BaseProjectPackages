using System.Collections.Generic;
using Base.AttributesPackage.Editor.Core;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Validation
{
    /// <summary>
    /// Logs validation issues on ScriptableObject assets once when entering play mode, mirroring the
    /// scene validator. Editor only, since it enumerates assets through the AssetDatabase.
    /// </summary>
    [InitializeOnLoad]
    internal static class ScriptableObjectValidationRunner
    {
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

            foreach (ScriptableObject asset in ScriptableObjectAssets.LoadAll())
            {
                buffer.Clear();
                ReferenceValidationScanner.Collect(asset, buffer);

                foreach (ReferenceIssue issue in buffer)
                    CustomLogger.LogError(ValidationLog.Build(issue), issue.Owner);
            }
        }
    }
}