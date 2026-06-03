#if UNITY_EDITOR
using System;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Identification.Editor
{
    /// <summary>
    /// Editor utility to generate unique IDs for all
    /// ScriptableObjects that implement <see cref="IUniquelyIdentifiable"/>.
    /// </summary>
    public static class IdGenerator
    {
        [MenuItem("Tools/Base Packages/Identifier/Generate Unique IDs", priority = 2)]
        public static void Generate()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

            int updatedCount = 0;
            int totalCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (asset is not IUniquelyIdentifiable data)
                    continue;

                totalCount++;

                if (!string.IsNullOrEmpty(data.UniqueId) && Guid.TryParse(data.UniqueId, out _))
                    continue;

                data.RegenerateId();
                updatedCount++;
            }

            AssetDatabase.SaveAssets();

            string scriptableObjectText = totalCount == 1 ? "scriptable object" : "scriptable objects";
            CustomLogger.Log(updatedCount > 0
                ? $"Generated IDs for {updatedCount} of {totalCount} {scriptableObjectText}."
                : $"All {totalCount} {scriptableObjectText} already have valid IDs. No changes made.", null);
        }
    }
}
#endif