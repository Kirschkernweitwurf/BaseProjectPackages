using System;
using Base.DataHubPackage.Editor.Extensions;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// One column of the Data Hub table: what it shows, where the value comes from and its start width.
    /// </summary>
    internal sealed class DataColumn
    {
        /// <summary>Creates a column.</summary>
        /// <param name="kind">Where the value comes from.</param>
        /// <param name="key">Identifies the column across sessions, for its saved width.</param>
        /// <param name="header">The title.</param>
        /// <param name="group">The title of the struct the field sits in, or null.</param>
        /// <param name="defaultWidth">The width before the user drags it.</param>
        internal DataColumn(EDataColumnKind kind, string key, string header, string group, float defaultWidth)
        {
            Kind = kind;
            Key = key;
            Header = header;
            Group = group;
            DefaultWidth = defaultWidth;
        }

        /// <summary>Where the value comes from.</summary>
        internal EDataColumnKind Kind { get; }

        /// <summary>Identifies the column across sessions.</summary>
        internal string Key { get; }

        /// <summary>The title.</summary>
        internal string Header { get; }

        /// <summary>The title of the struct the field sits in, shown above a run of its columns, or null.</summary>
        internal string Group { get; }

        /// <summary>The width before the user drags it.</summary>
        internal float DefaultWidth { get; }

        /// <summary>The serialized property path of a field column.</summary>
        internal string PropertyPath { get; set; }

        /// <summary>The declared type of a field column, for the object picker.</summary>
        internal Type FieldType { get; set; }

        /// <summary>True when the cell can be edited in the table.</summary>
        internal bool IsEditable { get; set; }

        /// <summary>The extension behind a computed column.</summary>
        internal IDataColumn Computed { get; set; }

        /// <summary>The width the column is drawn at.</summary>
        internal float Width { get; set; }
    }
}