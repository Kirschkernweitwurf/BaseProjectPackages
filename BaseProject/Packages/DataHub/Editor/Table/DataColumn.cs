using System;
using Base.DataHubPackage.Editor.Extensions;
using UnityEngine;

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

        /// <summary>The title with the struct name in front, like "Mood Scores Electrified".</summary>
        internal string FullTitle => Group == null
            ? Header
            : Group + " " + Header;

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

        /// <summary>True when the user dragged this column before, so its width is theirs and not measured.</summary>
        internal bool HasSavedWidth { get; set; }

        /// <summary>What the column holds and how to edit it, shown on its title.</summary>
        internal string Tooltip { get; set; }

        /// <summary>Where the title and the value sit in the column: numbers right, toggles centered.</summary>
        internal TextAnchor Alignment { get; set; } = TextAnchor.MiddleLeft;

        /// <summary>The smallest value the field allows through <c>[Min]</c> or <c>[Range]</c>, or null.</summary>
        internal float? MinValue { get; set; }

        /// <summary>The largest value the field allows through <c>[Range]</c>, or null.</summary>
        internal float? MaxValue { get; set; }

        /// <summary>True for an editable number field, which bulk edit can change.</summary>
        internal bool IsNumber => Kind == EDataColumnKind.Field
            && IsEditable
            && IsNumberType(FieldType);

        /// <summary>True for a whole number field, where bulk edit rounds its results.</summary>
        internal bool IsWholeNumber => FieldType == typeof(int)
            || FieldType == typeof(long);

        /// <summary>The width the column is drawn at.</summary>
        internal float Width { get; set; }

        /// <summary>True for the number types a cell can edit.</summary>
        /// <param name="type">The field type.</param>
        /// <returns>True for int, long, float and double.</returns>
        internal static bool IsNumberType(Type type) => type == typeof(int)
            || type == typeof(long)
            || type == typeof(float)
            || type == typeof(double);
    }
}