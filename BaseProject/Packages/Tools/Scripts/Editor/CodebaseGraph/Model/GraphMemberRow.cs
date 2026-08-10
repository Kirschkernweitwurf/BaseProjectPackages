namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>One line of a type node's member list, so a class on the graph reads like a class.</summary>
    public sealed class GraphMemberRow
    {
        /// <summary>Single letter standing for the kind of member.</summary>
        public string Glyph { get; }

        /// <summary>Member name with its type, as it would be read in source.</summary>
        public string Label { get; }

        /// <summary>Declared visibility, which sets the row color.</summary>
        public EAccessLevel Access { get; }

        /// <summary>True when the analyzer reported something that is still showing.</summary>
        public bool HasFinding { get; }

        /// <summary>True when something was reported and then reviewed and dismissed.</summary>
        public bool IsDismissed { get; }

        /// <summary>Creates a member row.</summary>
        /// <param name="glyph">Single letter standing for the kind.</param>
        /// <param name="label">Member name with its type.</param>
        /// <param name="access">Declared visibility.</param>
        /// <param name="hasFinding">Whether anything reported on it is still showing.</param>
        /// <param name="isDismissed">Whether what was reported has been dismissed.</param>
        public GraphMemberRow(string glyph,
            string label,
            EAccessLevel access,
            bool hasFinding,
            bool isDismissed)
        {
            Glyph = glyph;
            Label = label;
            Access = access;
            HasFinding = hasFinding;
            IsDismissed = isDismissed;
        }
    }
}
