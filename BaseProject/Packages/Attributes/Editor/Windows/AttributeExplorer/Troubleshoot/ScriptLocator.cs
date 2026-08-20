using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>
    /// Finds the script asset that declares a type, so a finding can be opened in the code editor.
    /// Results are cached because the underlying asset search is expensive and the answer only changes
    /// on a domain reload.
    /// </summary>
    internal static class ScriptLocator
    {
        private const string ScriptFilter = "t:MonoScript";

        private static readonly Dictionary<Type, MonoScript> Scripts = new();

        /// <summary>Opens the script declaring the given type in the code editor.</summary>
        /// <param name="type">The type to open.</param>
        /// <returns>True when a matching script was found and opened.</returns>
        public static bool Open(Type type)
        {
            MonoScript script = Find(type);
            if (script == null)
                return false;

            AssetDatabase.OpenAsset(script);
            return true;
        }

        /// <summary>Selects and pings the script declaring the given type.</summary>
        /// <param name="type">The type to ping.</param>
        public static void Ping(Type type)
        {
            MonoScript script = Find(type);
            if (script == null)
                return;

            Selection.activeObject = script;
            EditorGUIUtility.PingObject(script);
        }

        private static MonoScript Find(Type type)
        {
            if (Scripts.TryGetValue(type, out MonoScript cached))
                return cached;

            MonoScript found = Search(type);
            Scripts[type] = found;
            return found;
        }

        // Searching by name narrows the candidates first, then the declared class decides, so a type
        // whose file name differs from the class name is still matched correctly.
        private static MonoScript Search(Type type)
        {
            foreach (string guid in AssetDatabase.FindAssets($"{type.Name} {ScriptFilter}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script != null && script.GetClass() == type)
                    return script;
            }

            return null;
        }
    }
}