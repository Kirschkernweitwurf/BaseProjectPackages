using System;

namespace Base.DataHubPackage.Editor.Extensions
{
    /// <summary>
    /// One entry in the type tree of the Data Hub: a data type whose assets are listed as a table.
    /// Found automatically, so adding a type to the hub is one class with a parameterless constructor.
    /// </summary>
    /// <remarks>
    /// An abstract <see cref="DataType"/> lists the assets of every type derived from it, with the
    /// columns the base type declares.
    /// </remarks>
    public interface IDataTable
    {
        /// <summary>
        /// Where the entry sits in the tree, with '/' between levels, for example "Cards/Dialogue".
        /// A parent that no table claims is shown as a plain group.
        /// </summary>
        string Path { get; }

        /// <summary>The asset type listed. Usually a <see cref="UnityEngine.ScriptableObject"/>.</summary>
        Type DataType { get; }

        /// <summary>Position among its siblings in the tree. Lower comes first.</summary>
        int Order { get; }

        /// <summary>
        /// What this data is, for someone new to the project, shown when hovering the tree entry and the
        /// title. Null shows nothing.
        /// </summary>
        string Description => null;

        /// <summary>
        /// True for a serialized field this table leaves out, like a base class field that means nothing for
        /// the listed type. Shows every field by default.
        /// </summary>
        /// <param name="fieldPath">The property path of the field, for example "description".</param>
        /// <returns>True to hide the column.</returns>
        bool IsFieldHidden(string fieldPath) => false;

        /// <summary>True for a computed column this table leaves out. Shows every column by default.</summary>
        /// <param name="columnType">The type of the <see cref="IDataColumn"/>.</param>
        /// <returns>True to hide the column.</returns>
        bool IsColumnHidden(Type columnType) => false;
    }
}