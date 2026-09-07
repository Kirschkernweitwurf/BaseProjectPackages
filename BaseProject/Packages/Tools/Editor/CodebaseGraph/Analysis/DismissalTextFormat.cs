using System;
using System.Collections.Generic;
using System.Text;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// A plain line format for moving dismissals in and out of the window. The findings report writes
    /// the current state in this shape, an agent or a person edits it, and the window reads it straight
    /// back. Lines are instructions rather than a replacement of the whole file, so anything not
    /// mentioned is left exactly as it was.
    /// </summary>
    internal static class DismissalTextFormat
    {
        /// <summary>Verb that hides the findings on an entry itself.</summary>
        internal const string DismissVerb = "dismiss";

        /// <summary>Verb that hides an entry and everything inside it.</summary>
        internal const string DismissWithContentsVerb = "dismiss-tree";

        /// <summary>Verb that brings a previously dismissed entry back.</summary>
        internal const string RestoreVerb = "restore";

        /// <summary>Verb that brings an entry back together with everything inside it.</summary>
        internal const string RestoreWithContentsVerb = "restore-tree";

        private const char CommentMarker = '#';
        private const char LineBreak = '\n';
        private const char VerbSeparator = ' ';

        /// <summary>Writes the current dismissals as instruction lines.</summary>
        /// <returns>One line per dismissed entry.</returns>
        internal static string Write()
        {
            StringBuilder builder = new();

            AppendAll(builder, DismissVerb, DismissalStore.DismissedAlone);
            AppendAll(builder, DismissWithContentsVerb, DismissalStore.DismissedWithContents);

            return builder.ToString();
        }

        /// <summary>
        /// Reads instruction lines and applies them, reporting on each one.
        /// </summary>
        /// <param name="text">The lines to read.</param>
        /// <returns>One result per instruction line, in the order they were written.</returns>
        internal static IReadOnlyList<DismissalLineResult> Apply(string text)
        {
            List<DismissalLineResult> results = new();

            if (string.IsNullOrEmpty(text))
                return results;

            string[] lines = text.Split(LineBreak);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // Blank lines and comments keep the format readable, so they are simply passed over.
                // They are not reported either, or a commented block would read as dozens of failures.
                if (line.Length == 0 || line[0] == CommentMarker)
                    continue;

                results.Add(new DismissalLineResult(i + 1, line, ApplyLine(line)));
            }

            return results;
        }

        private static void AppendAll(StringBuilder builder, string verb, IReadOnlyCollection<string> ids)
        {
            List<string> sorted = new(ids);
            sorted.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string id in sorted)
            {
                builder.Append(verb);
                builder.Append(VerbSeparator);
                builder.Append(id);
                builder.Append(LineBreak);
            }
        }

        private static EDismissalOutcome ApplyLine(string line)
        {
            int split = line.IndexOf(VerbSeparator);

            if (split <= 0)
                return EDismissalOutcome.UnknownVerb;

            string verb = line[..split];
            string id = line[(split + 1)..].Trim();

            // The verb is read first, so a line with both a bad verb and a bad id is reported as the
            // bad verb. Fixing that one is what makes the rest of the line worth looking at.
            if (verb != DismissVerb
                && verb != DismissWithContentsVerb
                && verb != RestoreVerb
                && verb != RestoreWithContentsVerb)
                return EDismissalOutcome.UnknownVerb;

            if (!GraphIdentity.IsValid(id))
                return EDismissalOutcome.UnreadableId;

            switch (verb)
            {
                case DismissVerb:
                    return Outcome(DismissalStore.Dismiss(id, false), EDismissalOutcome.AlreadyDismissed);

                case DismissWithContentsVerb:
                    return Outcome(DismissalStore.Dismiss(id, true), EDismissalOutcome.AlreadyDismissed);

                case RestoreVerb:
                    return Outcome(DismissalStore.Restore(id), EDismissalOutcome.NotDismissed);

                default:
                    return Outcome(DismissalStore.RestoreWithContents(id) > 0, EDismissalOutcome.NotDismissed);
            }
        }

        private static EDismissalOutcome Outcome(bool changed, EDismissalOutcome unchanged) => changed
            ? EDismissalOutcome.Applied
            : unchanged;
    }
}