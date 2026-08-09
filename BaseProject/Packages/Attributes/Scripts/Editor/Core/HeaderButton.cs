using System.Reflection;

namespace Base.AttributePackage.Editor
{
    /// <summary>One header button, resolved once per type so repaints run no reflection.</summary>
    public readonly struct HeaderButton
    {
        /// <summary>The method invoked when the button is pressed.</summary>
        public readonly MethodInfo Method;

        /// <summary>The attribute that declared the button.</summary>
        public readonly HeaderButtonAttribute Attribute;

        /// <summary>The resolved label shown on the button.</summary>
        public readonly string Label;

        /// <summary>Creates a header button record.</summary>
        /// <param name="method">The method invoked when the button is pressed.</param>
        /// <param name="attribute">The attribute that declared the button.</param>
        /// <param name="label">The resolved label shown on the button.</param>
        public HeaderButton(MethodInfo method, HeaderButtonAttribute attribute, string label)
        {
            Method = method;
            Attribute = attribute;
            Label = label;
        }
    }
}
