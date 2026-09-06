using System.Collections.Generic;
using System.Text;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// Renders the table as tab separated text, so a leak can leave the window and land in a bug
    /// report or a message without anyone retyping it.
    /// <para>
    /// The layout mirrors what is on screen: an event on its own line, its subscribers indented under
    /// it, and the same state wording the pills use.
    /// </para>
    /// </summary>
    internal static class EventBusReport
    {
        private const string ReportEventFormat = "{0}\t\t\t\t{1}";
        private const string ReportHandlerFormat = "\t{0}\t{1}\t{2}\t{3}";
        private const string ReportHeader = "Event\tSubscriber\tHandler\tTarget\tState";

        /// <summary>Renders the whole table.</summary>
        /// <param name="rows">The rows currently listed, headers and subscribers alike.</param>
        /// <returns>The table as tab separated text, one row per line.</returns>
        internal static string Build(IReadOnlyList<EventBusRow> rows)
        {
            StringBuilder builder = new();

            builder.AppendLine(ReportHeader);

            foreach (EventBusRow row in rows)
                builder.AppendLine(Row(row));

            return builder.ToString();
        }

        /// <summary>Renders one row, which is either an event or one of its subscribers.</summary>
        /// <param name="row">The row to render.</param>
        /// <returns>The row as tab separated text.</returns>
        internal static string Row(EventBusRow row)
        {
            if (row.IsHeader)
                return string.Format(ReportEventFormat, row.Event.TypeName, EventBusBadges.CountText(row.Event));

            HandlerEntry handler = row.Handler;

            return string.Format(ReportHandlerFormat, handler.SubscriberName, handler.MethodName,
                handler.TargetName, EventBusBadges.StateContent(handler.State).text);
        }
    }
}