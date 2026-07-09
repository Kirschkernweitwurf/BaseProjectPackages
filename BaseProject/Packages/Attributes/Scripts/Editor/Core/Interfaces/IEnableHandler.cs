namespace Base.AttributePackage.Editor.Core.Interfaces
{
    /// <summary>
    /// Decides whether a member is editable. Any handler returning false disables the field.
    /// </summary>
    public interface IEnableHandler
    {
        /// <summary>Returns false to disable the member.</summary>
        bool ShouldEnable(in MemberContext context);
    }
}