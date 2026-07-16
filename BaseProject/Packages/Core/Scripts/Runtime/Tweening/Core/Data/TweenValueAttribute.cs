using System;

namespace Base.CorePackage.Tweening.Core.Data
{
    /// <summary>
    /// Marks a serialized field as a tween value, meaning that an assigned profile replaces it.
    /// The inspector hides every field marked with this while a profile is assigned.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TweenValueAttribute : Attribute { }
}