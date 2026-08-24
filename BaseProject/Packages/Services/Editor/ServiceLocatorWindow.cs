using System;
using System.Collections.Generic;
using System.Text;
using Base.EditorUiPackage;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ServicePackage.Editor
{
    /// <summary>
    /// Live view of the <see cref="ServiceLocator"/>: every type currently registered, the instance
    /// behind it, where that instance lives and whether it is still usable. The table re-reads the
    /// locator on a timer while play mode runs, so a service appearing or going away shows up
    /// without touching the window.
    /// <para>
    /// Read only on purpose. <see cref="ServiceLocator.TryGet{T}"/> logs and drops a destroyed entry
    /// the first time anyone asks for it, so a window that resolved or deregistered anything would
    /// change the very state it exists to report.
    /// </para>
    /// </summary>
    internal sealed class ServiceLocatorWindow : EditorWindow
    {
        private const string CopyLabel = "Copy";
        private const string CopyRowItem = "Copy Row";
        private const string CopyTooltip = "Copy the whole table to the clipboard as tab separated text.";
        private const string CopyTypeItem = "Copy Type Name";
        private const string EditModeMessage = "Nothing to inspect yet";
        private const string EditModeHint = "The service locator is cleared before every play mode run. "
            + "Enter play mode and the registrations show up here as they happen.";
        private const string EmptyHint = "Services register themselves in Awake. If this stays empty, no "
            + "GameServiceBehaviour reached its Awake, or every one of them lives in a scene that is not "
            + "loaded.";
        private const string EmptyMessage = "No services registered";
        private const string InstanceHeader = "Instance";
        private const string LocationHeader = "Location";
        private const string MenuPath = "Tools/Base Packages/Runtime/Service Locator";
        private const string NoMatchHint = "Clear the search box or switch the problem filter off to see the "
            + "rest of the table.";
        private const string NoMatchMessage = "Nothing matches the filter";
        private const string PingItem = "Ping";
        private const string PingLabel = "Ping";
        private const string ProblemsLabel = "Problems only";
        private const string ProblemsTooltip = "Show only entries whose instance was destroyed without "
            + "deregistering, or that are filed under a type they do not implement.";
        private const double RefreshInterval = 0.25d;
        private const string RefreshLabel = "Refresh";
        private const string ReportHeader = "Service\tInstance\tLocation\tState";
        private const string RowFormat = "{0}\t{1}\t{2}\t{3}";
        private const string SearchControlName = "ServiceLocatorSearch";
        private const string SelectItem = "Select";
        private const string ServiceHeader = "Service";
        private const string StaleMessage = "Play mode has ended, but these entries are still registered. The "
            + "locator's table is static, so it outlives the run, and with Domain Reload disabled it outlives "
            + "the whole editor session. Anything still listed either never deregistered, or is filed under a "
            + "type whose owner deregistered a different instance. It is cleared again when play mode starts.";
        private const string StateHeader = "State";
        private const string SummaryFormat = "{0} of {1} registered";
        private const string SummaryOkText = "All healthy";
        private const string SummaryProblemFormat = "{0} problem";
        private const string SummaryProblemsFormat = "{0} problems";
        private const string WindowTitle = "Service Locator";

        private static readonly GUIContent AliveContent = new("Alive",
            "The instance is usable and implements the type it is filed under.");
        private static readonly GUIContent DestroyedContent = new("Destroyed",
            "The instance was destroyed without deregistering. The next lookup logs an error and drops "
            + "the entry.");
        private static readonly GUIContent MismatchContent = new("Mismatch",
            "The instance does not implement the type it is filed under, so every lookup for that type "
            + "fails.");

        // The badge column is measured from these rather than from the rows, so its width cannot
        // depend on how many services happen to be registered. Declared after the three it holds,
        // because static field initializers run in the order they are written.
        private static readonly GUIContent[] StateBadges =
        {
            AliveContent,
            DestroyedContent,
            MismatchContent
        };

        private static readonly GUIContent CopyContent = new(CopyLabel, CopyTooltip);
        private static readonly GUIContent PingContent = new(PingLabel,
            "Select this object and highlight it in the hierarchy.");
        private static readonly GUIContent ProblemsContent = new(ProblemsLabel, ProblemsTooltip);

        // None of these are created where they are declared. A window Unity restores after a domain
        // reload can reach its first GUI pass without any field initializer having run, and then
        // every one of them is null. EnsureInitialized is called from the GUI pass for that reason.
        private List<ServiceRegistrationEntry> _entries;
        private List<ServiceRegistrationEntry> _filtered;
        private ServiceLocatorColumns _columns;
        private ServiceLocatorStyles _styles;

        // Reused rather than allocated per row. A pill is a tinted rectangle with plain text on it,
        // and this carries the tooltip that explains the state on top of both.
        private GUIContent _stateTooltip;

        private int _hoveredIndex = -1;
        private bool _isInitialized;
        private bool _isPlaying;
        private bool _needsFilter;
        private bool _needsRebuild;
        private double _nextRefreshTime;
        private int _problemCount;
        private bool _problemsOnly;
        private string _search;
        private Vector2 _scroll;
        private int _selectedIndex = -1;
        private EServiceColumn _sortColumn;
        private ESortOrder _sortOrder;
        private float _stateColumnWidth;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            minSize = new Vector2(ServiceLocatorStyles.MinWindowWidth, ServiceLocatorStyles.MinWindowHeight);

            // The row buttons only fill in under the mouse, and that state can only be drawn if the
            // window redraws while the mouse moves inside a row it is already on.
            wantsMouseMove = true;

            EditorApplication.update += PollWhilePlaying;
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;

            EnsureInitialized();

            _needsRebuild = true;
        }

        private void OnGUI()
        {
            EnsureInitialized();

            _styles.EnsureBuilt();

            if (Event.current.type == EventType.Layout)
                ApplyPendingWork();

            if (Event.current.type == EventType.MouseMove)
                Repaint();

            if (Event.current.type == EventType.MouseLeaveWindow)
                SetHovered(-1);

            HandleShortcuts();

            DrawToolbar();
            DrawSummaryBar();

            if (_entries.Count == 0)
            {
                DrawEmptyState(_isPlaying
                    ? EmptyMessage
                    : EditModeMessage, _isPlaying
                    ? EmptyHint
                    : EditModeHint);

                return;
            }

            // Leftovers from the last run are worth explaining rather than hiding: the window is
            // right, the table really does still hold them.
            if (!_isPlaying)
                EditorGUILayout.HelpBox(StaleMessage, MessageType.Info);

            if (_filtered.Count == 0)
            {
                DrawEmptyState(NoMatchMessage, NoMatchHint);
                return;
            }

            MeasureStateColumn();
            DrawTable();
        }

        private void OnDisable()
        {
            EditorApplication.update -= PollWhilePlaying;
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;

            // A window torn down before it ever drew has no styles to release, and a plain C# field
            // is a real null here rather than a destroyed Unity object.
            if (_styles != null)
                _styles.Dispose();
        }
#endregion

        /// <summary>Opens or focuses the window and reads the locator again.</summary>
        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            ServiceLocatorWindow window = GetWindow<ServiceLocatorWindow>(WindowTitle);

            // OnEnable only runs for a window that was closed, so an open one would otherwise keep
            // showing whatever it last read.
            window._needsRebuild = true;

            window.Show();
        }

        // Called from the first GUI pass as well, because a restored window can get there without
        // OnEnable having run and with every field still null.
        private void EnsureInitialized()
        {
            _columns ??= new ServiceLocatorColumns();
            _entries ??= new List<ServiceRegistrationEntry>();
            _filtered ??= new List<ServiceRegistrationEntry>();
            _search ??= string.Empty;
            _stateTooltip ??= new GUIContent();
            _styles ??= new ServiceLocatorStyles();

            if (_isInitialized)
                return;

            // A value type cannot be asked whether it was ever set, so the ones whose zero value is
            // the wrong answer are restored under a flag the same reload clears.
            _isInitialized = true;
            _sortOrder = ESortOrder.Default;
        }

        private static GUIContent StateContent(EServiceState state) => state switch
        {
            EServiceState.Destroyed => DestroyedContent,
            EServiceState.Mismatch => MismatchContent,
            _ => AliveContent
        };

        private static Color StateColor(EServiceState state) => state switch
        {
            EServiceState.Destroyed => ServiceLocatorStyles.DestroyedBadgeColor,
            EServiceState.Mismatch => ServiceLocatorStyles.MismatchBadgeColor,
            _ => ServiceLocatorStyles.AliveBadgeColor
        };

        private static Texture StateIcon(EServiceState state) => state switch
        {
            EServiceState.Destroyed => EditorIcons.Error,
            EServiceState.Mismatch => EditorIcons.Warning,
            _ => null
        };

        private static string ReportRow(ServiceRegistrationEntry entry) => string.Format(RowFormat, entry.TypeName,
            entry.InstanceTypeName, entry.Location, StateContent(entry.State).text);

        private static void Ping(ServiceRegistrationEntry entry)
        {
            if (!entry.CanPing)
                return;

            Selection.activeObject = entry.Context;
            EditorGUIUtility.PingObject(entry.Context);
        }

        private static void DrawBottomSeparator(Rect row)
            => EditorRows.DrawSeparator(new Rect(row.x, row.yMax - EditorMetrics.SeparatorThickness, row.width,
                EditorMetrics.SeparatorThickness));

        // Every change that adds or removes a row is applied on the layout event and never mid pass.
        // IMGUI matches the controls of a repaint against the last layout, so a row appearing halfway
        // through a pass is what produces the control count errors in the console.
        private void ApplyPendingWork()
        {
            // Read once per layout pass and used for the rest of it, so a play mode change landing
            // between the layout and the repaint cannot alter how many controls the pass draws.
            _isPlaying = EditorApplication.isPlaying;

            if (_needsRebuild)
                Rebuild();
            else if (_needsFilter)
                ApplyFilter();

            _needsRebuild = false;
            _needsFilter = false;
        }

        // Handled before the controls are drawn, so the window sees the key first. The arrows are
        // left alone while the search box has focus, because there they belong to the text cursor.
        private void HandleShortcuts()
        {
            if (Event.current.type != EventType.KeyDown)
                return;

            if (Event.current.keyCode == KeyCode.Escape)
            {
                ClearSearch();
                return;
            }

            if (GUI.GetNameOfFocusedControl() == SearchControlName)
                return;

            switch (Event.current.keyCode)
            {
                case KeyCode.DownArrow:
                    MoveSelection(1);
                    break;

                case KeyCode.UpArrow:
                    MoveSelection(-1);
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    PingSelected();
                    break;
            }
        }

        private void ClearSearch()
        {
            if (_search.Length == 0)
                return;

            _search = string.Empty;
            _needsFilter = true;

            GUI.FocusControl(null);
            Event.current.Use();
        }

        private void MoveSelection(int step)
        {
            if (_filtered.Count == 0)
                return;

            _selectedIndex = Mathf.Clamp(_selectedIndex + step, 0, _filtered.Count - 1);

            Event.current.Use();
            Repaint();
        }

        private void PingSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _filtered.Count)
                return;

            Ping(_filtered[_selectedIndex]);
            Event.current.Use();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();

                GUI.SetNextControlName(SearchControlName);
                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(ServiceLocatorStyles.SearchWidth));

                _problemsOnly = GUILayout.Toggle(_problemsOnly, ProblemsContent, EditorStyles.toolbarButton);

                if (EditorGUI.EndChangeCheck())
                    _needsFilter = true;

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(CopyContent, EditorStyles.toolbarButton,
                        GUILayout.Width(ServiceLocatorStyles.ToolbarButtonWidth)))
                    CopyReport();

                if (GUILayout.Button(RefreshLabel, EditorStyles.toolbarButton,
                        GUILayout.Width(ServiceLocatorStyles.ToolbarButtonWidth)))
                    _needsRebuild = true;
            }
        }

        // The counts and the health of the whole table, so the answer to "is anything wrong" is
        // readable without scanning every row.
        private void DrawSummaryBar()
        {
            Rect bar = GUILayoutUtility.GetRect(0f, ServiceLocatorStyles.SummaryHeight, GUILayout.ExpandWidth(true));
            Rect line = new(bar.x + ServiceLocatorStyles.OuterMargin, bar.y,
                bar.width - ServiceLocatorStyles.OuterMargin * 2f, bar.height);

            GUI.Label(line, string.Format(SummaryFormat, _filtered.Count, _entries.Count), _styles.Summary);

            if (_entries.Count == 0)
                return;

            bool hasProblems = _problemCount > 0;

            string text = hasProblems
                ? string.Format(_problemCount == 1
                    ? SummaryProblemFormat
                    : SummaryProblemsFormat, _problemCount)
                : SummaryOkText;

            float width = EditorRows.MeasureBadge(text, _styles.Badge, ServiceLocatorStyles.MinBadgeWidth);
            Rect pill = new(line.xMax - width, line.y, width, line.height);

            DrawPill(pill, text, hasProblems
                ? ServiceLocatorStyles.SummaryProblemColor
                : ServiceLocatorStyles.SummaryOkColor);
        }

        // One texture tinted through GUI.color rather than one per fill, so every pill in the window
        // costs a single rounded texture no matter how many states there are.
        private void DrawPill(Rect cell, string text, Color fill)
        {
            Rect pill = PillRect(cell);

            DrawPillBackground(pill, fill);
            GUI.Label(pill, text, _styles.Badge);
        }

        private void DrawPillBackground(Rect pill, Color fill)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color previous = GUI.color;

            GUI.color = fill;
            _styles.PillBackground.Draw(pill, false, false, false, false);
            GUI.color = previous;
        }

        private static Rect PillRect(Rect cell) => new(cell.x,
            cell.y + (cell.height - EditorMetrics.PillHeight) * 0.5f, cell.width, EditorMetrics.PillHeight);

        // The badge text comes from a closed set of states, so the column is measured from those
        // rather than from every row: its width cannot depend on how many rows there are.
        private void MeasureStateColumn()
        {
            _stateColumnWidth = MeasureBadge(StateHeader);

            foreach (GUIContent badge in StateBadges)
                _stateColumnWidth = Mathf.Max(_stateColumnWidth, MeasureBadge(badge.text));
        }

        private float MeasureBadge(string text)
            => EditorRows.MeasureBadge(text, _styles.Badge, ServiceLocatorStyles.MinBadgeWidth);

        private void DrawTable()
        {
            GUILayout.Space(ServiceLocatorStyles.OuterMargin);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(ServiceLocatorStyles.OuterMargin);

                using (EditorGUILayout.VerticalScope card = new(_styles.Card))
                {
                    DrawHeader();
                    DrawRows();

                    // Last, so the lines sit on top of the rows and a column can be grabbed at any
                    // row rather than only in the header. The group rectangle is only real once the
                    // layout pass has run.
                    if (Event.current.type != EventType.Layout)
                        _columns.DrawAndProcessDividers(TableArea(card.rect, _filtered.Count));
                }

                GUILayout.Space(ServiceLocatorStyles.OuterMargin);
            }

            GUILayout.Space(ServiceLocatorStyles.OuterMargin);
        }

        // The card fills whatever height is left in the window, but a divider drawn down all of it
        // reads as a line through empty space. It stops at the last row instead, or at the bottom of
        // the card when the list is long enough to scroll.
        private static Rect TableArea(Rect card, int rowCount)
        {
            float content = ServiceLocatorStyles.CardPadding * 2f + ServiceLocatorStyles.HeaderHeight
                + rowCount * ServiceLocatorStyles.RowHeight;

            return new Rect(card.x, card.y, card.width, Mathf.Min(card.height, content));
        }

        private void DrawHeader()
        {
            Rect header = GUILayoutUtility.GetRect(0f, ServiceLocatorStyles.HeaderHeight, GUILayout.ExpandWidth(true));

            _columns.Recalculate(header, _stateColumnWidth);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(header, ServiceLocatorStyles.HeaderColor);

            DrawSortableTitle(_columns.Service(header), ServiceHeader, EServiceColumn.Service, header);
            DrawSortableTitle(_columns.Instance(header), InstanceHeader, EServiceColumn.Instance, header);
            DrawSortableTitle(_columns.Location(header), LocationHeader, EServiceColumn.Location, header);
            DrawSortableTitle(_columns.State(header), StateHeader, EServiceColumn.State, header);

            DrawBottomSeparator(header);
        }

        // The arrow sits directly after the title rather than at the far edge of the cell, so it
        // reads as belonging to that word and does not drift away when a column is widened.
        private void DrawSortableTitle(Rect cell, string title, EServiceColumn column, Rect header)
        {
            GUI.Label(cell, title, _styles.Header);

            if (_sortColumn == column)
            {
                float titleWidth = _styles.Header.CalcSize(new GUIContent(title)).x;
                Rect arrow = new(cell.x + titleWidth + ServiceLocatorStyles.HeaderArrowGap, cell.y,
                    EditorMetrics.SortArrowWidth, cell.height);

                EditorRows.DrawSortArrow(arrow, _sortOrder, EditorPalette.Text);
            }

            Event current = Event.current;

            if (current.type != EventType.MouseDown
                || current.button != 0
                || !cell.Contains(current.mousePosition))
                return;

            // The few pixels around a divider belong to the drag, not to the title behind them.
            if (_columns.IsOverDivider(current.mousePosition, header))
                return;

            CycleSort(column);
            current.Use();
        }

        // First click sorts, second reverses, third hands the order back to the window. A different
        // column always starts that cycle over rather than inheriting the previous direction.
        private void CycleSort(EServiceColumn column)
        {
            if (_sortColumn != column)
            {
                _sortColumn = column;
                _sortOrder = ESortOrder.Ascending;
            }
            else
            {
                _sortOrder = _sortOrder switch
                {
                    ESortOrder.Ascending => ESortOrder.Descending,
                    ESortOrder.Descending => ESortOrder.Default,
                    _ => ESortOrder.Ascending
                };
            }

            _needsFilter = true;

            Repaint();
        }

        private void DrawRows()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            int hovered = -1;

            for (int i = 0; i < _filtered.Count; i++)
            {
                if (DrawRow(i, _filtered[i]))
                    hovered = i;
            }

            EditorGUILayout.EndScrollView();

            SetHovered(hovered);
        }

        // Repainting only when the highlighted row actually changes is what keeps the window from
        // repainting continuously for as long as the mouse merely rests somewhere over the list.
        private void SetHovered(int index)
        {
            if (_hoveredIndex == index)
                return;

            _hoveredIndex = index;

            Repaint();
        }

        private bool DrawRow(int index, ServiceRegistrationEntry entry)
        {
            Rect row = GUILayoutUtility.GetRect(0f, ServiceLocatorStyles.RowHeight, GUILayout.ExpandWidth(true));
            bool isHovered = row.Contains(Event.current.mousePosition);

            EditorRows.DrawRowBackground(row, index, isHovered, index == _selectedIndex);

            if (entry.IsProblem && Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(row, ServiceLocatorStyles.ProblemRowColor);

            GUIContent state = StateContent(entry.State);
            Rect stateCell = _columns.State(row);

            DrawName(_columns.Service(row), entry);
            GUI.Label(_columns.Instance(row), entry.InstanceTypeName, _styles.Detail);
            GUI.Label(_columns.Location(row), entry.Location, _styles.Detail);

            DrawPill(stateCell, state.text, StateColor(entry.State));

            // Laid over the pill with no text of its own, purely so hovering it explains what the
            // state means. A pill is drawn, not a control, so there is nowhere else to put a tooltip.
            _stateTooltip.tooltip = state.tooltip;
            GUI.Label(stateCell, _stateTooltip);

            DrawPingButton(_columns.Ping(row), entry);
            DrawBottomSeparator(row);

            HandleRowInput(row, index, entry);

            return isHovered;
        }

        // A problem row carries the matching console icon, so a red row is recognizable at a glance
        // and stays recognizable for anyone who cannot separate the two tints by color alone.
        private void DrawName(Rect cell, ServiceRegistrationEntry entry)
        {
            Texture icon = StateIcon(entry.State);

            if (icon == null)
            {
                GUI.Label(cell, new GUIContent(entry.TypeName, entry.NamespaceName), _styles.Name);
                return;
            }

            float size = ServiceLocatorStyles.IconSize;
            Rect iconRect = new(cell.x, cell.y + (cell.height - size) * 0.5f, size, size);

            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

            float offset = size + ServiceLocatorStyles.IconGap;
            Rect label = new(cell.x + offset, cell.y, Mathf.Max(0f, cell.width - offset), cell.height);

            GUI.Label(label, new GUIContent(entry.TypeName, entry.NamespaceName), _styles.Name);
        }

        private void HandleRowInput(Rect row, int index, ServiceRegistrationEntry entry)
        {
            Event current = Event.current;

            if (!row.Contains(current.mousePosition))
                return;

            if (current.type == EventType.ContextClick)
            {
                Select(index);
                ShowRowMenu(entry);
                current.Use();

                return;
            }

            if (current.type != EventType.MouseDown || current.button != 0)
                return;

            Select(index);

            if (current.clickCount == 2)
                Ping(entry);

            current.Use();
        }

        private void Select(int index)
        {
            _selectedIndex = index;

            // Moves keyboard focus off the search box, so the arrow keys start moving the selection
            // instead of the text cursor the moment a row is clicked.
            GUI.FocusControl(null);
            Repaint();
        }

        private void ShowRowMenu(ServiceRegistrationEntry entry)
        {
            GenericMenu menu = new();

            if (entry.CanPing)
            {
                menu.AddItem(new GUIContent(PingItem), false, () => Ping(entry));
                menu.AddItem(new GUIContent(SelectItem), false, () => Selection.activeObject = entry.Context);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(PingItem));
                menu.AddDisabledItem(new GUIContent(SelectItem));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(CopyTypeItem), false,
                () => EditorGUIUtility.systemCopyBuffer = entry.RegisteredType.FullName);
            menu.AddItem(new GUIContent(CopyRowItem), false,
                () => EditorGUIUtility.systemCopyBuffer = ReportRow(entry));

            menu.ShowAsContext();
        }

        // The fill is drawn rather than left to the style's hover state, which never appeared: a
        // GUIStyle resolves hover through a background this button deliberately does not have at rest.
        private void DrawPingButton(Rect cell, ServiceRegistrationEntry entry)
        {
            if (!entry.CanPing)
                return;

            Rect button = PillRect(cell);
            bool isHovered = button.Contains(Event.current.mousePosition);

            DrawPillBackground(button, isHovered
                ? ServiceLocatorStyles.PingHoverColor
                : ServiceLocatorStyles.PingRestColor);

            if (GUI.Button(button, PingContent, isHovered
                    ? _styles.PingHot
                    : _styles.Ping))
                Ping(entry);
        }

        private void DrawEmptyState(string message, string hint)
        {
            Rect area = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // The reserved rectangle is only real once the layout pass has run, so there is nothing
            // meaningful to place anything in before that.
            if (Event.current.type != EventType.Repaint)
                return;

            Rect icon = ServiceLocatorStyles.EmptyIconRect(area);

            GUI.DrawTexture(icon, EditorIcons.Script, ScaleMode.ScaleToFit, true, 0f, EditorPalette.DimText, 0f, 0f);

            Rect title = new(area.x, icon.yMax + ServiceLocatorStyles.EmptyLineGap, area.width,
                EditorMetrics.RowHeight);

            GUI.Label(title, message, _styles.EmptyTitle);

            Rect hintArea = new(area.center.x - area.width * 0.25f, title.yMax, area.width * 0.5f,
                area.yMax - title.yMax);

            GUI.Label(hintArea, hint, _styles.EmptyHint);
        }

        private void CopyReport()
        {
            StringBuilder builder = new();

            builder.AppendLine(ReportHeader);

            foreach (ServiceRegistrationEntry entry in _filtered)
                builder.AppendLine(ReportRow(entry));

            EditorGUIUtility.systemCopyBuffer = builder.ToString();
        }

        // Registrations come and go while the game runs and nothing in the editor raises an event for
        // it, so reading the locator again on a timer is the only way to stay current.
        private void PollWhilePlaying()
        {
            if (!EditorApplication.isPlaying)
                return;

            if (EditorApplication.timeSinceStartup < _nextRefreshTime)
                return;

            _nextRefreshTime = EditorApplication.timeSinceStartup + RefreshInterval;
            _needsRebuild = true;

            Repaint();
        }

        private void HandlePlayModeChanged(PlayModeStateChange change)
        {
            _needsRebuild = true;

            Repaint();
        }

        private void Rebuild()
        {
            _entries.Clear();
            _problemCount = 0;

            foreach (KeyValuePair<Type, IGameService> pair in ServiceLocator.Registrations)
            {
                ServiceRegistrationEntry entry = new(pair.Key, pair.Value);

                _entries.Add(entry);

                if (entry.IsProblem)
                    _problemCount++;
            }

            // No ordering here: the locator is a dictionary, so the rows have to be sorted before
            // they are drawn either way, and ApplyFilter does that with whatever column is active.
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            _filtered.Clear();

            foreach (ServiceRegistrationEntry entry in _entries)
            {
                if (_problemsOnly && !entry.IsProblem)
                    continue;

                if (entry.Matches(_search))
                    _filtered.Add(entry);
            }

            _filtered.Sort(Compare);

            // The list the selection indexes into just changed under it, and a stale index would
            // either highlight the wrong row or point past the end.
            _selectedIndex = Mathf.Min(_selectedIndex, _filtered.Count - 1);
        }

        private int Compare(ServiceRegistrationEntry first, ServiceRegistrationEntry second)
        {
            if (_sortOrder == ESortOrder.Default)
                return Ordinal(first.TypeName, second.TypeName);

            int result = _sortColumn switch
            {
                EServiceColumn.Instance => Ordinal(first.InstanceTypeName, second.InstanceTypeName),
                EServiceColumn.Location => Ordinal(first.Location, second.Location),
                EServiceColumn.State => first.State.CompareTo(second.State),
                _ => Ordinal(first.TypeName, second.TypeName)
            };

            // Rows that tie fall back to the name, so a column with few distinct values does not let
            // its rows swap places between two reads a quarter of a second apart.
            if (result == 0)
                result = Ordinal(first.TypeName, second.TypeName);

            return _sortOrder == ESortOrder.Descending
                ? -result
                : result;
        }

        private static int Ordinal(string first, string second)
            => string.Compare(first, second, StringComparison.Ordinal);
    }
}