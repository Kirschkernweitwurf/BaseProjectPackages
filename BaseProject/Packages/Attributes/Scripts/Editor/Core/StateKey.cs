using System;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Builds the string keys used for EditorPrefs entries and for per-field editor state, so every
    /// caller composes them the same way and keys never collide by accident.
    /// </summary>
    internal static class StateKey
    {
        private const string Separator = ".";

        /// <summary>Key for a named state on an owner type, for example a foldout name.</summary>
        public static string For(Type owner, string name) => owner.FullName + Separator + name;

        /// <summary>Key for a named state on an owner type inside a category, for example a title.</summary>
        public static string For(Type owner, string category, string name)
            => owner.FullName + Separator + category + Separator + name;

        /// <summary>Key for one serialized field on one concrete instance.</summary>
        public static string For(int instanceId, string propertyPath) => instanceId + Separator + propertyPath;

        /// <summary>Key for a single component of a vector field.</summary>
        public static string For(string propertyPath, int component) => propertyPath + Separator + component;
    }
}