#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CreateAssetMenuOverview
{
    /// <summary>
    /// Immutable description of a single <see cref="CreateAssetMenuAttribute"/> found in the
    /// project, a package or one of Unity's built-in assemblies.
    /// </summary>
    public sealed class CreateAssetEntry
    {
        /// <summary>Menu path under "Assets/Create", e.g. "Balance/Audio Settings".</summary>
        public string MenuName { get; }

        /// <summary>Top-level menu segment, used for grouping and filtering.</summary>
        public string Root { get; }

        /// <summary>ScriptableObject type that declares the attribute.</summary>
        public Type DeclaringType { get; }

        /// <summary>Short type name, used as the secondary column.</summary>
        public string TypeName { get; }

        /// <summary>Default file name created for new assets of this type.</summary>
        public string FileName { get; }

        /// <summary>Menu order that positions the item inside the Create menu.</summary>
        public int Order { get; }

        /// <summary>Where the defining script lives.</summary>
        public ECreateAssetOrigin Origin { get; }

        /// <summary>Script asset that defines the type, or null for built-in types.</summary>
        public MonoScript Script { get; }

        /// <summary>Project-relative asset path, or a dash for built-in types.</summary>
        public string AssetPath { get; }

        /// <summary>Creates an entry from a resolved CreateAssetMenu attribute.</summary>
        public CreateAssetEntry(string menuName, string fileName, Type declaringType, int order,
            ECreateAssetOrigin origin, MonoScript script, string assetPath)
        {
            // Unity falls back to the type name when no menu name is supplied.
            MenuName = string.IsNullOrEmpty(menuName)
                ? declaringType.Name
                : menuName;

            int separator = MenuName.IndexOf('/');
            Root = separator >= 0
                ? MenuName[..separator]
                : MenuName;

            DeclaringType = declaringType;
            TypeName = declaringType.Name;

            // Unity falls back to "New {type}" when no file name is supplied.
            FileName = string.IsNullOrEmpty(fileName)
                ? $"New {declaringType.Name}"
                : fileName;

            Order = order;
            Origin = origin;
            Script = script;
            AssetPath = string.IsNullOrEmpty(assetPath)
                ? "-"
                : assetPath;
        }
    }
}
#endif