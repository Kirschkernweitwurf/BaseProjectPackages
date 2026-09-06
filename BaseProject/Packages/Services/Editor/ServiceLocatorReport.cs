using System.Collections.Generic;
using System.Text;

namespace Base.ServicesPackage.Editor
{
    /// <summary>
    /// Renders the table as tab separated text, so a destroyed registration can leave the window and
    /// land in a bug report or a message without anyone retyping it.
    /// <para>
    /// The columns mirror what is on screen, and the state reads with the same wording the pills use.
    /// </para>
    /// </summary>
    internal static class ServiceLocatorReport
    {
        private const string ReportHeader = "Service\tInstance\tLocation\tState";
        private const string RowFormat = "{0}\t{1}\t{2}\t{3}";

        /// <summary>Renders the whole table.</summary>
        /// <param name="entries">The registrations currently listed.</param>
        /// <returns>The table as tab separated text, one row per line.</returns>
        internal static string Build(IReadOnlyList<ServiceRegistrationEntry> entries)
        {
            StringBuilder builder = new();

            builder.AppendLine(ReportHeader);

            foreach (ServiceRegistrationEntry entry in entries)
                builder.AppendLine(Row(entry));

            return builder.ToString();
        }

        /// <summary>Renders one registration.</summary>
        /// <param name="entry">The registration to render.</param>
        /// <returns>The row as tab separated text.</returns>
        internal static string Row(ServiceRegistrationEntry entry) => string.Format(RowFormat, entry.TypeName,
            entry.InstanceTypeName, entry.Location, ServiceLocatorBadges.StateContent(entry.State).text);
    }
}