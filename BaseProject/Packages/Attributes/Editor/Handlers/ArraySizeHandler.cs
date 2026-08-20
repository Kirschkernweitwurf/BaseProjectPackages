using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Keeps an <see cref="ArraySizeAttribute"/> field inside its bounds. Runs before the field so the
    /// inspector never shows a size the attribute forbids, not even for one frame.
    /// </summary>
    internal sealed class ArraySizeHandler : IBeforeFieldHandler
    {
        private const int HandlerOrder = -50;

        public int Order => HandlerOrder;

        public void BeforeField(in MemberContext context)
        {
            if (!ArraySizeLimits.TryGet(context, out int minimum, out int maximum))
                return;

            SerializedProperty property = context.Property;
            int clamped = property.arraySize;

            if (minimum >= 0)
                clamped = Mathf.Max(clamped, minimum);

            if (maximum >= 0)
                clamped = Mathf.Min(clamped, maximum);

            if (clamped != property.arraySize)
                property.arraySize = clamped;
        }
    }
}