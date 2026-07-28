namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Clamps <see cref="MaxAttribute"/> fields to a maximum. Applies to int and float and to each
    /// component of Vector2, Vector3, Vector2Int and Vector3Int.
    /// </summary>
    public sealed class MaxHandler : IAfterFieldHandler
    {
        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            MaxAttribute attribute = context.GetAttribute<MaxAttribute>();
            if (attribute == null)
                return;

            NumericPropertyClamp.Apply(context.Property, float.NegativeInfinity, attribute.Max);
        }
    }
}