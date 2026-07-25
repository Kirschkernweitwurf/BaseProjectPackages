#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.Editor.MenuOverview;
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

        private readonly MenuScriptLookup _scripts = new();

        /// <inheritdoc/>
        public IReadOnlyList<MenuItemEntry> Collect()
        {
            _scripts.Clear();
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
            MonoScript script = _scripts.Resolve(type);
            string assetPath = MenuScriptLookup.PathOf(script);
            EMenuItemOrigin origin = MenuItemOriginResolver.Classify(assetPath);

            return MenuItemEntry.Attributed(attribute.menuItem, type, method.Name, attribute.priority,
                attribute.validate, origin, script, assetPath);
        }
    }
}
#endif