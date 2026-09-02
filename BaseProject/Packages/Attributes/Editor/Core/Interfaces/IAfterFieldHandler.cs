namespace Base.AttributesPackage.Editor.Core.Interfaces
{
    /// <summary>
    /// Runs after a member is drawn. Used for clamping, validation messages, constraints and previews.
    /// Handlers run in ascending <see cref="Order"/>.
    /// </summary>
    internal interface IAfterFieldHandler
    {
        /// <summary>Run order. Lower runs first. Constraints low, validation mid, previews high.</summary>
        int Order { get; }

        /// <summary>Called once after the field was drawn.</summary>
        void AfterField(in MemberContext context);
    }
}