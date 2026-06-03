using System;
using UnityEditor;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Immutable description of a single script that defines a custom execution order,
    /// either through the <see cref="UnityEngine.DefaultExecutionOrder"/> attribute or
    /// through the project's Script Execution Order settings.
    /// </summary>
    public sealed class ExecutionOrderEntry
    {
        /// <summary>The script asset this entry was built from.</summary>
        public MonoScript Script { get; }

        /// <summary>The runtime type declared by the script.</summary>
        public Type Type { get; }

        /// <summary>Short type name, used as the display label.</summary>
        public string Name { get; }

        /// <summary>Namespace of the type, or a dash when it has none.</summary>
        public string Namespace { get; }

        /// <summary>Project-relative asset path of the script.</summary>
        public string AssetPath { get; }

        /// <summary>Where the script's source lives.</summary>
        public ScriptOrigin Origin { get; }

        /// <summary>Order requested by the attribute, or zero when absent.</summary>
        public int AttributeOrder { get; }

        /// <summary>Order stored in the Project Settings (the script's meta file).</summary>
        public int ProjectOrder { get; }

        /// <summary>
        /// Order that actually wins at runtime. The project value takes priority when it
        /// is non-zero; otherwise the attribute value is used.
        /// </summary>
        public int EffectiveOrder => ProjectOrder != 0 ? ProjectOrder : AttributeOrder;

        /// <summary>Creates an entry. <paramref name="type"/> supplies the name and namespace.</summary>
        public ExecutionOrderEntry(MonoScript script, Type type, string assetPath, ScriptOrigin origin, int attributeOrder, int projectOrder)
        {
            Script = script;
            Type = type;
            Name = type.Name;
            Namespace = string.IsNullOrEmpty(type.Namespace) ? "-" : type.Namespace;
            AssetPath = assetPath;
            Origin = origin;
            AttributeOrder = attributeOrder;
            ProjectOrder = projectOrder;
        }
    }
}