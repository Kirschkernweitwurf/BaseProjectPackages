using System;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Convenience base that removes the cast every handle drawer would otherwise repeat.
    /// </summary>
    /// <typeparam name="TAttribute">The attribute this drawer reacts to.</typeparam>
    /// <remarks>
    /// The two members below are public although the class is internal. An implicit interface
    /// implementation has to be public whatever the access level of the type declaring it, so
    /// narrowing them does not compile.
    /// </remarks>
    internal abstract class HandleDrawer<TAttribute> : IHandleDrawer where TAttribute : Attribute
    {
        /// <inheritdoc/>
        public Type AttributeType => typeof(TAttribute);

        /// <inheritdoc/>
        public void Draw(in HandleContext context, Attribute attribute) => Draw(context, (TAttribute)attribute);

        /// <summary>Draws the handle for one field.</summary>
        /// <param name="context">The field being visualized and the object that owns it.</param>
        /// <param name="attribute">The typed attribute instance found on the field.</param>
        protected abstract void Draw(in HandleContext context, TAttribute attribute);
    }
}