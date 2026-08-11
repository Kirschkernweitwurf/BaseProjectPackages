namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Opens a <see cref="StartExpandedAttribute"/> field the first time it is drawn. Only the first
    /// draw is forced, so folding it up afterwards sticks.
    /// </summary>
    public sealed class StartExpandedHandler : IBeforeFieldHandler
    {
        private const int HandlerOrder = -60;

        public int Order => HandlerOrder;

        public void BeforeField(in MemberContext context)
        {
            if (context.GetAttribute<StartExpandedAttribute>() == null)
                return;

            if (FirstDraw.IsFirst(context.Property))
                context.Property.isExpanded = true;
        }
    }
}
