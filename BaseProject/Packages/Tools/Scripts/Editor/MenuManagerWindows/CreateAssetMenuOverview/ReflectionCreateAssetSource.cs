#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.Editor.Shared;
using Base.UtilityPackage;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows.CreateAssetMenuOverview
{
    /// <summary>
    /// Default source that scans every loaded assembly for types decorated with
    /// <see cref="CreateAssetMenuAttribute"/>, covering project, package and built-in
    /// Unity ScriptableObjects.
    /// </summary>
    internal sealed class ReflectionCreateAssetSource : ICreateAssetSource
    {
        private readonly MenuScriptLookup _scripts = new();

        /// <inheritdoc/>
        public IReadOnlyList<CreateAssetEntry> Collect()
        {
            _scripts.Clear();
            List<CreateAssetEntry> entries = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in ReflectionUtility.GetLoadableTypes(assembly))
                    CollectFromType(type, entries);
            }

            return entries;
        }

        private void CollectFromType(Type type, List<CreateAssetEntry> entries)
        {
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
            EAssetOrigin origin = AssetOriginResolver.Classify(assetPath);

            return CreateAssetEntry.Attributed(attribute.menuName, attribute.fileName, type, attribute.order,
                origin, script, assetPath);
        }
    }
}
#endif