namespace Base.ToolPackage.Identification
{
    /// <summary>
    /// Stable design-time identifier for assets or definitions.
    /// </summary>
    public interface IUniquelyIdentifiable
    {
        /// <summary>
        /// Globally unique, editor-assigned identifier.
        /// </summary>
        string UniqueId { get; }

#if UNITY_EDITOR
        /// <summary>
        /// Generate a new unique ID.
        /// </summary>
        void RegenerateId();
#endif
    }
}