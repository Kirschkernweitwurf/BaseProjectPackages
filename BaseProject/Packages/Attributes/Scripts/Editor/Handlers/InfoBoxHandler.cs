namespace Base.AttributePackage.Editor
{
    /// <summary>Draws the box for <see cref="InfoBoxAttribute"/>, above or below, compact or full.</summary>
    internal sealed class InfoBoxHandler : IBeforeFieldHandler, IAfterFieldHandler
    {
        public int Order => 20;

        public void AfterField(in MemberContext context) => Draw(context, EInfoBoxPosition.Below);

        public void BeforeField(in MemberContext context) => Draw(context, EInfoBoxPosition.Above);

        private static void Draw(in MemberContext context, EInfoBoxPosition position)
        {
            InfoBoxAttribute attribute = context.GetAttribute<InfoBoxAttribute>();

            if (attribute != null && attribute.Position == position)
                InfoBoxRenderer.Draw(attribute, ValueResolver.Text(context, attribute.Message));
        }
    }
}