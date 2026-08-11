#pragma warning disable 414
using Base.UtilityPackage.Serialization;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Showcase
{
    /// <summary>
    /// Target for the searching auto-getters in the showcase. It exists so those fields have a type that
    /// occurs exactly once in a project rather than a bare ScriptableObject, which would resolve to
    /// whichever asset the database happened to return first and demonstrate nothing.
    /// </summary>
    /// <remarks>
    /// Create one through Assets, Create, Base, Showcase Config. Without it the two getters stay empty,
    /// which is itself worth seeing: they fail by finding nothing rather than by complaining.
    /// </remarks>
    [CreateAssetMenu(fileName = "ShowcaseConfig", menuName = "Base/Showcase Config")]
    public sealed class ShowcaseConfigAsset : ScriptableObject
    {
        [Title("Config", "#8CD5E9")]
        [InfoBox("Shown when this asset is expanded inline from the showcase, which proves the embedded "
            + "inspector runs the whole pipeline rather than Unity's default drawing.")]
        [SerializeField] private string label = "Showcase config";

        [Required] [SerializeField] private Material referenceMaterial;

        [ProgressBar(100f, EColor.Green)]
        [SerializeField] private float completion = 40f;

        [SerializeField] private TypeReference handler = new();
    }
}
#pragma warning restore 414