#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Default source that scans every loaded assembly for static methods decorated with
    /// <see cref="MenuItem"/>, covering project, package and built-in Unity menu items.
    /// </summary>
    public sealed class ReflectionMenuItemSource : IMenuItemSource
    {
        private const BindingFlags MethodFlags = BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;
        private const string PackagePrefix = "Packages/";
        private const string ProjectPrefix = "Assets/";

        private readonly Dictionary<Type, MonoScript> _scriptCache = new();

        /// <inheritdoc/>
        public IReadOnlyList<MenuItemEntry> Collect()
        {
            _scriptCache.Clear();
            List<MenuItemEntry> entries = new();

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

        private static EMenuItemOrigin ClassifyOrigin(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return EMenuItemOrigin.BuiltIn;

            if (assetPath.StartsWith(PackagePrefix, StringComparison.Ordinal))
                return EMenuItemOrigin.Package;

            if (assetPath.StartsWith(ProjectPrefix, StringComparison.Ordinal))
                return EMenuItemOrigin.Project;

            return EMenuItemOrigin.BuiltIn;
        }

        private void CollectFromType(Type type, List<MenuItemEntry> entries)
        {
            if (type == null)
                return;

            MethodInfo[] methods;
            try
            {
                methods = type.GetMethods(MethodFlags);
            }
            catch (TypeLoadException)
            {
                return; // A missing dependency makes the whole type unusable; skip it.
            }

            foreach (MethodInfo method in methods)
            {
                foreach (object raw in method.GetCustomAttributes(typeof(MenuItem), false))
                    entries.Add(BuildEntry(type, method, (MenuItem)raw));
            }
        }

        private MenuItemEntry BuildEntry(Type type, MethodInfo method, MenuItem attribute)
        {
            MonoScript script = ResolveScript(type);
            string assetPath = script != null
                ? AssetDatabase.GetAssetPath(script)
                : null;

            EMenuItemOrigin origin = ClassifyOrigin(assetPath);

            return new MenuItemEntry(attribute.menuItem, type, method.Name, attribute.priority,
                attribute.validate, origin, script, assetPath);
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