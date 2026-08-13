using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Fields that offer a list of valid answers instead of free text.</summary>
    [AttributeSample("Pickers")]
    internal sealed class PickersSample : ScriptableObject
    {
        [Tag]
        [Tooltip("Shows a dropdown of the tags the project has. The value stays a string, but you pick "
            + "it from a list instead of typing it, so it cannot be spelled wrong.")]
        public string tag = "Untagged";

        [Layer] public int layer;

        [SortingLayer] public string sortingLayer = "Default";

        [SceneName] public string scene;

        [FolderPath] public string outputFolder;

        [FilePath("json")] public string configFile;

        [AssetDropdown] public Material material;

        [Dropdown(nameof(Presets))] public string preset = "Low";

        [Expandable] public ScriptableObject inlineAsset;

        [PreviewObject(96f)] public Texture2D preview;

        // The dropdown reads its options from here, so they can be computed rather than listed twice.
        private string[] Presets => new[]
        {
            "Low",
            "Medium",
            "High"
        };
    }
}