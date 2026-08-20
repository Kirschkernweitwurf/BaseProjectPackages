using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A string edited as a folder path.</summary>
    [AttributeSample(typeof(FolderPathAttribute), EAttributeCategory.Pickers,
        Description = "Turns a string into a folder path with a browse button. Project relative by default, so the "
            + "value survives moving the project.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "FolderPath(true) stores an absolute path instead."
        })]
    internal sealed class FolderPathSample : ScriptableObject
    {
        [FolderPath]
        [Tooltip("Press browse to pick a folder.")]
        public string outputFolder;
    }
}