using System;
using System.Collections.Generic;
using UnityEditor;

namespace Base.ToolsPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Caches the <see cref="MonoScript"/> that declares a type. Every overview source shares
    /// the same lookup rules so a static and a dynamic entry of the same type resolve alike.
    /// </summary>
    internal sealed class MenuScriptLookup
    {
        private readonly Dictionary<Type, MonoScript> _cache = new();

        /// <summary>Returns the project relative path of a script, or null when there is none.</summary>
        internal static string PathOf(MonoScript script) => script == null
            ? null
            : AssetDatabase.GetAssetPath(script);

        /// <summary>Drops every cached lookup so the next resolve hits the asset database again.</summary>
        internal void Clear() => _cache.Clear();

        /// <summary>Returns the script that declares the type, or null when it cannot be found.</summary>
        internal MonoScript Resolve(Type type)
        {
            if (type == null)
                return null;

            if (_cache.TryGetValue(type, out MonoScript cached))
                return cached;

            MonoScript resolved = Find(type);
            _cache[type] = resolved;
            return resolved;
        }

        private static MonoScript Find(Type type)
        {
            MonoScript fallback = null;

            foreach (string guid in AssetDatabase.FindAssets($"{type.Name} t:{nameof(MonoScript)}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script == null)
                    continue;

                if (script.GetClass() == type)
                    return script;

                fallback ??= script; // File name matches but holds another type; keep as a hint.
            }

            return fallback;
        }
    }
}