using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>An int picked from the project layers.</summary>
    [AttributeSample(typeof(LayerAttribute), EAttributeCategory.Pickers,
        Description = "Shows a dropdown of the project layers and stores the layer index, so the number in the field "
            + "always matches a layer that exists.",
        Requirements = "The field has to be an int, since that is what the layer APIs take.",
        Variations = new[]
        {
            "Nothing to configure."
        })]
    internal sealed class LayerSample : ScriptableObject
    {
        [Layer]
        [Tooltip("Stores the layer index, picked from the list.")]
        public int layer;
    }
}