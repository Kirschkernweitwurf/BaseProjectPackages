#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Immutable description of a single <see cref="MenuItem"/> attribute found in the
    /// project, a package or one of Unity's built-in assemblies.
    /// </summary>
    public sealed class MenuItemEntry
    {
        /// <summary>Full menu path, e.g. "Tools/My Tool".</summary>
        public string MenuPath { get; }

        /// <summary>Top-level menu segment, e.g. "Tools", used for grouping and filtering.</summary>
        public string Root { get; }

        /// <summary>Type that declares the decorated method.</summary>
        public Type DeclaringType { get; }

        /// <summary>Name of the decorated method, used to locate the source line.</summary>
        public string MethodName { get; }

        /// <summary>"Type.Method" label, used as the secondary column.</summary>
        public string Member { get; }

        /// <summary>Menu priority that orders the item inside its parent menu.</summary>
        public int Priority { get; }

        /// <summary>True when the method only validates whether the item is enabled.</summary>
        public bool IsValidation { get; }

        /// <summary>Where the defining script lives.</summary>
        public EMenuItemOrigin Origin { get; }

        /// <summary>Script asset that defines the item, or null for built-in items.</summary>
        public MonoScript Script { get; }

        /// <summary>Project-relative asset path, or a dash for built-in items.</summary>
        public string AssetPath { get; }

        /// <summary>Creates an entry from a resolved menu item.</summary>
        public MenuItemEntry(string menuPath, Type declaringType, string methodName, int priority,
            bool isValidation, EMenuItemOrigin origin, MonoScript script, string assetPath)
        {
            MenuPath = menuPath;
            int separator = menuPath.IndexOf('/');
            Root = separator >= 0
                ? menuPath[..separator]
                : menuPath;

            DeclaringType = declaringType;
            MethodName = methodName;
            Member = $"{declaringType.Name}.{methodName}";
            Priority = priority;
            IsValidation = isValidation;
            Origin = origin;
            Script = script;
            AssetPath = string.IsNullOrEmpty(assetPath)
                ? "-"
                : assetPath;
        }
    }
}
#endif