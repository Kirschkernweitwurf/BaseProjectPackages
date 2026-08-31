using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Finds the script asset that declares a type, so a window can open it or ping it in the
    /// project. Results are cached, because the search behind them walks the asset database and the
    /// answer only changes when scripts are added, renamed or deleted.
    /// </summary>
    /// <remarks>
    /// A miss is cached as well, so a type with no script of its own does not search the project
    /// again on every repaint. That also means a script added after the first lookup is not seen
    /// until <see cref="Clear"/> runs, which is what a tool should call when it rescans.
    /// </remarks>
    public static class ScriptAssetLookup
    {
        private const string ScriptFilter = "t:" + nameof(MonoScript);

        private static readonly Dictionary<Type, MonoScript> Cache = new();

        /// <summary>Returns the script that declares the type, or null when it cannot be found.</summary>
        /// <param name="type">The type to look up.</param>
        /// <returns>The declaring script, or null.</returns>
        public static MonoScript Resolve(Type type)
        {
            if (type == null)
                return null;

            if (Cache.TryGetValue(type, out MonoScript cached))
                return cached;

            MonoScript found = Search(type);
            Cache[type] = found;

            return found;
        }

        /// <summary>Returns the project relative path of a script, or null when there is none.</summary>
        /// <param name="script">The script to locate.</param>
        /// <returns>The asset path, or null.</returns>
        public static string PathOf(MonoScript script) => script == null
            ? null
            : AssetDatabase.GetAssetPath(script);

        /// <summary>Opens the script declaring the type in the code editor.</summary>
        /// <param name="type">The type to open.</param>
        /// <returns>True when a script was found and opened.</returns>
        public static bool Open(Type type)
        {
            MonoScript script = Resolve(type);

            if (script == null)
                return false;

            AssetDatabase.OpenAsset(script);

            return true;
        }

        /// <summary>Selects the script declaring the type and pings it in the project window.</summary>
        /// <param name="type">The type to ping.</param>
        /// <returns>True when a script was found and pinged.</returns>
        public static bool Ping(Type type)
        {
            MonoScript script = Resolve(type);

            if (script == null)
                return false;

            Selection.activeObject = script;
            EditorGUIUtility.PingObject(script);

            return true;
        }

        /// <summary>Drops every cached lookup, so the next resolve hits the asset database again.</summary>
        public static void Clear() => Cache.Clear();

        // Searching by name narrows the candidates first and the declared class decides, so a type
        // whose file is named after something else is still matched correctly.
        private static MonoScript Search(Type type)
        {
            string name = TypeNameUtility.TrimArity(type.Name);

            if (string.IsNullOrEmpty(name))
                return null;

            MonoScript fallback = null;

            foreach (string guid in AssetDatabase.FindAssets($"{name} {ScriptFilter}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script == null)
                    continue;

                if (script.GetClass() == type)
                    return script;

                // GetClass answers for the one type named after its file and for nothing else, so it
                // returns null for a generic type and for every extra type sharing a file. A file
                // named exactly after the type is the best guess left. The name search itself is
                // loose, which is why the exact name is required: looking for Pool would otherwise
                // settle for PoolManager.
                if (fallback == null && script.name == name)
                    fallback = script;
            }

            return fallback;
        }
    }
}