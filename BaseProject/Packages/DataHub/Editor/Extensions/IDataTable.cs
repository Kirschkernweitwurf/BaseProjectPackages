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
    }
}