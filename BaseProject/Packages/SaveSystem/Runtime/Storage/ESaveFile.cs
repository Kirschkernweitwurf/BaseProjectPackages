namespace Base.SaveSystemPackage.Storage
{
    /// <summary>
    /// The files one save is made of. Each maps to a storage key inside the slot folder, so callers
    /// name the part they want instead of spelling out a file name.
    /// </summary>
    public enum ESaveFile : byte
    {
        /// <summary>The collected state of every savable.</summary>
        Data = 0,

        /// <summary>The metadata, written last as the commit marker.</summary>
        Meta = 1,

        /// <summary>The optional thumbnail.</summary>
        Screenshot = 2
    }
}