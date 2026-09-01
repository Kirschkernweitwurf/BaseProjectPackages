using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A reference filled from the project.</summary>
    [AttributeSample(typeof(GetScriptableObjectAttribute), EAttributeCategory.References,
        Description = "Fills itself with the first asset of the field type found in the project, for the single "
            + "settings object a component always points at.",
        Requirements = "An asset of that type has to exist in the project. With none, or with several, nothing useful "
            + "happens.",
        Variations = new[]
        {
            "Nothing to configure. It fills only while the field is empty, so an explicit assignment is never "
            + "overwritten."
        })]
    internal sealed class GetScriptableObjectSample : ScriptableObject
    {
        [GetScriptableObject]
        [Tooltip("Fills itself with the first asset of this type in the project.")]
        public ScriptableObject config;
    }
}