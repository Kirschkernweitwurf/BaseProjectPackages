using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A string edited as a file path.</summary>
    [AttributeSample(typeof(FilePathAttribute), EAttributeCategory.Pickers,
        Description = "Turns a string into a file path with a browse button, optionally narrowed to one extension so "
            + "the picker cannot return something the field could not read.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "FilePath() accepts any file.",
            "FilePath(extension) narrows the picker.",
            "A second argument stores an absolute path instead of a project relative one."
        })]
    internal sealed class FilePathSample : ScriptableObject
    {
        [FilePath("json")]
        [Tooltip("Press browse to pick a json file.")]
        public string configFile;
    }
}