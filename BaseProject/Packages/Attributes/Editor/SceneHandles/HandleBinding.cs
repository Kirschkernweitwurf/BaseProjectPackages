using System;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>One attribute on a field paired with the drawer that reacts to it.</summary>
    internal readonly struct HandleBinding
    {
        /// <summary>The attribute instance found on the field.</summary>
        internal readonly Attribute Attribute;

        /// <summary>The drawer that renders it.</summary>
        internal readonly IHandleDrawer Drawer;

        /// <summary>Creates a binding.</summary>
        /// <param name="attribute">The attribute instance found on the field.</param>
        /// <param name="drawer">The drawer that renders it.</param>
        public HandleBinding(Attribute attribute, IHandleDrawer drawer)
        {
            Attribute = attribute;
            Drawer = drawer;
        }
    }
}