using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A string picked from the sorting layers.</summary>
    [AttributeSample(typeof(SortingLayerAttribute), EAttributeCategory.Pickers,
        Description = "Shows a dropdown of the sorting layers, stored by name, for anything that has to be drawn in "
            + "front of or behind something else.",
        Requirements = "A project starts with only the Default layer, so the dropdown has one entry until "
            + "more are added under Project Settings, Tags and Layers.",
        Info = "Stored by name rather than by id, so a layer renamed in the project settings breaks every "
            + "field that pointed at it.",
        Variations = new[]
        {
            "The field can be a string to store the name, or an int to store the id."
        })]
    internal sealed class SortingLayerSample : ScriptableObject
    {
        [SortingLayer]
        [Tooltip("Stores the sorting layer name.")]
        public string sortingLayer = "Default";
    }
}