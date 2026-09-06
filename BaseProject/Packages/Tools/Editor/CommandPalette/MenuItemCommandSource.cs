using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolsPackage.Editor.MenuManagerModel;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolsPackage.Editor.CommandPalette
{
    /// <summary>
    /// Collects every static <see cref="MenuItem"/> known to the editor. Validation functions,
    /// component context menus and Unity's hidden internal entries are skipped because none of
    /// them can be invoked on their own.
    /// </summary>
    internal sealed class MenuItemCommandSource : ICommandSource
    {
        private const string ContextPrefix = "CONTEXT/";
        private const string HiddenPrefix = "internal:";
        private const char HotkeySeparator = ' ';

        private static readonly char[] HotkeyMarkers =
        {
            '%',
            '#',
            '&',
            '_'
        };

        /// <inheritdoc/>
        public void Collect(List<CommandEntry> entries)
        {
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<MenuItem>())
            {
                Type owner = method.DeclaringType;

                if (owner == null)
                    continue;

                foreach (MenuItem attribute in method.GetCustomAttributes<MenuItem>(false))
                    TryAdd(entries, owner, method, attribute);
            }
        }

        // Unity appends the hotkey to the menu string, for example "Tools/Rebuild %#r". It is not
        // part of the path and would break both the display and ExecuteMenuItem.
        private static string StripHotkey(string menuItem)
        {
            int separator = menuItem.LastIndexOf(HotkeySeparator);

            if (separator <= 0 || separator == menuItem.Length - 1)
                return menuItem;

            return Array.IndexOf(HotkeyMarkers, menuItem[separator + 1]) >= 0
                ? menuItem[..separator]
                : menuItem;
        }

        private static void Invoke(string path)
        {
            if (EditorApplication.ExecuteMenuItem(path))
                return;

            CustomLogger.LogWarning($"'{path}' is not available right now.", null);
        }

        private static void TryAdd(List<CommandEntry> entries, Type owner, MethodInfo method, MenuItem attribute)
        {
            if (attribute.validate || string.IsNullOrWhiteSpace(attribute.menuItem))
                return;

            string raw = attribute.menuItem.Trim();

            if (raw.StartsWith(ContextPrefix, StringComparison.Ordinal)
                || raw.StartsWith(HiddenPrefix, StringComparison.Ordinal))
                return;

            string path = StripHotkey(raw);

            if (path.Length == 0)
                return;

            entries.Add(new CommandEntry(MenuEntryId.ForMenuItem(owner, method.Name), path, owner,
                ECommandKind.MenuItem, AssemblyOriginLookup.Classify(owner), execute: () => Invoke(path)));
        }
    }
}