using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>Custom drawing in the component header.</summary>
    [AttributeSample(typeof(HeaderDrawAttribute), EAttributeCategory.Widgets,
        Description = "Hands a rectangle in the component title bar to a method of your own, for the case where "
            + "neither a button nor a label is what the header should hold.",
        Requirements = "Drawn by the real Inspector, which is what owns the component title bar. Use the button below "
            + "to put this sample into your scene, then look at it in the Inspector.",
        Variations = new[]
        {
            "The method takes the rectangle it may draw in.",
            "Width sets how wide that rectangle is."
        })]
    internal sealed class HeaderDrawSample : MonoBehaviour
    {
        [Tooltip("The header drawing below reads this field.")]
        public float charge = 0.6f;

        /// <summary>Draws into the rectangle the header hands over.</summary>
        [HeaderDraw(Width = 60f)]
        private void DrawCharge(Rect rect)
        {
            GUI.Box(rect, GUIContent.none);

            Rect fill = new(rect.x, rect.y, rect.width * Mathf.Clamp01(charge), rect.height);

            GUI.DrawTexture(fill, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0f,
                Color.cyan, 0f, 0f);
        }
    }
}