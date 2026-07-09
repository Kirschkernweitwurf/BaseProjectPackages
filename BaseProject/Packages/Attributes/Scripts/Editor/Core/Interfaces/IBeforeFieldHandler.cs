namespace Base.AttributePackage.Editor.Core.Interfaces
{
    /// <summary>
    /// Runs before a member is drawn. Used for section titles, separator lines and info boxes.
    /// Handlers run in ascending <see cref="Order"/>. Works on any field type, including lists.
    /// </summary>
    public interface IBeforeFieldHandler
    {
        /// <summary>Run order. Lower runs first.</summary>
        int Order { get; }

        /// <summary>Called once before the field is drawn.</summary>
        void BeforeField(in MemberContext context);
    }
}