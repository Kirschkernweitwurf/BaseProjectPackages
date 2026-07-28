namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Clamps <see cref="MinMaxAttribute"/> fields into an inclusive range. Applies to int and float
    /// and to each component of Vector2, Vector3, Vector2Int and Vector3Int.
    /// </summary>
    public sealed class MinMaxHandler : IAfterFieldHandler
    {
        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            MinMaxAttribute attribute = context.GetAttribute<MinMaxAttribute>();
            if (attribute == null)
                return;

            NumericPropertyClamp.Apply(context.Property, attribute.Min, attribute.Max);
        }
    }
}