using System;
using Base.ToolsPackage.Editor.Shared;
using UnityEditor;

namespace Base.ToolsPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Immutable description of a single script that defines a custom execution order,
    /// either through the <see cref="UnityEngine.DefaultExecutionOrder"/> attribute or
    /// through the project's Script Execution Order settings.
    /// </summary>
    internal sealed class ExecutionOrderEntry
    {
        /// <summary>The script asset this entry was built from.</summary>
        internal MonoScript Script { get; }

        /// <summary>The runtime type declared by the script.</summary>
        internal Type Type { get; }

        /// <summary>Short type name, used as the display label.</summary>
        internal string Name { get; }

        /// <summary>Namespace of the type, or a dash when it has none.</summary>
        internal string Namespace { get; }

        /// <summary>Project-relative asset path of the script.</summary>
        internal string AssetPath { get; }

        /// <summary>Where the script's source lives.</summary>
        internal EAssetOrigin Origin { get; }

        /// <summary>
        /// Order that actually wins at runtime. The project value takes priority when it
        /// is non-zero; otherwise the attribute value is used.
        /// </summary>
        internal int EffectiveOrder => ProjectOrder != 0
            ? ProjectOrder
            : AttributeOrder;

        /// <summary>Order requested by the attribute, or zero when absent.</summary>
        private int AttributeOrder { get; }

        /// <summary>Order stored in the Project Settings (the script's meta file).</summary>
        private int ProjectOrder { get; }

        /// <summary>Creates an entry. <paramref name="type"/> supplies the name and namespace.</summary>
        public ExecutionOrderEntry(MonoScript script, Type type, string assetPath, EAssetOrigin origin,
            int attributeOrder, int projectOrder)
        {
            Script = script;
            Type = type;
            Name = type.Name;
            Namespace = string.IsNullOrEmpty(type.Namespace)
                ? "-"
                : type.Namespace;

            AssetPath = assetPath;
            Origin = origin;
            AttributeOrder = attributeOrder;
            ProjectOrder = projectOrder;
        }
    }
}