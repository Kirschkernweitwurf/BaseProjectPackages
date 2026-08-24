namespace Base.ServicePackage.Editor
{
    /// <summary>
    /// The columns of the service table that can be sorted by.
    /// </summary>
    internal enum EServiceColumn : byte
    {
        /// <summary>The type the registered instance actually is.</summary>
        Instance = 0,

        /// <summary>The game object and scene the instance lives in.</summary>
        Location = 1,

        /// <summary>The type the service is filed under. This is also the window's default order.</summary>
        Service = 2,

        /// <summary>The condition of the entry, worst first.</summary>
        State = 3
    }
}