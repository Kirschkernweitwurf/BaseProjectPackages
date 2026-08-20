namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// A handler that draws a member itself instead of letting the normal field be drawn.
    /// </summary>
    /// <remarks>
    /// The one extension point that can replace a whole collection. A property drawer cannot: Unity
    /// applies a PropertyAttribute drawer to each element of an array rather than to the array, so a
    /// drawer can restyle rows but never remove them.
    /// </remarks>
    internal interface IFieldReplacementHandler
    {
        /// <summary>Draws the member, or declines and lets the normal field be drawn.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>True when this handler drew the member.</returns>
        bool TryDraw(in MemberContext context);
    }
}