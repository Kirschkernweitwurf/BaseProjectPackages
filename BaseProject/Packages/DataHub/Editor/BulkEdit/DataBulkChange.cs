namespace Base.DataHubPackage.Editor.BulkEdit
{
    /// <summary>One row of a bulk edit preview: the asset and its value before and after.</summary>
    internal sealed class DataBulkChange
    {
        /// <summary>Creates the line.</summary>
        /// <param name="name">The asset name.</param>
        /// <param name="before">The value now.</param>
        /// <param name="after">The value the edit gives it.</param>
        internal DataBulkChange(string name, double before, double after)
        {
            Name = name;
            Before = before;
            After = after;
        }

        /// <summary>The asset name.</summary>
        internal string Name { get; }

        /// <summary>The value now.</summary>
        internal double Before { get; }

        /// <summary>The value the edit gives it.</summary>
        internal double After { get; }

        /// <summary>True when the edit changes this row.</summary>
        internal bool IsChanged => Before != After;
    }
}