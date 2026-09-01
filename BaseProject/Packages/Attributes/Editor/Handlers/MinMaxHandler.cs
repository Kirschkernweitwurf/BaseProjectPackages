using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Clamps <see cref="MinMaxAttribute"/> fields into an inclusive range. Applies to int and float
    /// and to each component of Vector2, Vector3, Vector2Int and Vector3Int.
    /// </summary>
    internal sealed class MinMaxHandler : IAfterFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 10;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            MinMaxAttribute attribute = context.GetAttribute<MinMaxAttribute>();
            if (attribute == null)
                return;

            NumericPropertyClamp.Apply(context.Property, attribute.Min, attribute.Max);
        }
    }
}