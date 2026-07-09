using Base.AttributePackage.Editor.Core;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Decides whether a member is drawn at all. All handlers must agree for the field to show.
    /// </summary>
    public interface IVisibilityHandler
    {
        /// <summary>Returns false to hide the member.</summary>
        bool ShouldShow(in MemberContext context);
    }
}