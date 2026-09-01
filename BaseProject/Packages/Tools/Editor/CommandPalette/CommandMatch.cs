namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>One entry that survived the current filter, together with its rank.</summary>
    internal readonly struct CommandMatch
    {
        /// <summary>The matched entry.</summary>
        internal CommandEntry Entry { get; }

        /// <summary>Rank of the entry, higher is better.</summary>
        internal int Score { get; }

        /// <summary>Whether the entry is pinned to the top of the results.</summary>
        internal bool IsPinned { get; }

        /// <summary>Creates a match.</summary>
        /// <param name="entry">The matched entry.</param>
        /// <param name="score">Rank of the entry.</param>
        /// <param name="isPinned">Whether the entry is pinned.</param>
        public CommandMatch(CommandEntry entry, int score, bool isPinned)
        {
            Entry = entry;
            Score = score;
            IsPinned = isPinned;
        }
    }
}