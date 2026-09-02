using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A reference picked by clicking the object in the Scene view.</summary>
    [AttributeSample(typeof(SceneViewPickerAttribute), EAttributeCategory.Pickers,
        Description = "Adds a crosshair button beside an object reference. Pressing it arms the Scene view, "
            + "and the next click there assigns whatever was hit, which beats hunting for the right object "
            + "in a deep hierarchy.",
        Requirements = "The pick runs in the Scene view, which only draws for the selected object, so it "
            + "cannot work from this window: the sample above is not in the hierarchy and cannot be "
            + "selected. Press Create in scene, select that object, then press the crosshair.",
        Variations = new[]
        {
            "Nothing to configure.",
            "Only one field can be armed at a time. Pressing the button again, pressing Escape or "
            + "clicking on nothing cancels."
        })]
    internal sealed class SceneViewPickerSample : MonoBehaviour
    {
        [SceneViewPicker]
        [Tooltip("Press the crosshair beside the field, then click an object in the Scene view.")]
        public Transform target;

        [SceneViewPicker]
        [Tooltip("The same on a component reference, which picks that component off whatever was hit.")]
        public Collider hitCollider;
    }
}