using System;

namespace Base.TweeningPackage.Core.Data
{
    /// <summary>
    /// Marks the serialized bool that turns the shared settings asset on. The inspector looks for
    /// this attribute instead of the field name, so renaming the field cannot break the layout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TweenSettingsToggleAttribute : Attribute { }
}