using System.Collections.Generic;
using Base.AttributesPackage;
using Base.UtilityPackage.Menus;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// Author-time configuration for an asset zoo. Create one or more of these as
    /// project assets via Assets &gt; Create &gt; Asset Zoo &gt; Zoo Config.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Asset Zoo/New Config", "ZC_ZooConfig")]
    internal class ZooConfig : ScriptableObject
    {
        [field: Title("Settings")]
        [Tooltip("Settings related to how prefabs are arranged in space.")]
        [field: SerializeField] public LayoutSettings Layout { get; private set; } = new();

        [Tooltip("Settings related to item / category labels in the zoo.")]
        [field: SerializeField] public LabelSettings Labels { get; private set; } = new();

        [Tooltip("Settings related to filling the categories automatically from asset names.")]
        [field: SerializeField] public AutoGenerateSettings Generation { get; private set; } = new();

        [field: Title("Content")]
        [field: Tooltip("Categories of prefabs to show in the zoo. Each category gets its own row/section.")]
        [field: SerializeField] public List<ZooCategory> Categories { get; private set; } = new();
    }
}