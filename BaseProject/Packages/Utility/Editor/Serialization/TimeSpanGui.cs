using System;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Draws a <see cref="TimeSpan"/> tick count as a row of day, hour, minute, second and millisecond
    /// fields, preceded by a sign button. Units that are switched off keep whatever they held, so
    /// narrowing the row for readability never quietly throws part of the duration away.
    /// </summary>
    /// <remarks>
    /// The components come from the absolute duration and the sign is a control of its own, because a
    /// negative span reports every component as negative and typing a minus into one of five fields is
    /// not how anyone thinks about it.
    /// <para>
    /// The row is laid out from explicit rectangles, so the caller has to suspend the ambient indent
    /// with <see cref="NoIndentScope"/> or every level shifts the fields further right than the label.
    /// </para>
    /// </remarks>
    public static class TimeSpanGui
    {
        private const string DaySuffix = "D";
        private const int HourMinuteSecondCells = 3;
        private const int HoursPerDay = 24;
        private const string HourSuffix = "H";
        private const string MillisecondSeparator = ".";
        private const int MillisecondsPerSecond = 1000;
        private const string MillisecondSuffix = "Ms";
        private const int MinutesPerHour = 60;
        private const string MinuteSuffix = "M";
        private const string NegativeLabel = "-";
        private const string PositiveLabel = "+";
        private const int SecondsPerMinute = 60;
        private const string SecondSuffix = "S";
        private const float SignWidth = 20f;
        private const string TimeSeparator = ":";

        // One short of the true maximum, so the largest count still leaves room for the smaller units
        // underneath it. Composing past TimeSpan.MaxValue throws.
        private static readonly int MaxDays = TimeSpan.MaxValue.Days - 1;

        // The true hour ceiling does not fit in an int, and composing anywhere near it overflows
        // TimeSpan anyway, so the cap is the largest hour count that still leaves room underneath.
        private static readonly int MaxHours = (int)Math.Min(int.MaxValue - 1L,
            TimeSpan.MaxValue.Ticks / TimeSpan.TicksPerHour - 1L);

        /// <summary>Draws the duration row.</summary>
        /// <param name="rect">The single line the row occupies.</param>
        /// <param name="ticks">The serialized tick count of the duration.</param>
        /// <param name="showDays">True to add a day field in front of the hours.</param>
        /// <param name="showMilliseconds">True to add a millisecond field after the seconds.</param>
        public static void Draw(Rect rect, SerializedProperty ticks, bool showDays, bool showMilliseconds)
        {
            // TimeSpan.MinValue has no positive counterpart, so Duration would throw on it. One tick
            // off the end costs nothing and keeps every value in the row representable.
            long safe = Math.Max(ticks.longValue, -TimeSpan.MaxValue.Ticks);

            TimeSpan value = new TimeSpan(safe).Duration();
            bool isNegative = safe < 0L;

            Rect sign = new(rect.x, rect.y, SignWidth, rect.height);

            if (GUI.Button(sign, isNegative
                    ? NegativeLabel
                    : PositiveLabel, EditorStyles.miniButton))
                isNegative = !isNegative;

            Rect row = new(sign.xMax + TimeUnitField.Gap, rect.y, rect.xMax - sign.xMax - TimeUnitField.Gap,
                rect.height);

            int cells = HourMinuteSecondCells;

            if (showDays)
                cells++;

            if (showMilliseconds)
                cells++;

            float cell = TimeUnitField.CellWidth(row, cells, 0f);
            float x = row.x;

            int days = 0;

            if (showDays)
            {
                days = TimeUnitField.Draw(TimeUnitField.Slice(row, ref x, cell), value.Days, DaySuffix, 0, MaxDays);

                TimeUnitField.DrawSeparator(TimeUnitField.Slice(row, ref x, TimeUnitField.SeparatorWidth),
                    TimeSeparator);
            }

            // With the day field off, the days fold into the hours instead of being dropped. TimeSpan
            // reports Hours as the remainder inside a day, so reading it alone loses everything above.
            int shownHours = showDays
                ? value.Hours
                : (int)Math.Min(value.TotalHours, MaxHours);

            int maxHours = showDays
                ? HoursPerDay - 1
                : MaxHours;

            int hours = TimeUnitField.Draw(TimeUnitField.Slice(row, ref x, cell), shownHours, HourSuffix,
                0, maxHours);

            TimeUnitField.DrawSeparator(TimeUnitField.Slice(row, ref x, TimeUnitField.SeparatorWidth),
                TimeSeparator);

            int minutes = TimeUnitField.Draw(TimeUnitField.Slice(row, ref x, cell), value.Minutes,
                MinuteSuffix, 0, MinutesPerHour - 1);

            TimeUnitField.DrawSeparator(TimeUnitField.Slice(row, ref x, TimeUnitField.SeparatorWidth),
                TimeSeparator);

            int seconds = TimeUnitField.Draw(TimeUnitField.Slice(row, ref x, cell), value.Seconds,
                SecondSuffix, 0, SecondsPerMinute - 1);

            int milliseconds = value.Milliseconds;

            if (showMilliseconds)
            {
                TimeUnitField.DrawSeparator(TimeUnitField.Slice(row, ref x, TimeUnitField.SeparatorWidth),
                    MillisecondSeparator);

                milliseconds = TimeUnitField.Draw(TimeUnitField.Slice(row, ref x, cell), value.Milliseconds,
                    MillisecondSuffix, 0, MillisecondsPerSecond - 1);
            }

            Write(ticks, Compose(days, hours, minutes, seconds, milliseconds, isNegative));
        }

        private static long Compose(int days, int hours, int minutes, int seconds, int milliseconds,
            bool isNegative)
        {
            long composed = new TimeSpan(days, hours, minutes, seconds, milliseconds).Ticks;

            return isNegative
                ? -composed
                : composed;
        }

        // Written only on an actual change, so an untouched inspector never marks its object dirty.
        private static void Write(SerializedProperty ticks, long value)
        {
            if (ticks.longValue != value)
                ticks.longValue = value;
        }
    }
}