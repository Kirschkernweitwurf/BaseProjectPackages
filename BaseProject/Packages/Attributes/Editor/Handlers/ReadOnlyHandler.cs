using Base.AttributePackage.Editor.Core.Interfaces;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Disables <see cref="ReadOnlyAttribute"/> fields while keeping them visible.</summary>
    internal sealed class ReadOnlyHandler : IEnableHandler
    {
        /// <inheritdoc/>
        public bool ShouldEnable(in MemberContext context) => context.GetAttribute<ReadOnlyAttribute>() == null;
    }
}