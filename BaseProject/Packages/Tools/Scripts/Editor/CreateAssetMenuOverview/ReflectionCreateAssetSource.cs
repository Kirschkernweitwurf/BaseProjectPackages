#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CreateAssetMenuOverview
{
    /// <summary>
    /// Default source that scans every loaded assembly for types decorated with
    /// <see cref="CreateAssetMenuAttribute"/>, covering project, package and built-in
    /// Unity ScriptableObjects.
    /// </summary>
    public sealed class ReflectionCreateAssetSource : ICreateAssetSource
    {
        private const string PackagePrefix = "Packages/";
        private const string ProjectPrefix = "Assets/";

        private readonly Dictionary<Type, MonoScript> _scriptCache = new();

        /// <inheritdoc/>
        public IReadOnlyList<CreateAssetEntry> Collect()
        {
            _scriptCache.Clear();
            List<CreateAssetEntry> entries = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in GetLoadableTypes(assembly))
                    CollectFromType(type, entries);
            }

            return entries;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                List<Type> loadable = new();
                foreach (Type type in exception.Types)
                {
                    if (type != null)
                        loadable.Add(type);
                }

                return loadable;
            }
        }

        private static MonoScript FindScript(Type type)
        {
            string[] guids = AssetDatabase.FindAssets($"{type.Name} t:MonoScript");
            MonoScript fallback = null;

            foreach (string guid in guids)
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

        private static ECreateAssetOrigin ClassifyOrigin(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return ECreateAssetOrigin.BuiltIn;

            if (assetPath.StartsWith(PackagePrefix, StringComparison.Ordinal))
                return ECreateAssetOrigin.Package;

            if (assetPath.StartsWith(ProjectPrefix, StringComparison.Ordinal))
                return ECreateAssetOrigin.Project;

            return ECreateAssetOrigin.BuiltIn;
        }

        private void CollectFromType(Type type, List<CreateAssetEntry> entries)
        {
            if (type == null)
                return;

            CreateAssetMenuAttribute attribute;
            try
            {
                attribute = type.GetCustomAttribute<CreateAssetMenuAttribute>(false);
            }
            catch (TypeLoadException)
            {
                return; // A missing dependency makes the whole type unusable; skip it.
            }

            if (attribute == null)
                return;

            entries.Add(BuildEntry(type, attribute));
        }

        private CreateAssetEntry BuildEntry(Type type, CreateAssetMenuAttribute attribute)
        {
            MonoScript script = ResolveScript(type);
            string assetPath = script != null
                ? AssetDatabase.GetAssetPath(script)
                : null;

            ECreateAssetOrigin origin = ClassifyOrigin(assetPath);

            return new CreateAssetEntry(attribute.menuName, attribute.fileName, type, attribute.order,
                origin, script, assetPath);
        }

        private MonoScript ResolveScript(Type type)
        {
            if (_scriptCache.TryGetValue(type, out MonoScript cached))
                return cached;

            MonoScript resolved = FindScript(type);
            _scriptCache[type] = resolved;
            return resolved;
        }
    }
}
#endif
