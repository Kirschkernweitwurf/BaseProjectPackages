using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolPackage.Editor.MenuManagerWindows;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Collects the pages of the project settings and the preferences. They are the one corner of
    /// the editor with no menu item of its own, so without this source the palette cannot offer a
    /// way to reach them at all.
    /// <para>
    /// The path a page lives at is only known once its provider object exists, so every factory
    /// method has to be called to find out. Unity's own are skipped: they build their keyword lists
    /// by loading the settings asset and walking every serialized property on it, which is far too
    /// much work for an index pass, and Unity Search already reaches them through its own token.
    /// </para>
    /// </summary>
    internal sealed class SettingsPageCommandSource : ICommandSource
    {
        private const char PathSeparator = '/';
        private const string PreferencesRoot = "Preferences";
        private const string ProjectSettingsRoot = "Project Settings";
        private const string UnityAssemblyPrefix = "Unity";

        /// <inheritdoc/>
        public void Collect(List<CommandEntry> entries)
        {
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<SettingsProviderAttribute>())
                AddFrom(method, entries);

            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<SettingsProviderGroupAttribute>())
                AddFrom(method, entries);
        }

        // A group factory hands back several pages from one method, so both shapes land here.
        private static void AddFrom(MethodInfo method, List<CommandEntry> entries)
        {
            if (!method.IsStatic
                || method.GetParameters().Length > 0
                || IsUnityOwned(method))
                return;

            object created = Invoke(method);

            if (created is SettingsProvider single)
            {
                Add(single, method, entries);
                return;
            }

            if (created is not IEnumerable<SettingsProvider> group)
                return;

            foreach (SettingsProvider provider in group)
                Add(provider, method, entries);
        }

        // Matched on the assembly name rather than on a list of paths: everything Unity ships,
        // engine, editor and packages alike, starts with the same prefix. A project assembly that
        // happens to start with it too is left out, which is the harmless half of the trade.
        private static bool IsUnityOwned(MethodInfo method)
        {
            Type declaring = method.DeclaringType;

            if (declaring == null)
                return true;

            return declaring.Assembly.GetName().Name.StartsWith(UnityAssemblyPrefix, StringComparison.Ordinal);
        }

        // A factory is somebody else's code running during our index pass, so it is allowed to
        // fail without taking the rest of the scan with it.
        private static object Invoke(MethodInfo method)
        {
            try
            {
                return method.Invoke(null, null);
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning("Could not read the settings page created by "
                    + $"{method.DeclaringType.Name}.{method.Name}: {exception.Message}", null);

                return null;
            }
        }

        private static void Add(SettingsProvider provider, MethodInfo origin, List<CommandEntry> entries)
        {
            if (provider == null
                || string.IsNullOrEmpty(provider.settingsPath))
                return;

            bool isUserScope = provider.scope == SettingsScope.User;
            string path = DisplayPath(provider.settingsPath, isUserScope);

            if (path == null)
                return;

            Type owner = origin.DeclaringType;

            // Copied out of the provider so the entry that outlives this pass closes over two
            // small values rather than over the whole provider object.
            string settingsPath = provider.settingsPath;

            entries.Add(new CommandEntry(MenuEntryId.ForSettings(settingsPath), path, owner,
                ECommandKind.Settings, AssemblyOriginLookup.Classify(owner),
                execute: () => Open(settingsPath, isUserScope), provider.keywords));
        }

        // The root segment is replaced rather than kept: a page registers itself under "Project/",
        // but the window it opens is called Project Settings, and that is what somebody types.
        private static string DisplayPath(string settingsPath, bool isUserScope)
        {
            int separator = settingsPath.IndexOf(PathSeparator);

            // Nothing below the root means the root page itself, which has no page to open.
            if (separator < 0
                || separator == settingsPath.Length - 1)
                return null;

            string root = isUserScope
                ? PreferencesRoot
                : ProjectSettingsRoot;

            return root + PathSeparator + settingsPath[(separator + 1)..];
        }

        private static void Open(string settingsPath, bool isUserScope)
        {
            if (isUserScope)
                SettingsService.OpenUserPreferences(settingsPath);
            else
                SettingsService.OpenProjectSettings(settingsPath);
        }
    }
}