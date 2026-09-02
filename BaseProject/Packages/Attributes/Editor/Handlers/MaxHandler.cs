using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>
    /// Clamps <see cref="MaxAttribute"/> fields to a maximum. Applies to int and float and to each
    /// component of Vector2, Vector3, Vector2Int and Vector3Int.
    /// </summary>
    internal sealed class MaxHandler : IAfterFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 10;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            MaxAttribute attribute = context.GetAttribute<MaxAttribute>();
            if (attribute == null)
                return;

            NumericPropertyClamp.Apply(context.Property, float.NegativeInfinity, attribute.Max);
        }
    }
}