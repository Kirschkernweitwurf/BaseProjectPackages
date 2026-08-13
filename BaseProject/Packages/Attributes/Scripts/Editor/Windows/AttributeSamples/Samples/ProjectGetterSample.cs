using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>References that fill themselves from the project.</summary>
    [AttributeSample("References")]
    internal sealed class ProjectGetterSample : ScriptableObject
    {
        [GetScriptableObject]
        [Tooltip("Fills itself with the first asset of the field's own type found in the project.")]
        public ProjectGetterSample config;

        [GetPrefabWithComponent(typeof(Collider))]
        [Tooltip("Fills itself with the first prefab carrying the named component. The prefab root is "
            + "assigned rather than the component.")]
        public GameObject colliderPrefab;

        [AssetOnly]
        [Tooltip("Only accepts a project asset. Dragging a scene object in is rejected.")]
        public GameObject assetOnly;

        [MustImplement(typeof(ITesterMarker))]
        [Tooltip("Only accepts objects carrying a component that implements the named interface.")]
        public GameObject mustImplement;

        [RequiredIf(nameof(assetOnly))]
        [Tooltip("Required only while another member says so, for a field that matters in one mode and "
            + "not the other.")]
        public Material conditionalMaterial;

        /// <summary>Something for the implement constraint above to ask for.</summary>
        public interface ITesterMarker { }
    }
}