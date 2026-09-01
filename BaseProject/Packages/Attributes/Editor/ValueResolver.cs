using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Logging;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Resolves a string attribute argument that may be either a literal or a reference to a member.
    /// A value starting with a dollar sign names a member to read; anything else is used
    /// as written.
    /// </summary>
    /// <remarks>
    /// This is what turns every fixed message in the package into a live one. A title, an info box, a
    /// label, a validation message and a button caption are all just strings, and every one of them is
    /// sometimes better computed than typed: a header that counts its section, a warning that names the
    /// value that broke it, a button that says what it will do to the current state.
    /// <para>
    /// Written as <c>"$" + nameof(Member)</c> rather than as a bare string, so a rename still moves the
    /// reference with it. The prefix is what distinguishes a reference from a literal that happens to
    /// share a member's name, and it is the only string-encoded part of the convention.
    /// </para>
    /// </remarks>
    internal static class ValueResolver
    {
        // Marks a string argument as naming a member rather than carrying a literal. Private because a
        // caller writes the prefix into the attribute argument by hand; nothing reads it back.
        private const char MemberPrefix = '$';

        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        // A member reference that cannot be resolved is reported once per member rather than on every
        // repaint, which would be dozens of identical console lines a second.
        private static readonly HashSet<string> Reported = new();

        /// <summary>Returns whether the argument names a member instead of carrying a literal.</summary>
        /// <param name="value">The attribute argument.</param>
        /// <returns>True when the value is a member reference.</returns>
        internal static bool IsMemberReference(string value) => !string.IsNullOrEmpty(value)
            && value[0] == MemberPrefix;

        /// <summary>
        /// Resolves a text argument. A literal is returned as written; a member reference is read from
        /// the object that owns the field and converted with ToString.
        /// </summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="value">The attribute argument.</param>
        /// <returns>The text to show, or the argument itself when it cannot be resolved.</returns>
        internal static string Text(in MemberContext context, string value)
        {
            if (!IsMemberReference(value))
                return value;

            return TryRead(context.DeclaringType, context.DeclaringObject, MemberName(value), out object read)
                ? read?.ToString() ?? string.Empty
                : value;
        }

        /// <summary>Reads a member by name, accepting a field, a readable property or a method.</summary>
        /// <param name="type">The type that declares the member.</param>
        /// <param name="owner">The instance to read from.</param>
        /// <param name="member">Name of the member.</param>
        /// <param name="value">The value read.</param>
        /// <returns>True when the member existed and could be read.</returns>
        internal static bool TryRead(Type type, object owner, string member, out object value)
        {
            value = null;

            if (type == null || owner == null || string.IsNullOrEmpty(member))
                return false;

            if (MemberValueResolver.TryResolve(type, owner, member, out value))
                return true;

            MethodInfo method = type.GetMethod(member, MethodFlags);
            if (method != null && method.GetParameters().Length == 0 && method.ReturnType != typeof(void))
            {
                value = method.Invoke(owner, null);
                return true;
            }

            Report(type, owner, member);
            return false;
        }

        /// <summary>Strips the prefix from a member reference.</summary>
        /// <param name="value">The attribute argument.</param>
        /// <returns>The member name.</returns>
        internal static string MemberName(string value) => IsMemberReference(value)
            ? value[1..]
            : value;

        // The owner is passed as the log context so the console line selects the object it is about. A
        // nested serializable type is not a Unity object, in which case there is nothing to select and
        // the message still names the type that was looked on.
        private static void Report(Type type, object owner, string member)
        {
            string key = type.FullName + "." + member;

            if (!Reported.Add(key))
                return;

            CustomLogger.LogWarning(
                $"'{member}' was not found on {type.Name}, so the attribute falls back to the literal.",
                owner as Object);
        }
    }
}