using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.TodoOverview.Model;
using Base.ToolsPackage.Editor.TodoOverview.Settings;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.TodoOverview
{
    /// <summary>
    /// The strip of controls across the top of the window: the search field, the owner, sort and group
    /// dropdowns, and the buttons that rescan, open the settings and take the packages folder in or
    /// out of the scan.
    /// <para>
    /// Every control here either edits the filter or asks for a rescan, and nothing here reads the
    /// items themselves. The window is told which of the two happened and decides what to redo, since
    /// a dropdown reports its choice when the menu closes rather than while the toolbar is drawn.
    /// </para>
    /// </summary>
    internal static class TodoToolbar
    {
        private const string AllOwnersLabel = "All";
        private const float DropdownWidth = 104f;
        private const string GroupFormat = "Group: {0}";
        private const string GroupTooltip = "Split the list into sections";
        private const string OwnerFormat = "Owner: {0}";
        private const string OwnerTooltip = "Show only the items of one person";
        private const string PackagesLabel = "Packages";
        private const string PackagesTooltip = "Scan the files under Packages as well";
        private const float PackagesWidth = 72f;
        private const string RefreshLabel = "Refresh";
        private const string RefreshTooltip = "Scan the project again";

        /// <summary>
        /// The name of the search field, so the window can tell whether the caret is in it before it
        /// reads a key press as a move through the list.
        /// </summary>
        internal const string SearchControl = "TodoOverviewSearch";

        private const string SearchHint = "Search";
        private const string SettingsLabel = "Settings";
        private const string SettingsTooltip = "Keywords, colors, patterns and ignored paths";
        private const string SortFormat = "Sort: {0}";
        private const string SortTooltip = "Order the items inside a section";
        private const float ToolbarButtonWidth = 64f;

        private static readonly GUIContent PackagesContent = new(PackagesLabel, PackagesTooltip);
        private static readonly GUIContent RefreshContent = new(RefreshLabel, RefreshTooltip);
        private static readonly GUIContent SettingsContent = new(SettingsLabel, SettingsTooltip);

        /// <summary>Draws the whole strip.</summary>
        /// <param name="filter">The filter the controls read and write.</param>
        /// <param name="owners">The people the scanned items are assigned to.</param>
        /// <param name="onFilterChanged">Called when a control changed what the list should show.</param>
        /// <param name="onRescan">Called when a control asked for the project to be read again.</param>
        /// <returns>
        /// The rectangle the search field was drawn in, which the window needs to tell a click meant
        /// for the field apart from one that should drop its focus.
        /// </returns>
        internal static Rect Draw(TodoFilter filter, IReadOnlyList<string> owners, Action onFilterChanged,
            Action onRescan)
        {
            Rect bar = GUILayoutUtility.GetRect(0f, TodoStyles.ToolbarHeight, GUILayout.ExpandWidth(true));

            TodoChrome.DrawBand(bar, TodoStyles.PanelColor());

            float y = bar.y + (bar.height - TodoStyles.ButtonHeight) * 0.5f;
            float x = bar.x + TodoStyles.RowInset;

            Rect searchRect = new(x, y, TodoStyles.SearchWidth, TodoStyles.ButtonHeight);
            DrawSearch(searchRect, filter, onFilterChanged);

            x += TodoStyles.SearchWidth + TodoStyles.Gap;
            x = DrawOwnerDropdown(x, y, filter, owners, onFilterChanged);
            x = DrawSortDropdown(x, y, filter, onFilterChanged);

            DrawGroupDropdown(x, y, filter, onFilterChanged);

            float right = bar.xMax - TodoStyles.RowInset;

            right = DrawRefreshButton(right, y, onRescan);
            right = DrawSettingsButton(right, y);

            DrawPackagesToggle(right, y, onRescan);

            TodoChrome.DrawSeparator(new Rect(bar.x, bar.yMax - TodoStyles.SeparatorThickness, bar.width,
                TodoStyles.SeparatorThickness));

            return searchRect;
        }

        // Drawn as a plain IMGUI text field on a fill of our own rather than with the editor's
        // search field, which drags a pile of editor only state into every pass of the toolbar.
        private static void DrawSearch(Rect rect, TodoFilter filter, Action onFilterChanged)
        {
            TodoChrome.DrawFill(rect, TodoStyles.FieldColor(), TodoStyles.ButtonRadius);

            GUI.SetNextControlName(SearchControl);

            string typed = GUI.TextField(rect, filter.Search, TodoStyles.Search);

            if (typed.Length == 0
                && GUI.GetNameOfFocusedControl() != SearchControl)
                GUI.Label(rect, SearchHint, TodoStyles.SearchHint);

            if (typed == filter.Search)
                return;

            filter.Search = typed;
            onFilterChanged();
        }

        private static float DrawOwnerDropdown(float x, float y, TodoFilter filter, IReadOnlyList<string> owners,
            Action onFilterChanged)
        {
            string current = filter.Owner == TodoFilter.AnyOwner
                ? AllOwnersLabel
                : filter.Owner;

            Rect rect = new(x, y, DropdownWidth, TodoStyles.ButtonHeight);
            GUIContent content = new(string.Format(OwnerFormat, current), OwnerTooltip);

            if (TodoChrome.DrawDropdown(rect, content))
                ShowOwnerMenu(rect, filter, owners, onFilterChanged);

            return rect.xMax + TodoStyles.Gap;
        }

        private static void ShowOwnerMenu(Rect rect, TodoFilter filter, IReadOnlyList<string> owners,
            Action onFilterChanged)
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent(AllOwnersLabel), filter.Owner == TodoFilter.AnyOwner,
                func: () => SetOwner(filter, TodoFilter.AnyOwner, onFilterChanged));

            foreach (string owner in owners)
            {
                string captured = owner;

                menu.AddItem(new GUIContent(captured), filter.Owner == captured,
                    func: () => SetOwner(filter, captured, onFilterChanged));
            }

            menu.DropDown(rect);
        }

        private static float DrawSortDropdown(float x, float y, TodoFilter filter, Action onFilterChanged)
        {
            Rect rect = new(x, y, DropdownWidth, TodoStyles.ButtonHeight);
            GUIContent content = new(string.Format(SortFormat, filter.Sort), SortTooltip);

            if (!TodoChrome.DrawDropdown(rect, content))
                return rect.xMax + TodoStyles.Gap;

            GenericMenu menu = new();

            foreach (ETodoSort value in (ETodoSort[])Enum.GetValues(typeof(ETodoSort)))
            {
                ETodoSort captured = value;

                menu.AddItem(new GUIContent(captured.ToString()), filter.Sort == captured,
                    func: () => SetSort(filter, captured, onFilterChanged));
            }

            menu.DropDown(rect);

            return rect.xMax + TodoStyles.Gap;
        }

        private static float DrawGroupDropdown(float x, float y, TodoFilter filter, Action onFilterChanged)
        {
            Rect rect = new(x, y, DropdownWidth, TodoStyles.ButtonHeight);
            GUIContent content = new(string.Format(GroupFormat, filter.Grouping), GroupTooltip);

            if (!TodoChrome.DrawDropdown(rect, content))
                return rect.xMax + TodoStyles.Gap;

            GenericMenu menu = new();

            foreach (ETodoGrouping value in (ETodoGrouping[])Enum.GetValues(typeof(ETodoGrouping)))
            {
                ETodoGrouping captured = value;

                menu.AddItem(new GUIContent(captured.ToString()), filter.Grouping == captured,
                    func: () => SetGrouping(filter, captured, onFilterChanged));
            }

            menu.DropDown(rect);

            return rect.xMax + TodoStyles.Gap;
        }

        private static float DrawRefreshButton(float right, float y, Action onRescan)
        {
            Rect rect = new(right - ToolbarButtonWidth, y, ToolbarButtonWidth, TodoStyles.ButtonHeight);

            if (TodoChrome.DrawButton(rect, RefreshContent, TodoStyles.ControlColor(), TodoStyles.Button,
                    TodoStyles.ButtonRadius))
                onRescan();

            return rect.x - TodoStyles.TightGap;
        }

        private static float DrawSettingsButton(float right, float y)
        {
            Rect rect = new(right - ToolbarButtonWidth, y, ToolbarButtonWidth, TodoStyles.ButtonHeight);

            if (TodoChrome.DrawButton(rect, SettingsContent, TodoStyles.ControlColor(), TodoStyles.Button,
                    TodoStyles.ButtonRadius))
                SettingsService.OpenProjectSettings(TodoSettingsProvider.Path);

            return rect.x - TodoStyles.TightGap;
        }

        private static void DrawPackagesToggle(float right, float y, Action onRescan)
        {
            TodoSettings settings = TodoSettings.instance;
            Rect rect = new(right - PackagesWidth, y, PackagesWidth, TodoStyles.ButtonHeight);

            Color fill = settings.IncludePackages
                ? TodoStyles.AccentColor()
                : TodoStyles.ControlColor();

            GUIStyle style = settings.IncludePackages
                ? TodoStyles.AccentLabel
                : TodoStyles.Button;

            if (!TodoChrome.DrawButton(rect, PackagesContent, fill, style, TodoStyles.ButtonRadius))
                return;

            settings.SetIncludePackages(!settings.IncludePackages);
            onRescan();
        }

        private static void SetOwner(TodoFilter filter, string owner, Action onFilterChanged)
        {
            filter.Owner = owner;
            onFilterChanged();
        }

        private static void SetSort(TodoFilter filter, ETodoSort sort, Action onFilterChanged)
        {
            filter.SetSort(sort);
            onFilterChanged();
        }

        private static void SetGrouping(TodoFilter filter, ETodoGrouping grouping, Action onFilterChanged)
        {
            filter.Grouping = grouping;
            onFilterChanged();
        }
    }
}