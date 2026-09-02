using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolsPackage.Editor.BaseToolsOverview
{
    /// <summary>
    /// Finds every settings page sitting under Base Tools, by building the same providers the
    /// settings window itself is built from and keeping the ones that land under this path.
    /// <para>
    /// A new page therefore shows up in the overview as soon as it compiles. Nothing is registered
    /// and no list has to be kept in step.
    /// </para>
    /// </summary>
    internal static class BaseToolsPageCatalog
    {
        /// <summary>The settings path of the overview itself, and the root every page hangs under.</summary>
        internal const string RootPath = "Project/Base Tools";

        private const string ChildPrefix = RootPath + "/";
        private const string KeywordSeparator = ", ";
        private const char PathSeparator = '/';
        private const string UnityAssemblyPrefix = "Unity";

        /// <summary>
        /// Collects the pages directly under Base Tools, sorted by name.
        /// </summary>
        /// <returns>One entry per page found, or an empty array when there is none.</returns>
        internal static BaseToolsPage[] Collect()
        {
            List<BaseToolsPage> pages = new();

            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<SettingsProviderAttribute>())
                AddFrom(method, pages);

            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<SettingsProviderGroupAttribute>())
                AddFrom(method, pages);

            BaseToolsPage[] sorted = pages.ToArray();

            Array.Sort(sorted,
                comparison: static (a, b) => string.Compare(a.Label, b.Label, StringComparison.Ordinal));

            return sorted;
        }

        // The path a page lives at is only known once its provider exists, so every factory has to
        // be called before it can be filtered. Unity's own are skipped: the settings window has
        // already built them, some of them are expensive, and none can end up under Base Tools.
        private static void AddFrom(MethodInfo method, ICollection<BaseToolsPage> pages)
        {
            if (!method.IsStatic
                || method.GetParameters().Length > 0
                || IsUnityOwned(method))
                return;

            object created = Invoke(method);

            if (created is SettingsProvider single)
            {
                Add(single, method, pages);
                return;
            }

            if (created is not IEnumerable<SettingsProvider> group)
                return;

            foreach (SettingsProvider provider in group)
                Add(provider, method, pages);
        }

        private static object Invoke(MethodInfo method)
        {
            try
            {
                return method.Invoke(null, null);
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning("Could not read the settings page created by "
                    + $"{method.DeclaringType?.Name}.{method.Name}: {exception.Message}", null);

                return null;
            }
        }

        private static void Add(SettingsProvider provider, MemberInfo origin, ICollection<BaseToolsPage> pages)
        {
            if (provider == null || string.IsNullOrEmpty(provider.settingsPath))
                return;

            string path = provider.settingsPath;

            if (!path.StartsWith(ChildPrefix, StringComparison.Ordinal))
                return;

            string name = path[ChildPrefix.Length..];

            // Only the direct children are listed. Anything deeper belongs to one of them and is
            // reached from there, so the overview stays a single flat list.
            if (name.Length == 0 || name.IndexOf(PathSeparator) >= 0)
                return;

            string label = string.IsNullOrEmpty(provider.label)
                ? name
                : provider.label;

            pages.Add(new BaseToolsPage(label, path, Summarize(provider, origin)));
        }

        // A provider carries no description, so the sentence comes from the attribute when the page
        // declares one. Without it the keywords are the only thing the page says about itself,
        // which still beats a bare name.
        private static string Summarize(SettingsProvider provider, MemberInfo origin)
        {
            BaseToolsPageAttribute described = origin.GetCustomAttribute<BaseToolsPageAttribute>()
                ?? origin.DeclaringType?.GetCustomAttribute<BaseToolsPageAttribute>();

            if (described != null && !string.IsNullOrEmpty(described.Description))
                return described.Description;

            return JoinKeywords(provider.keywords);
        }

        private static string JoinKeywords(IEnumerable<string> keywords)
        {
            if (keywords == null)
                return string.Empty;

            return string.Join(KeywordSeparator, keywords);
        }

        // Matched on the assembly name rather than a list of paths: everything Unity ships, engine,
        // editor and packages alike, starts with the same prefix. A project assembly that happens to
        // start with it too is left out of the overview, which is the harmless half of the trade.
        private static bool IsUnityOwned(MethodInfo method)
        {
            Type declaring = method.DeclaringType;

            if (declaring == null)
                return true;

            return declaring.Assembly.GetName().Name.StartsWith(UnityAssemblyPrefix, StringComparison.Ordinal);
        }
    }
}