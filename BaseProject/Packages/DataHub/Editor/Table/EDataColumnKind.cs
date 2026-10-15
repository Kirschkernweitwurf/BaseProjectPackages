namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>Where the value of a table column comes from.</summary>
    internal enum EDataColumnKind : byte
    {
        /// <summary>The file name of the asset. Always the first column.</summary>
        Name = 0,

        /// <summary>
        /// The validation badge of the row. Always right after the name, so it is never scrolled away.
        /// </summary>
        Check = 1,

        /// <summary>One serialized field, read and written through its property.</summary>
        Field = 2,

        /// <summary>A computed value from an <see cref="Extensions.IDataColumn"/>. Read only.</summary>
        Computed = 3
    }
}