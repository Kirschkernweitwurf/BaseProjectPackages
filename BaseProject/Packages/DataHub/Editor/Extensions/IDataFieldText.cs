using System;
using UnityEditor;

namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// Shows a serialized field as one read-only column of plain text instead of one column per inner field.
    /// Meant for types whose inner fields mean nothing to a designer, like a localized string that would
    /// otherwise show a table GUID and a key id where its text should be. Found automatically and used for
    /// every field of <see cref="FieldType"/> on every table.
    /// </summary>
    public interface IDataFieldText
    {
        /// <summary>The field type this reads. It also covers derived types.</summary>
        Type FieldType { get; }

        /// <summary>The text of one cell. Called on repaint, so keep it cheap.</summary>
        /// <param name="property">The property of the field on one asset.</param>
        /// <returns>The cell text.</returns>
        string GetText(SerializedProperty property);
    }
}