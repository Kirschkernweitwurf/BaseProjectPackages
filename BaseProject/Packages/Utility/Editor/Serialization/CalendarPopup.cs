using System;
using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// The month grid behind the calendar button of a date row. Picking a day writes the date back and
    /// leaves the time of day alone, so the picker never silently resets a time that was typed by hand.
    /// </summary>
    /// <remarks>
    /// The popup outlives the draw call that opened it, so it holds the property rather than a value
    /// and applies its own edit. A target that went away while the popup was open closes it instead of
    /// writing into a serialized object that is no longer there.
    /// <para>
    /// Days are addressed as whole day numbers counted from <see cref="DateTime.MinValue"/> rather than
    /// as dates. A six week grid reaches a few days either side of its month, and at the first and the
    /// last month of the supported range those days have no <see cref="DateTime"/> to be built from.
    /// Counting instead of constructing lets those cells simply stay empty.
    /// </para>
    /// </remarks>
    internal sealed class CalendarPopup : PopupWindowContent
    {
        private const float CellInset = 2f;
        private const float CellSize = 28f;
        private const int ColumnCount = 7;
        private const int FirstDay = 1;
        private const long FirstDayNumber = 0L;
        private const int FirstMonth = 1;
        private const float FooterHeight = 22f;
        private const float HeaderHeight = 20f;
        private const float LineThickness = 1f;
        private const int MaxDaysInMonth = 31;
        private const int MonthsPerYear = 12;
        private const string MonthSuffix = "M";
        private const float NavigationWidth = 24f;
        private const string NextLabel = "\u203A";
        private const float OutlineThickness = 1f;
        private const float Padding = 8f;
        private const string PreviousLabel = "\u2039";
        private const int RowCount = 6;
        private const float RowSpacing = 4f;
        private const string TodayLabel = "Today";
        private const float WeekdayHeight = 18f;

        // Sunday first, which is the order the weekday letters below are written in. Starting the week
        // anywhere else means changing both or the columns stop matching their headings.
        private const DayOfWeek WeekStart = DayOfWeek.Sunday;

        private const string YearSuffix = "Y";

        private static GUIStyle AdjacentDayStyle => _adjacentDayStyle ??= DayStyleWith(AdjacentMonthText, false);

        private static GUIStyle DayStyle => _dayStyle ??= DayStyleWith(DayText, false);

        private static GUIStyle SelectedDayStyle => _selectedDayStyle ??= DayStyleWith(SelectionText, true);

        private static GUIStyle WeekdayStyle => _weekdayStyle ??= BuildWeekdayStyle();

        private static GUIStyle WeekendDayStyle => _weekendDayStyle ??= DayStyleWith(WeekendText, false);

        private static readonly string[] WeekdayLetters =
        {
            "S",
            "M",
            "T",
            "W",
            "T",
            "F",
            "S"
        };

        // Every number the grid can print, built once. The grid redraws constantly and would otherwise
        // format forty two throwaway strings per repaint.
        private static readonly string[] DayLabels = CreateDayLabels();

        private static readonly long LastDayNumber = DateTime.MaxValue.Ticks / TimeSpan.TicksPerDay;
        private static readonly int FirstMonthNumber = MonthNumber(DateTime.MinValue);
        private static readonly int LastMonthNumber = MonthNumber(DateTime.MaxValue);

        private static readonly Color AdjacentMonthText = Pick(new Color(0.42f, 0.42f, 0.46f),
            new Color(0.64f, 0.64f, 0.68f));
        private static readonly Color DayText = Pick(new Color(0.83f, 0.83f, 0.86f),
            new Color(0.16f, 0.16f, 0.18f));
        private static readonly Color HeaderLine = Pick(new Color(1f, 1f, 1f, 0.08f),
            new Color(0f, 0f, 0f, 0.12f));
        private static readonly Color HoverFill = Pick(new Color(1f, 1f, 1f, 0.09f),
            new Color(0f, 0f, 0f, 0.07f));
        private static readonly Color SelectionFill = new(0.23f, 0.55f, 0.90f);
        private static readonly Color SelectionText = Color.white;
        private static readonly Color TodayOutline = Pick(new Color(0.45f, 0.62f, 0.86f),
            new Color(0.28f, 0.48f, 0.78f));
        private static readonly Color WeekdayText = Pick(new Color(0.55f, 0.55f, 0.59f),
            new Color(0.45f, 0.45f, 0.50f));
        private static readonly Color WeekendText = Pick(new Color(0.70f, 0.66f, 0.66f),
            new Color(0.42f, 0.34f, 0.34f));

        private readonly SerializedProperty _ticks;

        private static GUIStyle _adjacentDayStyle;
        private static GUIStyle _dayStyle;
        private static GUIStyle _selectedDayStyle;
        private static GUIStyle _weekdayStyle;
        private static GUIStyle _weekendDayStyle;

        private DateTime _visibleMonth;

        private CalendarPopup(SerializedProperty ticks)
        {
            _ticks = ticks;
            _visibleMonth = FirstOfMonth(SerializableDateTime.ToDateTime(ticks.longValue));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Hover is painted by hand, and mouse move events are off by default on an editor window, so
        /// without this the pointer could cross the grid without a single cell ever lighting up.
        /// </remarks>
        public override void OnOpen() => editorWindow.wantsMouseMove = true;

        /// <inheritdoc/>
        public override Vector2 GetWindowSize() => new(ColumnCount * CellSize + Padding * 2f,
            HeaderHeight
            + RowSpacing
            + WeekdayHeight
            + RowCount * CellSize
            + RowSpacing
            + FooterHeight
            + Padding * 2f);

        /// <inheritdoc/>
        public override void OnGUI(Rect rect)
        {
            if (!IsTargetAlive())
            {
                editorWindow.Close();
                return;
            }

            // Hover is painted by hand, so the window has to be told to redraw as the pointer moves.
            // Without this a cell only lights up once something else happens to trigger a repaint.
            if (Event.current.type == EventType.MouseMove)
                editorWindow.Repaint();

            Rect content = new(rect.x + Padding, rect.y + Padding, rect.width - Padding * 2f,
                rect.height - Padding * 2f);

            float y = content.y;

            DrawHeader(new Rect(content.x, y, content.width, HeaderHeight));
            y += HeaderHeight + RowSpacing;

            DrawLine(new Rect(content.x, y - RowSpacing * 0.5f, content.width, LineThickness));
            DrawWeekdays(new Rect(content.x, y, content.width, WeekdayHeight));
            y += WeekdayHeight;

            Rect grid = new(content.x, y, content.width, RowCount * CellSize);

            DrawGrid(grid);

            DrawLine(new Rect(content.x, grid.yMax + RowSpacing * 0.5f, content.width, LineThickness));
            DrawFooter(new Rect(content.x, grid.yMax + RowSpacing, content.width, FooterHeight));
        }

        /// <summary>Opens the calendar under the given control.</summary>
        /// <param name="anchor">The rectangle the popup drops down from.</param>
        /// <param name="ticks">The serialized tick count a picked day is written into.</param>
        internal static void Show(Rect anchor, SerializedProperty ticks)
            => PopupWindow.Show(anchor, new CalendarPopup(ticks));

        private static string[] CreateDayLabels()
        {
            string[] labels = new string[MaxDaysInMonth];

            for (int day = 0; day < labels.Length; day++)
                labels[day] = (day + FirstDay).ToString();

            return labels;
        }

        private static Color Pick(Color pro, Color personal) => EditorGUIUtility.isProSkin
            ? pro
            : personal;

        // Every state pinned to one color. A label inherits hover and focus colors from the skin, which
        // would light half the grid up white the moment the pointer crossed it.
        private static GUIStyle DayStyleWith(Color color, bool isBold)
        {
            GUIStyle style = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = isBold
                    ? FontStyle.Bold
                    : FontStyle.Normal
            };

            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;

            return style;
        }

        private static GUIStyle BuildWeekdayStyle()
        {
            GUIStyle style = new(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

            style.normal.textColor = WeekdayText;

            return style;
        }

        private static bool IsWeekend(DayOfWeek day) => day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;

        private static Rect Inset(Rect cell) => new(cell.x + CellInset, cell.y + CellInset,
            cell.width - CellInset * 2f, cell.height - CellInset * 2f);

        private static bool IsHovered(Rect cell) => cell.Contains(Event.current.mousePosition);

        private static DateTime FirstOfMonth(DateTime value) => new(value.Year, value.Month, FirstDay);

        // Whole months counted from year one, so a step can be clamped before it is turned back into a
        // date. DateTime.AddMonths throws rather than saturating once the result leaves the range.
        private static int MonthNumber(DateTime value) => value.Year * MonthsPerYear + value.Month - 1;

        private static long DayNumber(DateTime value) => value.Ticks / TimeSpan.TicksPerDay;

        private static DateTime FromDayNumber(long dayNumber) => new(dayNumber * TimeSpan.TicksPerDay);

        // The week start on or before the first of the month, so the grid always opens on a full week.
        // Can land before day zero, which is why it is a count and not a date.
        private static long FirstCellDay(DateTime month)
        {
            int offset = (int)month.DayOfWeek - (int)WeekStart;

            if (offset < 0)
                offset += ColumnCount;

            return DayNumber(month) - offset;
        }

        private static void DrawWeekdays(Rect rect)
        {
            for (int column = 0; column < ColumnCount; column++)
            {
                Rect cell = new(rect.x + column * CellSize, rect.y, CellSize, rect.height);

                GUI.Label(cell, WeekdayLetters[column], WeekdayStyle);
            }
        }

        private static void DrawLine(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, HeaderLine);
        }

        private static void DrawOutline(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, OutlineThickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - OutlineThickness, rect.width, OutlineThickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, OutlineThickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - OutlineThickness, rect.y, OutlineThickness, rect.height), color);
        }

        private static GUIStyle ResolveDayStyle(DateTime day, bool isSelected, bool isCurrentMonth)
        {
            if (isSelected)
                return SelectedDayStyle;

            if (!isCurrentMonth)
                return AdjacentDayStyle;

            return IsWeekend(day.DayOfWeek)
                ? WeekendDayStyle
                : DayStyle;
        }

        private bool IsTargetAlive() => _ticks != null
            && _ticks.serializedObject != null
            && _ticks.serializedObject.targetObject != null;

        private void DrawHeader(Rect rect)
        {
            float navigation = NavigationWidth * 2f;
            float cell = TimeUnitField.CellWidth(rect, 2, navigation);
            float x = rect.x;

            int year = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), _visibleMonth.Year,
                YearSuffix, DateTime.MinValue.Year, DateTime.MaxValue.Year);

            x += TimeUnitField.Gap;

            int month = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), _visibleMonth.Month,
                MonthSuffix, FirstMonth, MonthsPerYear);

            _visibleMonth = new DateTime(year, month, FirstDay);

            Rect previous = new(rect.xMax - navigation, rect.y, NavigationWidth, rect.height);
            Rect next = new(rect.xMax - NavigationWidth, rect.y, NavigationWidth, rect.height);

            if (GUI.Button(previous, PreviousLabel, EditorStyles.miniButtonLeft))
                Step(-1);

            if (GUI.Button(next, NextLabel, EditorStyles.miniButtonRight))
                Step(1);
        }

        private void DrawGrid(Rect rect)
        {
            long first = FirstCellDay(_visibleMonth);
            long selected = DayNumber(SerializableDateTime.ToDateTime(_ticks.longValue));
            long today = DayNumber(DateTime.Now);

            for (int index = 0; index < RowCount * ColumnCount; index++)
            {
                long dayNumber = first + index;

                // The grid overhangs its month at both ends. At the very first and the very last month
                // of the supported range that overhang has no date, so the cell is left empty.
                if (dayNumber < FirstDayNumber || dayNumber > LastDayNumber)
                    continue;

                Rect cell = new(rect.x + index % ColumnCount * CellSize,
                    rect.y + index / ColumnCount * CellSize, CellSize, CellSize);

                DrawDay(cell, dayNumber, selected, today);
            }
        }

        private void DrawDay(Rect cell, long dayNumber, long selected, long today)
        {
            DateTime day = FromDayNumber(dayNumber);

            bool isSelected = dayNumber == selected;
            bool isCurrentMonth = day.Month == _visibleMonth.Month;

            // Inset, so the fill reads as a chip sitting in the cell rather than as a block of color
            // running into the days either side of it.
            Rect chip = Inset(cell);

            if (Event.current.type == EventType.Repaint)
                DrawDayBackground(cell, chip, isSelected, dayNumber == today);

            if (GUI.Button(cell, DayLabels[day.Day - FirstDay],
                    ResolveDayStyle(day, isSelected, isCurrentMonth)))
                Pick(day);
        }

        private void DrawDayBackground(Rect cell, Rect chip, bool isSelected, bool isToday)
        {
            if (isSelected)
            {
                EditorGUI.DrawRect(chip, SelectionFill);
                return;
            }

            if (IsHovered(cell))
                EditorGUI.DrawRect(chip, HoverFill);

            if (isToday)
                DrawOutline(chip, TodayOutline);
        }

        private void DrawFooter(Rect rect)
        {
            if (GUI.Button(rect, TodayLabel, EditorStyles.miniButton))
                Pick(DateTime.Now.Date);
        }

        private void Step(int months)
        {
            int stepped = Math.Clamp(MonthNumber(_visibleMonth) + months, FirstMonthNumber, LastMonthNumber);

            _visibleMonth = new DateTime(stepped / MonthsPerYear, stepped % MonthsPerYear + FirstMonth, FirstDay);
        }

        private void Pick(DateTime day)
        {
            TimeSpan timeOfDay = SerializableDateTime.ToDateTime(_ticks.longValue).TimeOfDay;

            _ticks.longValue = day.Date.Ticks + timeOfDay.Ticks;
            _ticks.serializedObject.ApplyModifiedProperties();

            _visibleMonth = FirstOfMonth(day);

            editorWindow.Close();
        }
    }
}