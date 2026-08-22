using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Decides whether the package draws a given inspector at all, and offers one switch to stop it
    /// drawing any of them.
    /// <para>
    /// The package registers an editor for every MonoBehaviour and every ScriptableObject, which is what
    /// lets an attribute work on any type without that type opting in. The cost is reach: a type
    /// declaring nothing from this package is still rendered by this pipeline instead of Unity's, so a
    /// fault anywhere in it reaches every inspector in the project rather than only the ones that asked.
    /// </para>
    /// <para>
    /// Two things narrow that. A type declaring nothing of ours falls through to Unity's own inspector,
    /// which is both cheaper and out of reach of anything here. And the switch turns the package's
    /// drawing off without uninstalling, so a project that hits a bad interaction has a way out that is
    /// not a git revert. It is off by default, so nothing changes for anyone who never touches it.
    /// </para>
    /// </summary>
    public static class AttributeInspectorSwitch
    {
        /// <summary>The editor preference the switch is stored under. Per user, per machine.</summary>
        public const string DisabledPreferenceKey = "Base.AttributePackage.InspectorDisabled";

        private const BindingFlags MemberFlags = BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.DeclaredOnly;

        private static readonly Assembly AttributeAssembly = typeof(TitleAttribute).Assembly;

        // Whether a type carries anything from this package. Walking every member of every inspected
        // type is far too much to repeat per repaint, and the answer cannot change without a domain
        // reload, which clears this.
        private static readonly Dictionary<Type, bool> Declaring = new();

        /// <summary>Whether the package's inspector is switched off for this user.</summary>
        public static bool IsDisabled
        {
            get => EditorPrefs.GetBool(DisabledPreferenceKey, false);
            set => EditorPrefs.SetBool(DisabledPreferenceKey, value);
        }

        /// <summary>
        /// Whether the package should draw the inspector for the given type.
        /// </summary>
        /// <param name="type">The inspected type.</param>
        /// <returns><c>true</c> when the switch is on and the type declares an attribute of ours.</returns>
        public static bool ShouldDraw(Type type)
        {
            if (IsDisabled)
                return false;

            if (type == null)
                return false;

            if (Declaring.TryGetValue(type, out bool declares))
                return declares;

            declares = DeclaresAttribute(type);
            Declaring[type] = declares;

            return declares;
        }

        // The hierarchy is walked rather than the type alone, because a serialized field carrying an
        // attribute is very often declared on a base class the derived type does not repeat.
        private static bool DeclaresAttribute(Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (HasAttribute(type) || HasMemberAttribute(type))
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        private static bool HasMemberAttribute(Type type)
        {
            foreach (MemberInfo member in type.GetMembers(MemberFlags))
            {
                if (HasAttribute(member))
                    return true;
            }

            return false;
        }

        // Ownership is decided by assembly rather than by a list of names, so an attribute added to the
        // package later is recognised without anything here being updated. Attribute data is read
        // without instantiating, so an attribute whose constructor throws cannot take the inspector out.
        private static bool HasAttribute(MemberInfo member)
        {
            foreach (CustomAttributeData data in member.GetCustomAttributesData())
            {
                if (data.AttributeType.Assembly == AttributeAssembly)
                    return true;
            }

            return false;
        }
    }
}