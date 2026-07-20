using System.Reflection;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// One cached inspector button: the annotated method, its attribute and the resolved label.
    /// Built once per type by <see cref="ButtonRenderer"/>.
    /// </summary>
    public readonly struct InspectorButton
    {
        /// <summary>The parameterless method invoked by the button.</summary>
        public readonly MethodInfo Method;

        /// <summary>The attribute carrying mode and confirmation settings.</summary>
        public readonly ButtonAttribute Attribute;

        /// <summary>Resolved button label, either the custom label or the nicified method name.</summary>
        public readonly string Label;

        /// <summary>Creates a cached button entry.</summary>
        public InspectorButton(MethodInfo method, ButtonAttribute attribute, string label)
        {
            Method = method;
            Attribute = attribute;
            Label = label;
        }
    }
}