namespace Utility.Identification
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
    }
}