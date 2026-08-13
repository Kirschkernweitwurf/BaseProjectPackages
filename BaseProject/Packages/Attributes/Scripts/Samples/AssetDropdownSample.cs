using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A reference picked from a flat list of assets.</summary>
    [AttributeSample(typeof(AssetDropdownAttribute), EAttributeCategory.Pickers,
        Description = "Lists every asset of the field type as a dropdown instead of opening the object picker. Worth "
            + "it only while the set is small and clearly named, since a flat list of four hundred materials is worse "
            + "than the picker it replaces.",
        Requirements = "The project needs assets of that type for the list to have anything in it.",
        Variations = new[]
        {
            "A filter string narrows the search the same way the project window does.",
            "Further arguments limit the search to given folders.",
            "Dropdown is the more general answer when the options can be computed."
        })]
    internal sealed class AssetDropdownSample : ScriptableObject
    {
        [AssetDropdown]
        [Tooltip("Every material in the project, as a flat list.")]
        public Material material;
    }
}