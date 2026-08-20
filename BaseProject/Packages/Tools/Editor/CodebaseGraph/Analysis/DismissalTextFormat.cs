using System;
using System.Collections.Generic;
using System.Text;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
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
        public const string DismissVerb = "dismiss";

        /// <summary>Verb that hides an entry and everything inside it.</summary>
        public const string DismissWithContentsVerb = "dismiss-tree";

        /// <summary>Verb that brings a previously dismissed entry back.</summary>
        public const string RestoreVerb = "restore";

        /// <summary>Verb that brings an entry back together with everything inside it.</summary>
        public const string RestoreWithContentsVerb = "restore-tree";

        private const char CommentMarker = '#';
        private const char LineBreak = '\n';
        private const char VerbSeparator = ' ';

        /// <summary>Writes the current dismissals as instruction lines.</summary>
        /// <returns>One line per dismissed entry.</returns>
        public static string Write()
        {
            StringBuilder builder = new();

            AppendAll(builder, DismissVerb, DismissalStore.DismissedAlone);
            AppendAll(builder, DismissWithContentsVerb, DismissalStore.DismissedWithContents);

            return builder.ToString();
        }

        /// <summary>Reads instruction lines and applies them.</summary>
        /// <param name="text">The lines to read.</param>
        /// <param name="applied">Receives how many lines changed something.</param>
        /// <param name="ignored">Receives how many lines could not be understood or changed nothing.</param>
        public static void Apply(string text, out int applied, out int ignored)
        {
            applied = 0;
            ignored = 0;

            if (string.IsNullOrEmpty(text))
                return;

            foreach (string raw in text.Split(LineBreak))
            {
                string line = raw.Trim();

                // Blank lines and comments keep the format readable, so they are simply passed over.
                if (line.Length == 0 || line[0] == CommentMarker)
                    continue;

                if (ApplyLine(line))
                    applied++;
                else
                    ignored++;
            }
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

        private static bool ApplyLine(string line)
        {
            int split = line.IndexOf(VerbSeparator);
            if (split <= 0)
                return false;

            string verb = line[..split];
            string id = line[(split + 1)..].Trim();

            if (!GraphIdentity.IsValid(id))
                return false;

            switch (verb)
            {
                case DismissVerb:
                    DismissalStore.Dismiss(id, false);
                    return true;

                case DismissWithContentsVerb:
                    DismissalStore.Dismiss(id, true);
                    return true;

                case RestoreVerb:
                    return DismissalStore.Restore(id);

                case RestoreWithContentsVerb:
                    return DismissalStore.RestoreWithContents(id) > 0;

                default:
                    return false;
            }
        }
    }
}