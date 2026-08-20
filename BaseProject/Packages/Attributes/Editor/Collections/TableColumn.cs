namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>One column of a table: which child property it shows, how wide, and under what header.</summary>
    internal readonly struct TableColumn
    {
        /// <summary>Name of the child property this column shows.</summary>
        public readonly string PropertyName;

        /// <summary>Header text.</summary>
        public readonly string Header;

        /// <summary>Share of the available width relative to the other columns.</summary>
        public readonly float Weight;

        /// <summary>Creates a column.</summary>
        /// <param name="propertyName">Name of the child property this column shows.</param>
        /// <param name="header">Header text.</param>
        /// <param name="weight">Share of the available width.</param>
        public TableColumn(string propertyName, string header, float weight)
        {
            PropertyName = propertyName;
            Header = header;
            Weight = weight;
        }
    }
}