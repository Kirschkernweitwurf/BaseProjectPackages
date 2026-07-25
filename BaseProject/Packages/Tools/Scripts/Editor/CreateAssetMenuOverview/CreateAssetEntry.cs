#if UNITY_EDITOR
using System;
using Base.ToolPackage.Editor.MenuOverview;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CreateAssetMenuOverview
{
    /// <summary>
    /// Immutable description of a single asset creation entry, either a
    /// <see cref="CreateAssetMenuAttribute"/> found in the project, a package or one of Unity's
    /// built-in assemblies, or an entry registered through the menu manager.
    /// </summary>
    public sealed class CreateAssetEntry
    {
        private const string EmptyLabel = "-";

        /// <summary>Menu path under "Assets/Create", e.g. "Balance/Audio Settings".</summary>
        public string MenuName { get; }

        /// <summary>Top-level menu segment, used for grouping and filtering.</summary>
        public string Root { get; }

        /// <summary>ScriptableObject type behind the entry, or null when the code is gone.</summary>
        public Type DeclaringType { get; }

        /// <summary>Short type name, used as the secondary column.</summary>
        public string TypeName { get; }

        /// <summary>Default file name created for new assets of this type.</summary>
        public string FileName { get; }

        /// <summary>Menu order that positions the item inside the Create menu.</summary>
        public int Order { get; }

        /// <summary>Order formatted for display. A dash when no order is assigned.</summary>
        public string OrderLabel { get; }

        /// <summary>Whether the entry is declared by an attribute or managed by the menu manager.</summary>
        public EMenuDefinition Definition { get; }

        /// <summary>Live state of the entry.</summary>
        public EMenuEntryState State { get; }

        /// <summary>Menu manager id of a dynamic entry, or an empty string for a static one.</summary>
        public string EntryId { get; }

        /// <summary>Where the defining script lives.</summary>
        public ECreateAssetOrigin Origin { get; }

        /// <summary>Script asset that defines the type, or null for built-in types.</summary>
        public MonoScript Script { get; }

        /// <summary>Project-relative asset path, or a dash for built-in types.</summary>
        public string AssetPath { get; }

        /// <summary>True when the entry is arranged in the menu manager.</summary>
        public bool IsDynamic => Definition == EMenuDefinition.Dynamic;

        private CreateAssetEntry(string menuName, string fileName, Type declaringType, string typeName, int order,
            EMenuDefinition definition, EMenuEntryState state, string entryId, ECreateAssetOrigin origin,
            MonoScript script, string assetPath)
        {
            // Unity falls back to the type name when no menu name is supplied.
            MenuName = string.IsNullOrEmpty(menuName)
                ? typeName
                : menuName;

            int separator = MenuName.IndexOf('/');
            Root = separator >= 0
                ? MenuName[..separator]
                : MenuName;

            DeclaringType = declaringType;
            TypeName = typeName;

            // Unity falls back to "New {type}" when no file name is supplied.
            FileName = string.IsNullOrEmpty(fileName)
                ? $"New {typeName}"
                : fileName;

            Order = order;
            OrderLabel = order == int.MinValue
                ? EmptyLabel
                : order.ToString();

            Definition = definition;
            State = state;
            EntryId = entryId ?? string.Empty;
            Origin = origin;
            Script = script;
            AssetPath = string.IsNullOrEmpty(assetPath)
                ? EmptyLabel
                : assetPath;
        }

        /// <summary>Creates an entry for a <see cref="CreateAssetMenuAttribute"/>.</summary>
        public static CreateAssetEntry Attributed(string menuName, string fileName, Type declaringType, int order,
            ECreateAssetOrigin origin, MonoScript script, string assetPath)
            => new(menuName, fileName, declaringType, declaringType.Name, order, EMenuDefinition.Static,
                EMenuEntryState.Active, string.Empty, origin, script, assetPath);

        /// <summary>Creates an entry for a type registered through the menu manager.</summary>
        public static CreateAssetEntry Managed(string entryId, string menuName, string fileName, Type declaringType,
            string typeName, int order, EMenuEntryState state, ECreateAssetOrigin origin, MonoScript script,
            string assetPath)
            => new(menuName, fileName, declaringType, typeName, order, EMenuDefinition.Dynamic, state, entryId,
                origin, script, assetPath);
    }
}
#endif