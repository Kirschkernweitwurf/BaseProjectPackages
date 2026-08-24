namespace Base.SaveSystemPackage.Unity.Autosave
{
    /// <summary>
    /// Which slot an autosave writes to.
    /// </summary>
    public enum EAutosaveTarget : byte
    {
        /// <summary>A slot of its own, so the timer can never overwrite a save the player made.</summary>
        DedicatedSlot = 0,

        /// <summary>
        /// The slot the player currently has selected. Autosaving is skipped while nothing is selected,
        /// because minting a slot on a timer would fill the load menu with one entry per interval.
        /// </summary>
        SelectedSlot = 1
    }
}