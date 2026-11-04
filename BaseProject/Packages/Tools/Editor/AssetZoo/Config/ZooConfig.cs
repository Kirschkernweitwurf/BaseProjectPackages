using System.Collections.Generic;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// One saved zoo: what it shows, how that is sorted and how it looks. Make one per library or per review.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Asset Zoo/New Config", "ZC_ZooConfig")]
    internal class ZooConfig : ScriptableObject
    {
        [Tooltip("How the assets stand in the zoo.")]
        [field: SerializeField] public LayoutSettings Layout { get; private set; } = new();

        [Tooltip("The labels, the things that show scale and the floor.")]
        [field: FormerlySerializedAs("<Labels>k__BackingField")]
        [field: SerializeField] public ShowSettings Show { get; private set; } = new();

        [Tooltip("Where the art comes from and how it is sorted into groups.")]
        [field: SerializeField] public AutoGenerateSettings Generation { get; private set; } = new();

        [field: Tooltip("The scene the zoo is built into. Opens by itself on build. Leave empty for the open scene.")]
        [field: SerializeField] public SceneAsset ZooScene { get; private set; }

        [field: Tooltip("The groups of the zoo, in the order they stand in.")]
        [field: SerializeField] public List<ZooCategory> Categories { get; private set; } = new();
    }
}