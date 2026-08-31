using System;
using System.Text;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Turns the names reflection reports into the names a person reads. Reflection writes a generic
    /// type as <c>Pool`1</c> and a nested one as <c>Outer+Inner</c>, neither of which matches how the
    /// type is written in source or what an asset search would find it by.
    /// </summary>
    public static class TypeNameUtility
    {
        private const char ArityMarker = '`';
        private const char NestedMarker = '+';
        private const string Separator = ", ";

        /// <summary>
        /// Drops the arity suffix from a raw reflection name, so <c>Pool`1</c> becomes <c>Pool</c>.
        /// </summary>
        /// <remarks>
        /// This is the name a script file carries, which is why anything looking for the file that
        /// declares a type has to go through here first. A backtick in an asset search matches
        /// nothing at all.
        /// </remarks>
        /// <param name="name">A name as reflection reports it.</param>
        /// <returns>The name without the arity suffix, or an empty string when there was no name.</returns>
        public static string TrimArity(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            int marker = name.IndexOf(ArityMarker);

            return marker < 0
                ? name
                : name[..marker];
        }

        /// <summary>
        /// Formats a type the way it would be written in source, generic arguments included.
        /// </summary>
        /// <param name="type">The type to format.</param>
        /// <returns>The readable name, without the namespace.</returns>
        public static string Format(Type type)
        {
            if (type == null)
                return string.Empty;

            if (type.IsArray)
                return $"{Format(type.GetElementType())}[]";

            // A by ref parameter reads as its own type; the reference is in the signature, not the name.
            if (type.IsByRef)
                return Format(type.GetElementType());

            if (!type.IsGenericType)
                return Readable(type.Name);

            StringBuilder builder = new(Readable(type.Name));
            Type[] arguments = type.GetGenericArguments();

            builder.Append('<');

            for (int index = 0; index < arguments.Length; index++)
            {
                if (index > 0)
                    builder.Append(Separator);

                builder.Append(Format(arguments[index]));
            }

            builder.Append('>');

            return builder.ToString();
        }

        /// <summary>
        /// The display name of a type including the types it is nested in, joined by dots.
        /// </summary>
        /// <param name="type">The type to name.</param>
        /// <returns>The short name, without the namespace.</returns>
        public static string FormatShortName(Type type)
        {
            if (type == null)
                return string.Empty;

            if (type.DeclaringType == null)
                return Format(type);

            return $"{FormatShortName(type.DeclaringType)}.{Format(type)}";
        }

        /// <summary>
        /// The namespace qualified display name of a type.
        /// </summary>
        /// <param name="type">The type to name.</param>
        /// <returns>The full name, falling back to the short name for a type with no namespace.</returns>
        public static string FormatFullName(Type type)
        {
            if (type == null)
                return string.Empty;

            string shortName = FormatShortName(type);

            return string.IsNullOrEmpty(type.Namespace)
                ? shortName
                : $"{type.Namespace}.{shortName}";
        }

        private static string Readable(string name) => TrimArity(name).Replace(NestedMarker, '.');
    }
}