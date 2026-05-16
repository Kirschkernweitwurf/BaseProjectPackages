using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Config
{
    /// <summary>
    /// Author-time configuration for an asset zoo. Create one or more of these as
    /// project assets via Assets &gt; Create &gt; Asset Zoo &gt; Zoo Config.
    /// </summary>
    [CreateAssetMenu(fileName = "ZooConfig", menuName = "ScriptableObjects/Asset Zoo/Zoo Config", order = 0)]
    public class ZooConfig : ScriptableObject
    {
        [Header("Settings")]
        [Tooltip("Settings related to how prefabs are arranged in space.")]
        [field: SerializeField] public LayoutSettings Layout { get; private set; } = new();

        [Tooltip("Settings related to item / category labels in the zoo.")]
        [field: SerializeField] public LabelSettings Labels { get; private set; } = new();

        [Header("Content")]
        [field: Tooltip("Categories of prefabs to show in the zoo. Each category gets its own row/section.")]
        [field: SerializeField] public List<ZooCategory> Categories { get; private set; } = new();
    }
}