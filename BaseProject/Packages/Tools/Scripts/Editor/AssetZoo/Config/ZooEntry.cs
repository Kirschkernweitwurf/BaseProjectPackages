using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// One prefab in the zoo, with an optional display name override.
    /// </summary>
    [Serializable]
    public class ZooEntry
    {
        [field: Tooltip("Prefab to show in the zoo.")]
        [field: SerializeField] public GameObject Prefab { get; private set; }

        [field: Tooltip("Optional. If empty, the prefab name is used as the label.")]
        [field: SerializeField] public string LabelOverride { get; private set; }
    }
}