using System;
using Base.ToolPackage.Editor.MenuManagerWindows;
using Base.ToolPackage.Editor.Shared;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// One executable entry of the command palette. Built once per index pass and never changed
    /// afterwards, so scoring a keystroke never touches the editor or the asset database.
    /// </summary>
    internal sealed class CommandEntry
    {
        private const char PathSeparator = '/';

        private readonly Action _execute;

        /// <summary>Stable id used to store tags and usage, independent of the current path.</summary>
        public string Id { get; }

        /// <summary>Full menu path the entry is indexed by, root segment included.</summary>
        public string Path { get; }

        /// <summary>Lowercase copy of <see cref="Path"/>, cached because every keystroke reads it.</summary>
        public string LowerPath { get; }

        /// <summary>Index of the first character of the last path segment.</summary>
        public int LeafStart { get; }

        /// <summary>Type that declares the command, or null when it could not be resolved.</summary>
        public Type Owner { get; }

        /// <summary>Short name of <see cref="Owner"/>, shown as the secondary label.</summary>
        public string Detail { get; }

        /// <summary>What executing the entry does.</summary>
        public ECommandKind Kind { get; }

        /// <summary>Whether the entry comes from an attribute or from the menu manager.</summary>
        public EMenuDefinition Definition { get; }

        /// <summary>Where the declaring code lives.</summary>
        public EAssetOrigin Origin { get; }

        /// <summary>Creates an entry.</summary>
        /// <param name="id">Stable id used for tags and usage.</param>
        /// <param name="path">Full menu path, root segment included.</param>
        /// <param name="owner">Type that declares the command.</param>
        /// <param name="kind">What executing the entry does.</param>
        /// <param name="definition">Whether the entry is attributed or managed.</param>
        /// <param name="origin">Where the declaring code lives.</param>
        /// <param name="execute">The action the palette runs.</param>
        public CommandEntry(string id, string path, Type owner, ECommandKind kind, EMenuDefinition definition,
            EAssetOrigin origin, Action execute)
        {
            Id = id;
            Path = path;
            LowerPath = path.ToLowerInvariant();

            int separator = path.LastIndexOf(PathSeparator);
            LeafStart = separator >= 0
                ? separator + 1
                : 0;

            Owner = owner;
            Detail = owner != null
                ? owner.Name
                : string.Empty;

            Kind = kind;
            Definition = definition;
            Origin = origin;
            _execute = execute;
        }

        /// <summary>Runs the command.</summary>
        public void Execute() => _execute();
    }
}