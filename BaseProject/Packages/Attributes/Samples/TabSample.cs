using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>Fields split across named tabs.</summary>
    [AttributeSample(typeof(TabAttribute), EAttributeCategory.Layout,
        Description = "Puts fields under named tabs inside one group, so a component with several modes shows one of "
            + "them at a time instead of all of them at once.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "The first argument names the tab, the second names the group the tab belongs to.",
            "Several groups can exist on one component, each with its own set of tabs."
        })]
    internal sealed class TabSample : ScriptableObject
    {
        [Tab("General", "Settings")]
        [Tooltip("On the first tab of the Settings group.")]
        public string profile = "Default";

        [Tab("General", "Settings")]
        public bool enabled = true;

        [Tab("Advanced", "Settings")]
        [Tooltip("On the second tab. Click Advanced above to reach it.")]
        public float threshold = 0.5f;

        [Tab("Advanced", "Settings")]
        public int retries = 3;
    }
}