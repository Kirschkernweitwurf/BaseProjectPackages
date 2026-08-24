using System;
using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Draws a <see cref="DateTime"/> tick count as a date row and a time of day row, each one a set of
    /// number fields with their unit letter. The date row ends in a button opening
    /// <see cref="CalendarPopup"/>, the time row in a button setting the time of day to now.
    /// </summary>
    /// <remarks>
    /// The property is passed in rather than a value, so the calendar popup can write its pick after
    /// the draw call that opened it has long returned. Both rows edit the same tick count and each one
    /// leaves the half it does not own untouched, which is what lets them be drawn independently.
    /// <para>
    /// Rows are laid out from explicit rectangles, so the caller has to suspend the ambient indent with
    /// <see cref="NoIndentScope"/> or every level shifts the fields further right than the label.
    /// </para>
    /// </remarks>
    public static class DateTimeGui
    {
        private const int DateCellCount = 3;
        // A slash rather than a dash. The box border already groups each number with its unit letter,
        // but a dash is the one separator that can still be read as the sign of the number after it.
        private const string DateSeparator = "/";
        private const string DaySuffix = "D";
        private const int FirstDay = 1;
        private const int FirstMonth = 1;
        private const int HoursPerDay = 24;
        private const string HourSuffix = "H";
        private const string MillisecondSeparator = ".";
        private const int MillisecondsPerSecond = 1000;
        private const string MillisecondSuffix = "Ms";
        private const int MinutesPerHour = 60;
        private const string MinuteSuffix = "M";
        private const int MonthsPerYear = 12;
        private const string MonthSuffix = "M";
        private const string NowLabel = "Now";
        private const float NowWidth = 38f;
        private const string PickerLabel = "\u25BE";
        private const float PickerWidth = 22f;
        private const int SecondsPerMinute = 60;
        private const string SecondSuffix = "S";
        private const int TimeCellCount = 3;
        private const int TimeCellCountWithMilliseconds = 4;
        private const string TimeSeparator = ":";
        private const string YearSuffix = "Y";

        /// <summary>Draws the year, month and day row plus its calendar button.</summary>
        /// <param name="rect">The single line the row occupies.</param>
        /// <param name="ticks">The serialized tick count of the date and time.</param>
        public static void DrawDate(Rect rect, SerializedProperty ticks)
        {
            DateTime value = SerializableDateTime.ToDateTime(ticks.longValue);

            float cell = TimeUnitField.CellWidth(rect, DateCellCount, PickerWidth);
            float x = rect.x;

            // Several selected objects holding different dates show one of them, and only an edit is
            // written back. Writing unconditionally would stamp the first object's date onto the rest
            // on the very first repaint, before anyone touched anything.
            EditorGUI.showMixedValue = ticks.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            int year = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), value.Year, YearSuffix,
                DateTime.MinValue.Year, DateTime.MaxValue.Year);

            TimeUnitField.DrawSeparator(TimeUnitField.Slice(rect, ref x, TimeUnitField.SeparatorWidth),
                DateSeparator);

            int month = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), value.Month, MonthSuffix,
                FirstMonth, MonthsPerYear);

            TimeUnitField.DrawSeparator(TimeUnitField.Slice(rect, ref x, TimeUnitField.SeparatorWidth),
                DateSeparator);

            // Clamped against the month that is now selected, so switching from March to February pulls
            // the 31st back to the 28th instead of throwing on a date that does not exist.
            int day = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), value.Day, DaySuffix,
                FirstDay, DateTime.DaysInMonth(year, month));

            bool edited = EditorGUI.EndChangeCheck();

            EditorGUI.showMixedValue = false;

            // Outside the change check: the popup writes its own pick, so counting the click as an edit
            // here would first write the unchanged fields back over every selected object.
            Rect picker = new(x + TimeUnitField.Gap, rect.y, PickerWidth, rect.height);

            if (GUI.Button(picker, PickerLabel, EditorStyles.miniButton))
                CalendarPopup.Show(picker, ticks);

            if (edited)
                ticks.longValue = new DateTime(year, month, day).Ticks + value.TimeOfDay.Ticks;
        }

        /// <summary>Draws the time of day row plus its now button.</summary>
        /// <param name="rect">The single line the row occupies.</param>
        /// <param name="ticks">The serialized tick count of the date and time.</param>
        /// <param name="showMilliseconds">True to add a millisecond field after the seconds.</param>
        public static void DrawTime(Rect rect, SerializedProperty ticks, bool showMilliseconds)
        {
            DateTime value = SerializableDateTime.ToDateTime(ticks.longValue);

            int cells = showMilliseconds
                ? TimeCellCountWithMilliseconds
                : TimeCellCount;

            float cell = TimeUnitField.CellWidth(rect, cells, NowWidth);
            float x = rect.x;

            EditorGUI.showMixedValue = ticks.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            int hour = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), value.Hour, HourSuffix,
                0, HoursPerDay - 1);

            TimeUnitField.DrawSeparator(TimeUnitField.Slice(rect, ref x, TimeUnitField.SeparatorWidth),
                TimeSeparator);

            int minute = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), value.Minute,
                MinuteSuffix, 0, MinutesPerHour - 1);

            TimeUnitField.DrawSeparator(TimeUnitField.Slice(rect, ref x, TimeUnitField.SeparatorWidth),
                TimeSeparator);

            int second = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), value.Second,
                SecondSuffix, 0, SecondsPerMinute - 1);

            int millisecond = value.Millisecond;

            if (showMilliseconds)
            {
                TimeUnitField.DrawSeparator(TimeUnitField.Slice(rect, ref x, TimeUnitField.SeparatorWidth),
                    MillisecondSeparator);

                millisecond = TimeUnitField.Draw(TimeUnitField.Slice(rect, ref x, cell), value.Millisecond,
                    MillisecondSuffix, 0, MillisecondsPerSecond - 1);
            }

            bool edited = EditorGUI.EndChangeCheck();

            EditorGUI.showMixedValue = false;

            Rect now = new(x + TimeUnitField.Gap, rect.y, NowWidth, rect.height);

            // Sets the time of day and nothing else. This row does not own the date, and a button that
            // quietly moved it would make the date row above disagree with itself.
            if (GUI.Button(now, NowLabel, EditorStyles.miniButton))
            {
                ticks.longValue = value.Date.Ticks + DateTime.Now.TimeOfDay.Ticks;
                return;
            }

            if (edited)
                ticks.longValue = value.Date.Ticks + new TimeSpan(0, hour, minute, second, millisecond).Ticks;
        }
    }
}