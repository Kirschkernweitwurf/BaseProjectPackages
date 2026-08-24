namespace Base.ServicePackage.Editor
{
    /// <summary>
    /// The condition of one entry in the <see cref="ServiceLocator"/>, as the window reports it.
    /// </summary>
    internal enum EServiceState : byte
    {
        /// <summary>The instance is usable and implements the type it is filed under.</summary>
        Alive = 0,

        /// <summary>The instance was destroyed without deregistering, so the entry is stale.</summary>
        Destroyed = 1,

        /// <summary>The instance does not implement the type it is filed under, so lookups fail.</summary>
        Mismatch = 2
    }
}