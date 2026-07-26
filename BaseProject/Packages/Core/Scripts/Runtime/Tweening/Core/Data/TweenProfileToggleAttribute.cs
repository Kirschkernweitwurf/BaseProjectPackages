using System;

namespace Base.CorePackage.Tweening.Core.Data
{
    /// <summary>
    /// Marks the serialized bool that turns the profile asset on. The inspector looks for this
    /// attribute instead of the field name, so renaming the field cannot break the layout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TweenProfileToggleAttribute : Attribute { }
}