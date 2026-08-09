using System;
using System.Text;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>Turns reflection type names into something readable, without the arity backticks.</summary>
    public static class TypeNameFormatter
    {
        private const char ArityMarker = '`';
        private const char NestedMarker = '+';
        private const string Separator = ", ";

        /// <summary>Formats a type the way it would be written in source.</summary>
        /// <param name="type">Type to format.</param>
        /// <returns>The readable name.</returns>
        public static string Format(Type type)
        {
            if (type == null)
                return string.Empty;

            if (type.IsArray)
                return $"{Format(type.GetElementType())}[]";

            if (type.IsByRef)
                return Format(type.GetElementType());

            if (!type.IsGenericType)
                return TrimArity(type.Name);

            StringBuilder builder = new(TrimArity(type.Name));
            builder.Append('<');

            Type[] arguments = type.GetGenericArguments();
            for (int index = 0; index < arguments.Length; index++)
            {
                if (index > 0)
                    builder.Append(Separator);

                builder.Append(Format(arguments[index]));
            }

            builder.Append('>');
            return builder.ToString();
        }

        /// <summary>Builds the display name of a type including its outer types, without the namespace.</summary>
        /// <param name="type">Type to name.</param>
        /// <returns>The short name, with nested types joined by a dot.</returns>
        public static string FormatShortName(Type type)
        {
            if (type == null)
                return string.Empty;

            if (type.DeclaringType == null)
                return Format(type);

            return $"{FormatShortName(type.DeclaringType)}.{Format(type)}";
        }

        /// <summary>Builds the full name of a type, falling back to the short name when none exists.</summary>
        /// <param name="type">Type to name.</param>
        /// <returns>The namespace qualified name.</returns>
        public static string FormatFullName(Type type)
        {
            if (type == null)
                return string.Empty;

            string shortName = FormatShortName(type);
            return string.IsNullOrEmpty(type.Namespace)
                ? shortName
                : $"{type.Namespace}.{shortName}";
        }

        private static string TrimArity(string name)
        {
            int arity = name.IndexOf(ArityMarker);
            string trimmed = arity < 0
                ? name
                : name[..arity];

            return trimmed.Replace(NestedMarker, '.');
        }
    }
}
