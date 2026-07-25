#if UNITY_EDITOR
using System;
using Base.ToolPackage.Editor.MenuOverview;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Immutable description of a single menu item, either a <see cref="MenuItem"/> attribute
    /// found in the project, a package or one of Unity's built-in assemblies, or an entry
    /// registered through the menu manager.
    /// </summary>
    public sealed class MenuItemEntry
    {
        private const string EmptyLabel = "-";

        /// <summary>Full menu path, e.g. "Tools/My Tool".</summary>
        public string MenuPath { get; }

        /// <summary>Top-level menu segment, e.g. "Tools", used for grouping and filtering.</summary>
        public string Root { get; }

        /// <summary>Type that declares the decorated method, or null when the code is gone.</summary>
        public Type DeclaringType { get; }

        /// <summary>Name of the decorated method, used to locate the source line.</summary>
        public string MethodName { get; }

        /// <summary>"Type.Method" label, used as the secondary column.</summary>
        public string Member { get; }

        /// <summary>Menu priority that orders the item inside its parent menu.</summary>
        public int Priority { get; }

        /// <summary>Priority formatted for display. A dash when no priority is assigned.</summary>
        public string PriorityLabel { get; }

        /// <summary>True when the method only validates whether the item is enabled.</summary>
        public bool IsValidation { get; }

        /// <summary>Whether the item is declared by an attribute or managed by the menu manager.</summary>
        public EMenuDefinition Definition { get; }

        /// <summary>Live state of the item.</summary>
        public EMenuEntryState State { get; }

        /// <summary>Menu manager id of a dynamic item, or an empty string for a static one.</summary>
        public string EntryId { get; }

        /// <summary>Where the defining script lives.</summary>
        public EMenuItemOrigin Origin { get; }

        /// <summary>Script asset that defines the item, or null for built-in items.</summary>
        public MonoScript Script { get; }

        /// <summary>Project-relative asset path, or a dash for built-in items.</summary>
        public string AssetPath { get; }

        /// <summary>True when the item is arranged in the menu manager.</summary>
        public bool IsDynamic => Definition == EMenuDefinition.Dynamic;

        private MenuItemEntry(string menuPath, Type declaringType, string methodName, int priority,
            bool isValidation, EMenuDefinition definition, EMenuEntryState state, string entryId,
            EMenuItemOrigin origin, MonoScript script, string assetPath)
        {
            MenuPath = menuPath;
            int separator = menuPath.IndexOf('/');
            Root = separator >= 0
                ? menuPath[..separator]
                : menuPath;

            DeclaringType = declaringType;
            MethodName = methodName;
            Member = declaringType != null
                ? $"{declaringType.Name}.{methodName}"
                : methodName;

            Priority = priority;
            PriorityLabel = priority == int.MinValue
                ? EmptyLabel
                : priority.ToString();

            IsValidation = isValidation;
            Definition = definition;
            State = state;
            EntryId = entryId ?? string.Empty;
            Origin = origin;
            Script = script;
            AssetPath = string.IsNullOrEmpty(assetPath)
                ? EmptyLabel
                : assetPath;
        }

        /// <summary>Creates an entry for a <see cref="MenuItem"/> attribute.</summary>
        public static MenuItemEntry Attributed(string menuPath, Type declaringType, string methodName, int priority,
            bool isValidation, EMenuItemOrigin origin, MonoScript script, string assetPath)
            => new(menuPath, declaringType, methodName, priority, isValidation, EMenuDefinition.Static,
                EMenuEntryState.Active, string.Empty, origin, script, assetPath);

        /// <summary>Creates an entry for an item registered through the menu manager.</summary>
        public static MenuItemEntry Managed(string entryId, string menuPath, Type declaringType, string methodName,
            int priority, EMenuEntryState state, EMenuItemOrigin origin, MonoScript script, string assetPath)
            => new(menuPath, declaringType, methodName, priority, false, EMenuDefinition.Dynamic, state, entryId,
                origin, script, assetPath);
    }
}
#endif