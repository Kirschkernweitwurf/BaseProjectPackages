using System;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Draws one scene view handle for one attribute. Implement this anywhere and
    /// <see cref="HandleRegistry"/> picks it up, the same discovery the inspector handlers use.
    /// Drawers are stateless, since one instance serves every inspected object.
    /// </summary>
    internal interface IHandleDrawer
    {
        /// <summary>The attribute this drawer reacts to.</summary>
        Type AttributeType { get; }

        /// <summary>Draws the handle for one field.</summary>
        /// <param name="context">The field being visualized and the object that owns it.</param>
        /// <param name="attribute">The attribute instance found on the field.</param>
        void Draw(in HandleContext context, Attribute attribute);
    }
}