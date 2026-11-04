using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Base.ToolsPackage.Editor.AssetZoo.Config
{
    /// <summary>
    /// What stands around the art: the labels in the Scene view, the things that show scale and the floor.
    /// </summary>
    [Serializable]
    internal class ShowSettings
    {
        [field: Tooltip("Draw group names and zones in the Scene view.")]
        [field: FormerlySerializedAs("<ShowCategoryLabels>k__BackingField")]
        [field: SerializeField] public bool HasGroupNames { get; private set; } = true;

        [field: Tooltip("Draw asset names in the Scene view. They appear as you get closer and never overlap.")]
        [field: FormerlySerializedAs("<ShowItemLabels>k__BackingField")]
        [field: SerializeField] public bool HasItemNames { get; private set; } = true;

        [field: Tooltip("Add the size and triangle count to the asset names. Hovering a name always shows them.")]
        [field: SerializeField] public bool HasSizes { get; private set; }

        [field: Tooltip("Draw a ruler in meters at the start of every group.")]
        [field: SerializeField] public bool HasRuler { get; private set; } = true;

        [field: Tooltip("Stands at the start of every group, so everything can be judged against it. A person "
            + "works best. Leave empty for none.")]
        [field: SerializeField] public GameObject ScaleFigure { get; private set; }

        [field: Tooltip("Build a floor that fits the zoo, so the art has ground and shadows to stand on.")]
        [field: SerializeField] public bool HasFloor { get; private set; } = true;

        [field: Tooltip("The material of the floor. Leave empty for the render pipeline's default.")]
        [field: SerializeField] public Material FloorMaterial { get; private set; }
    }
}