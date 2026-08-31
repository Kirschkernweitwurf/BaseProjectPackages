using System;
using System.Collections.Generic;
using System.Text;
using Base.EditorUiPackage;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using EventBusBehaviour = Base.CorePackage.EventBus.EventBus;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// Live view of an event bus: every event type it currently holds handlers for, and who is
    /// subscribed to each. The table re-reads the bus on a timer while play mode runs, so a
    /// subscription appearing or going away shows up without touching the window.
    /// <para>
    /// The point of it is the leak. A handler that still runs on an object Unity already destroyed
    /// keeps that object's whole graph alive and fires on every publish, and nothing in the bus
    /// reports it because a delegate holds its target itself. Those rows are marked and can be
    /// filtered down to on their own.
    /// </para>
    /// <para>
    /// Read only on purpose. Clearing or unsubscribing from here would change the very state the
    /// window exists to report, and would hide the bug rather than fix it.
    /// </para>
    /// </summary>
    internal sealed class EventBusWindow : EditorWindow
    {
        private const string CollapseLabel = "Collapse";
        private const string CopyLabel = "Copy";
        private const string CopyRowItem = "Copy Row";
        private const string CopyTooltip = "Copy the whole table to the clipboard as tab separated text.";
        private const string CopyTypeItem = "Copy Event Type Name";
        private const string EditModeHint = "An event bus only holds handlers while the game runs. Enter play "
            + "mode and the subscriptions show up here as they happen.";
        private const string EditModeMessage = "Nothing to inspect yet";
        private const string EmptyHint = "Either nothing has subscribed yet, or every subscriber unsubscribed "
            + "again. An event type disappears from this list once its last handler is gone.";
        private const string EmptyMessage = "No subscribers on this bus";
        private const string EventHeader = "Event / Subscriber";
        private const string ExpandLabel = "Expand";
        private const string HandlerHeader = "Handler";
        private const string LeakCountFormat = "{0} of {1} leaked";
        private const string LeaksLabel = "Leaks only";
        private const string LeaksTooltip = "Show only subscriptions whose object was destroyed. Those "
            + "handlers still run on every publish and keep the destroyed object alive.";
        private const string MenuPath = "Tools/Base Packages/Runtime/Event Bus";
        private const string MultipleBusesMessage = "More than one event bus is loaded. Only one of them can "
            + "be the registered service, so anything publishing through the service locator reaches that "
            + "one alone.";
        private const string NoMatchHint = "Clear the search box or switch the leak filter off to see the rest "
            + "of the table.";
        private const string NoMatchMessage = "Nothing matches the filter";
        private const string PingItem = "Ping Subscriber";
        private const string PingLabel = "Ping";
        private const double RefreshInterval = 0.25d;
        private const string RefreshLabel = "Refresh";
        private const string ReportHeader = "Event\tSubscriber\tHandler\tTarget\tState";
        private const string ReportHandlerFormat = "\t{0}\t{1}\t{2}\t{3}";
        private const string ReportEventFormat = "{0}\t\t\t\t{1}";
        private const string SearchControlName = "EventBusSearch";
        private const string SelectItem = "Select Subscriber";
        private const string StateHeader = "State";
        private const string SummaryFormat = "{0} of {1} events, {2} handlers";
        private const string SummaryOkText = "No leaks";
        private const string TargetHeader = "Target";
        private const string WindowTitle = "Event Bus";

        private static readonly GUIContent DestroyedContent = new("Destroyed",
            "The object this handler runs on was destroyed but never unsubscribed. It still fires on "
            + "every publish and keeps the destroyed object alive.");
        private static readonly GUIContent LiveContent = new("Live",
            "The handler runs on a Unity object that is still alive.");
        private static readonly GUIContent PlainContent = new("Object",
            "The handler runs on a plain C# object. Unity does not manage its lifetime, so whether this "
            + "is a leak depends on who owns it.");
        private static readonly GUIContent StaticContent = new("Static",
            "The handler is a static method, so it has no instance that could outlive its subscription.");

        // The badge column is measured from these rather than from the rows, so its width cannot
        // depend on how many subscribers happen to be listed. Declared after the four it holds,
        // because static field initializers run in the order they are written.
        private static readonly GUIContent[] StateBadges =
        {
            DestroyedContent,
            LiveContent,
            PlainContent,
            StaticContent
        };

        private static readonly GUIContent CopyContent = new(CopyLabel, CopyTooltip);
        private static readonly GUIContent LeaksContent = new(LeaksLabel, LeaksTooltip);
        private static readonly GUIContent PingContent = new(PingLabel,
            "Select this subscriber and highlight it in the hierarchy.");

        private static readonly string NoBusMessage = $"No {typeof(EventBusBehaviour).Name} in the loaded scenes";

        // None of these are created where they are declared. A window Unity restores after a domain
        // reload can reach its first GUI pass without any field initializer having run, and then
        // every one of them is null. EnsureInitialized is called from the GUI pass for that reason.
        private List<EventBusBehaviour> _buses;
        private List<EventTypeEntry> _entries;
        private HashSet<Type> _expanded;
        private List<EventTypeEntry> _filtered;
        private List<EventBusRow> _rows;

        // Reused scratch list, so ordering the subscribers of every event does not allocate a list
        // per event on every rebuild.
        private List<HandlerEntry> _sorted;
        private EventBusColumns _columns;
        private EventBusStyles _styles;

        // Reused rather than allocated per row. A pill is a tinted rectangle with plain text on it,
        // and this carries the tooltip that explains the state on top of both.
        private GUIContent _stateTooltip;

        private float _badgeColumnWidth;
        private int _busIndex;
        private string[] _busLabels;
        private int _handlerCount;
        private int _hoveredIndex = -1;
        private bool _isInitialized;
        private bool _isPlaying;
        private int _leakCount;
        private bool _leaksOnly;
        private bool _needsFilter;
        private bool _needsRebuild;
        private double _nextRefreshTime;
        private bool? _pendingExpandAll;
        private Type _pendingToggle;
        private string _search;
        private Vector2 _scroll;
        private int _selectedIndex = -1;
        private EEventColumn _sortColumn;
        private ESortOrder _sortOrder;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            minSize = new Vector2(EventBusStyles.MinWindowWidth, EventBusStyles.MinWindowHeight);

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

            if (_buses.Count == 0)
            {
                DrawEmptyState(_isPlaying
                    ? NoBusMessage
                    : EditModeMessage, EditModeHint);

                return;
            }

            if (_buses.Count > 1)
                EditorGUILayout.HelpBox(MultipleBusesMessage, MessageType.Warning);

            // A bus sitting in an unplayed scene is empty for a different reason than one that is
            // running, and the hint that helps is different too.
            if (_entries.Count == 0)
            {
                DrawEmptyState(EmptyMessage, _isPlaying
                    ? EmptyHint
                    : EditModeHint);

                return;
            }

            if (_rows.Count == 0)
            {
                DrawEmptyState(NoMatchMessage, NoMatchHint);
                return;
            }

            MeasureBadgeColumn();
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

        /// <summary>Opens or focuses the window and reads the bus again.</summary>
        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            EventBusWindow window = GetWindow<EventBusWindow>(WindowTitle);

            // OnEnable only runs for a window that was closed, so an open one would otherwise keep
            // showing whatever it last read.
            window._needsRebuild = true;

            window.Show();
        }

        // Called from the first GUI pass as well, because a restored window can get there without
        // OnEnable having run and with every field still null.
        private void EnsureInitialized()
        {
            _busLabels ??= Array.Empty<string>();
            _buses ??= new List<EventBusBehaviour>();
            _columns ??= new EventBusColumns();
            _entries ??= new List<EventTypeEntry>();
            _expanded ??= new HashSet<Type>();
            _filtered ??= new List<EventTypeEntry>();
            _rows ??= new List<EventBusRow>();
            _search ??= string.Empty;
            _sorted ??= new List<HandlerEntry>();
            _stateTooltip ??= new GUIContent();
            _styles ??= new EventBusStyles();

            if (_isInitialized)
                return;

            // A value type cannot be asked whether it was ever set, so the ones whose zero value is
            // the wrong answer are restored under a flag the same reload clears.
            _isInitialized = true;
            _sortOrder = ESortOrder.Default;
        }

        private static GUIContent StateContent(EHandlerState state) => state switch
        {
            EHandlerState.Destroyed => DestroyedContent,
            EHandlerState.Live => LiveContent,
            EHandlerState.Static => StaticContent,
            _ => PlainContent
        };

        private static Color StateColor(EHandlerState state) => state switch
        {
            EHandlerState.Destroyed => EventBusStyles.DestroyedBadgeColor,
            EHandlerState.Live => EventBusStyles.LiveBadgeColor,
            _ => EventBusStyles.NeutralBadgeColor
        };

        private static string CountText(EventTypeEntry entry) => entry.HasLeaks
            ? string.Format(LeakCountFormat, entry.LeakCount, entry.Handlers.Count)
            : entry.Handlers.Count.ToString();

        private static string ReportRow(EventBusRow row)
        {
            if (row.IsHeader)
                return string.Format(ReportEventFormat, row.Event.TypeName, CountText(row.Event));

            HandlerEntry handler = row.Handler;

            return string.Format(ReportHandlerFormat, handler.SubscriberName, handler.MethodName,
                handler.TargetName, StateContent(handler.State).text);
        }

        private static void Ping(HandlerEntry handler)
        {
            if (handler == null || !handler.CanPing)
                return;

            Selection.activeObject = handler.Context;
            EditorGUIUtility.PingObject(handler.Context);
        }

        // A leaking row carries the console error icon, so it stays recognizable for anyone who
        // cannot separate the two row tints by color alone.
        private static void DrawLabelWithIcon(Rect cell, GUIContent content, GUIStyle style, Texture icon)
        {
            if (icon == null)
            {
                GUI.Label(cell, content, style);
                return;
            }

            float size = EventBusStyles.IconSize;
            Rect iconRect = new(cell.x, cell.y + (cell.height - size) * 0.5f, size, size);

            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);

            float offset = size + EventBusStyles.IconGap;
            Rect label = new(cell.x + offset, cell.y, Mathf.Max(0f, cell.width - offset), cell.height);

            GUI.Label(label, content, style);
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

            bool expansionChanged = ApplyExpansion();

            if (expansionChanged)
                RebuildRows();
        }

        // Reports whether anything moved, so the flattened rows are only rebuilt when they actually
        // have to be. Runs after the rebuild, so expanding everything expands what the table is
        // about to show rather than what it showed a moment ago.
        private bool ApplyExpansion()
        {
            bool changed = false;

            if (_pendingExpandAll.HasValue)
            {
                ExpandAll(_pendingExpandAll.Value);

                _pendingExpandAll = null;
                changed = true;
            }

            if (_pendingToggle == null)
                return changed;

            // Add reports whether the set changed, which turns the two calls into one toggle.
            if (!_expanded.Add(_pendingToggle))
                _expanded.Remove(_pendingToggle);

            _pendingToggle = null;

            return true;
        }

        private void ExpandAll(bool expand)
        {
            _expanded.Clear();

            if (!expand)
                return;

            foreach (EventTypeEntry entry in _entries)
                _expanded.Add(entry.EventType);
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

                case KeyCode.LeftArrow:
                    SetSelectedExpanded(false);
                    break;

                case KeyCode.RightArrow:
                    SetSelectedExpanded(true);
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    ActivateSelected();
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
            if (_rows.Count == 0)
                return;

            _selectedIndex = Mathf.Clamp(_selectedIndex + step, 0, _rows.Count - 1);

            Event.current.Use();
            Repaint();
        }

        // Left and right fold the selected event. On a subscriber row, left walks up to the event it
        // belongs to instead, which is how every tree in the editor behaves.
        private void SetSelectedExpanded(bool expand)
        {
            if (_selectedIndex < 0 || _selectedIndex >= _rows.Count)
                return;

            EventBusRow row = _rows[_selectedIndex];

            if (!row.IsHeader && !expand)
            {
                SelectHeaderOf(row.Event);
                Event.current.Use();

                return;
            }

            if (!row.IsHeader || _expanded.Contains(row.Event.EventType) == expand)
                return;

            _pendingToggle = row.Event.EventType;

            Event.current.Use();
            Repaint();
        }

        private void SelectHeaderOf(EventTypeEntry entry)
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                if (!_rows[i].IsHeader || _rows[i].Event != entry)
                    continue;

                _selectedIndex = i;
                Repaint();

                return;
            }
        }

        private void ActivateSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _rows.Count)
                return;

            EventBusRow row = _rows[_selectedIndex];

            if (row.IsHeader)
                _pendingToggle = row.Event.EventType;
            else
                Ping(row.Handler);

            Event.current.Use();
            Repaint();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                DrawBusPicker();

                EditorGUI.BeginChangeCheck();

                GUI.SetNextControlName(SearchControlName);
                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(EventBusStyles.SearchWidth));

                _leaksOnly = GUILayout.Toggle(_leaksOnly, LeaksContent, EditorStyles.toolbarButton);

                if (EditorGUI.EndChangeCheck())
                    _needsFilter = true;

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(ExpandLabel, EditorStyles.toolbarButton,
                        GUILayout.Width(EventBusStyles.ToolbarButtonWidth)))
                    _pendingExpandAll = true;

                if (GUILayout.Button(CollapseLabel, EditorStyles.toolbarButton,
                        GUILayout.Width(EventBusStyles.ToolbarButtonWidth)))
                    _pendingExpandAll = false;

                if (GUILayout.Button(CopyContent, EditorStyles.toolbarButton,
                        GUILayout.Width(EventBusStyles.ToolbarButtonWidth)))
                    CopyReport();

                if (GUILayout.Button(RefreshLabel, EditorStyles.toolbarButton,
                        GUILayout.Width(EventBusStyles.ToolbarButtonWidth)))
                    _needsRebuild = true;
            }
        }

        // Drawn only when there is a choice to make. The count can only change on a rebuild, which is
        // deferred to the layout event, so the control never appears or vanishes mid pass.
        private void DrawBusPicker()
        {
            if (_buses.Count < 2)
                return;

            EditorGUI.BeginChangeCheck();

            _busIndex = EditorGUILayout.Popup(_busIndex, _busLabels, EditorStyles.toolbarPopup,
                GUILayout.Width(EventBusStyles.BusPopupWidth));

            if (EditorGUI.EndChangeCheck())
                _needsRebuild = true;
        }

        // The counts and the health of the whole bus, so the answer to "is anything leaking" is
        // readable without expanding a single event.
        private void DrawSummaryBar()
        {
            Rect bar = GUILayoutUtility.GetRect(0f, EventBusStyles.SummaryHeight, GUILayout.ExpandWidth(true));
            Rect line = new(bar.x + EventBusStyles.OuterMargin, bar.y, bar.width - EventBusStyles.OuterMargin * 2f,
                bar.height);

            GUI.Label(line, string.Format(SummaryFormat, _filtered.Count, _entries.Count, _handlerCount),
                _styles.Summary);

            if (_entries.Count == 0)
                return;

            bool hasLeaks = _leakCount > 0;

            string text = hasLeaks
                ? string.Format(LeakCountFormat, _leakCount, _handlerCount)
                : SummaryOkText;

            float width = EditorRows.MeasureBadge(text, _styles.Badge, EventBusStyles.MinBadgeWidth);
            Rect pill = new(line.xMax - width, line.y, width, line.height);

            DrawPill(pill, text, hasLeaks
                ? EventBusStyles.SummaryProblemColor
                : EventBusStyles.SummaryOkColor);
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

        // One shared width, so the badges form a column instead of every row sizing its own and the
        // edges going ragged. Every state badge text is known up front, so only the count on an
        // event row has to be measured against the data.
        private void MeasureBadgeColumn()
        {
            _badgeColumnWidth = MeasureBadge(StateHeader);

            foreach (GUIContent badge in StateBadges)
                _badgeColumnWidth = Mathf.Max(_badgeColumnWidth, MeasureBadge(badge.text));

            foreach (EventTypeEntry entry in _filtered)
                _badgeColumnWidth = Mathf.Max(_badgeColumnWidth, MeasureBadge(CountText(entry)));
        }

        private float MeasureBadge(string text)
            => EditorRows.MeasureBadge(text, _styles.Badge, EventBusStyles.MinBadgeWidth);

        private void DrawTable()
        {
            GUILayout.Space(EventBusStyles.OuterMargin);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EventBusStyles.OuterMargin);

                using (EditorGUILayout.VerticalScope card = new(_styles.Card))
                {
                    DrawHeader();
                    DrawRows();

                    // Last, so the lines sit on top of the rows and a column can be grabbed at any
                    // row rather than only in the header. The group rectangle is only real once the
                    // layout pass has run.
                    if (Event.current.type != EventType.Layout)
                        _columns.DrawAndProcessDividers(TableArea(card.rect, _rows.Count));
                }

                GUILayout.Space(EventBusStyles.OuterMargin);
            }

            GUILayout.Space(EventBusStyles.OuterMargin);
        }

        // The card fills whatever height is left in the window, but a divider drawn down all of it
        // reads as a line through empty space. It stops at the last row instead, or at the bottom of
        // the card when the list is long enough to scroll.
        private static Rect TableArea(Rect card, int rowCount)
        {
            float content = EventBusStyles.CardPadding * 2f + EventBusStyles.HeaderHeight
                + rowCount * EventBusStyles.RowHeight;

            return new Rect(card.x, card.y, card.width, Mathf.Min(card.height, content));
        }

        private void DrawHeader()
        {
            Rect header = GUILayoutUtility.GetRect(0f, EventBusStyles.HeaderHeight, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(header, EventBusStyles.HeaderColor);

            _columns.Recalculate(header, _badgeColumnWidth);

            DrawSortableTitle(_columns.Subscriber(header, EventBusStyles.Indent), EventHeader,
                EEventColumn.Event, header);
            DrawSortableTitle(_columns.Method(header), HandlerHeader, EEventColumn.Handler, header);
            DrawSortableTitle(_columns.Target(header), TargetHeader, EEventColumn.Target, header);
            DrawSortableTitle(_columns.Badge(header), StateHeader, EEventColumn.State, header);

            DrawBottomSeparator(header);
        }

        // The arrow sits directly after the title rather than at the far edge of the cell, so it
        // reads as belonging to that word and does not drift away when a column is widened.
        private void DrawSortableTitle(Rect cell, string title, EEventColumn column, Rect header)
        {
            GUI.Label(cell, title, _styles.Header);

            if (_sortColumn == column)
            {
                float titleWidth = _styles.Header.CalcSize(new GUIContent(title)).x;
                Rect arrow = new(cell.x + titleWidth + EventBusStyles.HeaderArrowGap, cell.y,
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
        private void CycleSort(EEventColumn column)
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

            for (int i = 0; i < _rows.Count; i++)
            {
                if (DrawRow(i, _rows[i]))
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

        private bool DrawRow(int index, EventBusRow row)
        {
            Rect area = GUILayoutUtility.GetRect(0f, EventBusStyles.RowHeight, GUILayout.ExpandWidth(true));
            bool isHovered = area.Contains(Event.current.mousePosition);

            if (row.IsHeader)
                DrawEventRow(area, index, row.Event, isHovered);
            else
                DrawHandlerRow(area, index, row.Handler, isHovered);

            DrawBottomSeparator(area);
            HandleRowInput(area, index, row);

            return isHovered;
        }

        private void DrawEventRow(Rect area, int index, EventTypeEntry entry, bool isHovered)
        {
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(area, EventBusStyles.GroupColor);

                if (entry.HasLeaks)
                    EditorGUI.DrawRect(area, EventBusStyles.LeakRowColor);

                if (isHovered)
                    EditorGUI.DrawRect(area, EditorPalette.Hover);

                if (index == _selectedIndex)
                    EditorGUI.DrawRect(area, EditorPalette.SelectionFill);
            }

            Rect content = _columns.Content(area);

            // The arrow takes exactly one indent, which is what puts an event name at the same x as
            // the column header above it and the subscriber names below it.
            Rect arrow = new(content.x, area.y, EventBusStyles.Indent, area.height);
            bool isExpanded = _expanded.Contains(entry.EventType);

            if (EditorGUI.Foldout(arrow, isExpanded, GUIContent.none) != isExpanded)
                _pendingToggle = entry.EventType;

            Rect label = new(arrow.xMax, area.y, Mathf.Max(0f, content.xMax - arrow.xMax), area.height);

            DrawLabelWithIcon(label, new GUIContent(entry.TypeName, entry.NamespaceName), _styles.NameBold,
                entry.HasLeaks
                    ? EditorIcons.Error
                    : null);

            DrawPill(_columns.Badge(area), CountText(entry), entry.HasLeaks
                ? EventBusStyles.DestroyedBadgeColor
                : EventBusStyles.CountBadgeColor);
        }

        private void DrawHandlerRow(Rect area, int index, HandlerEntry handler, bool isHovered)
        {
            EditorRows.DrawRowBackground(area, index, isHovered, index == _selectedIndex);

            if (handler.IsLeak && Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(area, EventBusStyles.LeakRowColor);

            EditorRows.DrawIndentGuides(new Rect(area.x + EventBusStyles.RowInset, area.y, area.width, area.height),
                1, EventBusStyles.GuideColor);

            GUIContent state = StateContent(handler.State);
            Rect badgeCell = _columns.Badge(area);

            DrawLabelWithIcon(_columns.Subscriber(area, EventBusStyles.Indent),
                new GUIContent(handler.SubscriberName), _styles.Name, handler.IsLeak
                    ? EditorIcons.Error
                    : null);

            GUI.Label(_columns.Method(area), handler.MethodName, _styles.Detail);
            GUI.Label(_columns.Target(area), handler.TargetName, _styles.Detail);

            DrawPill(badgeCell, state.text, StateColor(handler.State));

            // Laid over the pill with no text of its own, purely so hovering it explains what the
            // state means. A pill is drawn, not a control, so there is nowhere else to put a tooltip.
            _stateTooltip.tooltip = state.tooltip;
            GUI.Label(badgeCell, _stateTooltip);

            DrawPingButton(_columns.Ping(area), handler);
        }

        // The fill is drawn rather than left to the style's hover state, which never appeared: a
        // GUIStyle resolves hover through a background this button deliberately does not have at rest.
        private void DrawPingButton(Rect cell, HandlerEntry handler)
        {
            if (!handler.CanPing)
                return;

            Rect button = PillRect(cell);
            bool isHovered = button.Contains(Event.current.mousePosition);

            DrawPillBackground(button, isHovered
                ? EventBusStyles.PingHoverColor
                : EventBusStyles.PingRestColor);

            if (GUI.Button(button, PingContent, isHovered
                    ? _styles.PingHot
                    : _styles.Ping))
                Ping(handler);
        }

        private void HandleRowInput(Rect area, int index, EventBusRow row)
        {
            Event current = Event.current;

            if (!area.Contains(current.mousePosition))
                return;

            if (current.type == EventType.ContextClick)
            {
                Select(index);
                ShowRowMenu(row);
                current.Use();

                return;
            }

            if (current.type != EventType.MouseDown || current.button != 0)
                return;

            Select(index);

            // A single click anywhere on an event row folds it, because the arrow alone is a small
            // target. The arrow is drawn first and takes the click when it was the thing hit, so by
            // the time this runs that case is already gone.
            if (row.IsHeader)
                _pendingToggle = row.Event.EventType;
            else if (current.clickCount == 2)
                Ping(row.Handler);

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

        private void ShowRowMenu(EventBusRow row)
        {
            GenericMenu menu = new();
            HandlerEntry handler = row.Handler;

            if (handler != null && handler.CanPing)
            {
                menu.AddItem(new GUIContent(PingItem), false, () => Ping(handler));
                menu.AddItem(new GUIContent(SelectItem), false, () => Selection.activeObject = handler.Context);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(PingItem));
                menu.AddDisabledItem(new GUIContent(SelectItem));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(CopyTypeItem), false,
                () => EditorGUIUtility.systemCopyBuffer = row.Event.EventType.FullName);
            menu.AddItem(new GUIContent(CopyRowItem), false,
                () => EditorGUIUtility.systemCopyBuffer = ReportRow(row));

            menu.ShowAsContext();
        }

        private void DrawEmptyState(string message, string hint)
        {
            Rect area = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // The reserved rectangle is only real once the layout pass has run, so there is nothing
            // meaningful to place anything in before that.
            if (Event.current.type != EventType.Repaint)
                return;

            Rect icon = EventBusStyles.EmptyIconRect(area);

            GUI.DrawTexture(icon, EditorIcons.Script, ScaleMode.ScaleToFit, true, 0f, EditorPalette.DimText, 0f, 0f);

            Rect title = new(area.x, icon.yMax + EventBusStyles.EmptyLineGap, area.width, EditorMetrics.RowHeight);

            GUI.Label(title, message, _styles.EmptyTitle);

            Rect hintArea = new(area.center.x - area.width * 0.25f, title.yMax, area.width * 0.5f,
                area.yMax - title.yMax);

            GUI.Label(hintArea, hint, _styles.EmptyHint);
        }

        private void CopyReport()
        {
            StringBuilder builder = new();

            builder.AppendLine(ReportHeader);

            foreach (EventBusRow row in _rows)
                builder.AppendLine(ReportRow(row));

            EditorGUIUtility.systemCopyBuffer = builder.ToString();
        }

        // Subscriptions come and go while the game runs and nothing in the editor raises an event for
        // it, so reading the bus again on a timer is the only way to stay current.
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
            RebuildBuses();
            RebuildEntries();
            ApplyFilter();
        }

        private void RebuildBuses()
        {
            _buses.Clear();
            _buses.AddRange(FindObjectsByType<EventBusBehaviour>(FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID));

            if (_busLabels.Length != _buses.Count)
                _busLabels = new string[_buses.Count];

            for (int i = 0; i < _buses.Count; i++)
                _busLabels[i] = SceneLabel.Describe(_buses[i]);

            _busIndex = Mathf.Clamp(_busIndex, 0, Mathf.Max(0, _buses.Count - 1));
        }

        private void RebuildEntries()
        {
            _entries.Clear();

            _handlerCount = 0;
            _leakCount = 0;

            if (_buses.Count == 0)
                return;

            EventBusBehaviour bus = _buses[_busIndex];

            if (bus == null)
                return;

            foreach (KeyValuePair<Type, Delegate> pair in bus.Handlers)
            {
                EventTypeEntry entry = new(pair.Key, pair.Value);

                _entries.Add(entry);

                _handlerCount += entry.Handlers.Count;
                _leakCount += entry.LeakCount;
            }

        }

        private void ApplyFilter()
        {
            _filtered.Clear();

            foreach (EventTypeEntry entry in _entries)
            {
                if (_leaksOnly && !entry.HasLeaks)
                    continue;

                if (entry.Matches(_search))
                    _filtered.Add(entry);
            }

            // The bus keeps a dictionary, so its order is arbitrary either way and the rows have to
            // be put in some order before they are drawn.
            _filtered.Sort(CompareEvents);

            RebuildRows();
        }

        private void RebuildRows()
        {
            _rows.Clear();

            foreach (EventTypeEntry entry in _filtered)
            {
                _rows.Add(new EventBusRow(entry, null));

                if (!_expanded.Contains(entry.EventType))
                    continue;

                _sorted.Clear();

                foreach (HandlerEntry handler in entry.Handlers)
                {
                    if (_leaksOnly && !handler.IsLeak)
                        continue;

                    _sorted.Add(handler);
                }

                // Sorted per event rather than across the whole table, because a subscriber only
                // means anything under the event it is subscribed to.
                _sorted.Sort(CompareHandlers);

                foreach (HandlerEntry handler in _sorted)
                    _rows.Add(new EventBusRow(entry, handler));
            }

            // The list the selection indexes into just changed under it, and a stale index would
            // either highlight the wrong row or point past the end.
            _selectedIndex = Mathf.Min(_selectedIndex, _rows.Count - 1);
        }

        // Handler and Target say nothing about an event, so those two columns leave the events in
        // name order and only reorder the subscribers underneath them.
        private int CompareEvents(EventTypeEntry first, EventTypeEntry second)
        {
            if (_sortOrder == ESortOrder.Default)
                return Ordinal(first.TypeName, second.TypeName);

            int result = _sortColumn switch
            {
                EEventColumn.State => first.Handlers.Count.CompareTo(second.Handlers.Count),
                _ => Ordinal(first.TypeName, second.TypeName)
            };

            if (result == 0)
                result = Ordinal(first.TypeName, second.TypeName);

            return Direct(result);
        }

        private int CompareHandlers(HandlerEntry first, HandlerEntry second)
        {
            if (_sortOrder == ESortOrder.Default)
                return 0;

            int result = _sortColumn switch
            {
                EEventColumn.Handler => Ordinal(first.MethodName, second.MethodName),
                EEventColumn.State => first.State.CompareTo(second.State),
                EEventColumn.Target => Ordinal(first.TargetName, second.TargetName),
                _ => Ordinal(first.SubscriberName, second.SubscriberName)
            };

            // Rows that tie fall back to the subscribing type, so a column with few distinct values
            // does not let its rows swap places between two reads a quarter of a second apart.
            if (result == 0)
                result = Ordinal(first.SubscriberName, second.SubscriberName);

            return Direct(result);
        }

        private int Direct(int result) => _sortOrder == ESortOrder.Descending
            ? -result
            : result;

        private static int Ordinal(string first, string second)
            => string.Compare(first, second, StringComparison.Ordinal);
    }
}