using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A string picked from the scenes in the build.</summary>
    [AttributeSample(typeof(SceneNameAttribute), EAttributeCategory.Pickers,
        Description = "Shows a dropdown of the scenes in the build settings. Stored as a string, so a scene left out "
            + "of the build is spotted here rather than at load time.",
        Requirements = "The project needs at least one scene in the build settings for the list to have anything in "
            + "it.",
        Variations = new[]
        {
            "Use SceneReference from the utility package when you would rather reference the asset than its name."
        })]
    internal sealed class SceneNameSample : ScriptableObject
    {
        [SceneName]
        [Tooltip("Picked from the scenes in the build settings.")]
        public string scene;
    }
}