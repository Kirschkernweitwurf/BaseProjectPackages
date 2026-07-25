#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.Editor.MenuOverview;
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
        private readonly MenuScriptLookup _scripts = new();

        /// <inheritdoc/>
        public IReadOnlyList<CreateAssetEntry> Collect()
        {
            _scripts.Clear();
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
            MonoScript script = _scripts.Resolve(type);
            string assetPath = MenuScriptLookup.PathOf(script);
            ECreateAssetOrigin origin = CreateAssetOriginResolver.Classify(assetPath);

            return CreateAssetEntry.Attributed(attribute.menuName, attribute.fileName, type, attribute.order,
                origin, script, assetPath);
        }
    }
}
#endif