using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Fields that offer a list of valid answers instead of free text.</summary>
    [AttributeSample("Pickers")]
    internal sealed class PickersSample : ScriptableObject
    {
        [InfoBox("Each of these knows what the project contains, so none of them can be spelled wrong.")]
        [Tag]
        [Tooltip("Dropdown of the project's tags, so a tag string cannot be misspelled.")]
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