using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Opens a <see cref="StartExpandedAttribute"/> field the first time it is drawn. Only the first
    /// draw is forced, so folding it up afterward sticks.
    /// </summary>
    internal sealed class StartExpandedHandler : IBeforeFieldHandler
    {
        private const int HandlerOrder = -60;

        /// <inheritdoc/>
        public int Order => HandlerOrder;

        /// <inheritdoc/>
        public void BeforeField(in MemberContext context)
        {
            if (context.GetAttribute<StartExpandedAttribute>() == null)
                return;

            if (FirstDraw.IsFirst(context.Property))
                context.Property.isExpanded = true;
        }
    }
}