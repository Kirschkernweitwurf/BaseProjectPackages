using System.Reflection;

namespace Base.AttributesPackage.Editor.Core
{
    /// <summary>One control in the component header, resolved once per type so repaints run no reflection.</summary>
    internal readonly struct HeaderItem
    {
        /// <summary>The member the control was declared on.</summary>
        internal readonly MemberInfo Member;

        /// <summary>What the control does.</summary>
        internal readonly EHeaderItemKind Kind;

        /// <summary>The button settings, or null for the other kinds.</summary>
        internal readonly HeaderButtonAttribute Button;

        /// <summary>The resolved label, used by buttons only.</summary>
        internal readonly string Label;

        /// <summary>Width the control takes in the header.</summary>
        internal readonly float Width;

        /// <summary>Creates a header item.</summary>
        /// <param name="member">The member the control was declared on.</param>
        /// <param name="kind">What the control does.</param>
        /// <param name="button">The button settings, or null.</param>
        /// <param name="label">The resolved label.</param>
        /// <param name="width">Width the control takes.</param>
        public HeaderItem(MemberInfo member, EHeaderItemKind kind, HeaderButtonAttribute button, string label,
            float width)
        {
            Member = member;
            Kind = kind;
            Button = button;
            Label = label;
            Width = width;
        }
    }
}