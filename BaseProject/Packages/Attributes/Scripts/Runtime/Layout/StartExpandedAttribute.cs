using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Opens a nested object or an array the first time it is seen, instead of leaving it folded away.
    /// The collection and title attributes carry their own expanded setting; this is for the plain
    /// nested fields and arrays that have nowhere else to say it.
    /// </summary>
    /// <remarks>
    /// Only the first draw is forced. Folding it up afterwards sticks, because an attribute that
    /// reopened the field every repaint would be unusable.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class StartExpandedAttribute : PropertyAttribute { }
}
