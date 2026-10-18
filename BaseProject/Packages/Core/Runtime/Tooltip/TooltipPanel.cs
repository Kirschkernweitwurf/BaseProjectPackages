using Base.AttributesPackage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Draws one <see cref="TooltipEntry"/>: an optional header with icon and title, and the text below it. Texts
    /// grow with their content up to a maximum width and wrap after that, so short tooltips stay small and long
    /// ones stay readable. The <see cref="TooltipView"/> copies the panel for every further entry.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipPanel : MonoBehaviour
    {
        [Title("Settings")]
        [Min(0f)]
        [Tooltip("Widest a text gets before it wraps, in canvas units.")]
        [SerializeField] private float maxTextWidth = 320f;

        [Title("References")]
        [Required]
        [Tooltip("Shows the entry's text. Give it a sprite asset to allow icons in the text.")]
        [SerializeField] private TMP_Text body;

        [Tooltip("Shows the entry's title. Optional.")]
        [SerializeField] private TMP_Text title;

        [Tooltip("Shows the entry's icon in front of the title. Optional.")]
        [SerializeField] private Image icon;

        [Tooltip("Row that holds icon and title, hidden when the entry has neither. Optional.")]
        [SerializeField] private GameObject header;

        /// <summary>Fills the panel with <paramref name="entry"/> and hides the parts it leaves empty.</summary>
        /// <param name="entry">The content to show.</param>
        public void Show(TooltipEntry entry)
        {
            bool hasTitle = ShowText(title, entry.Title);
            bool hasIcon = ShowIcon(entry);
            ShowText(body, entry.Body);

            if (header != null)
                header.SetActive(hasTitle || hasIcon);
        }

        /// <summary>
        /// Caps the width the layout gives <paramref name="text"/>. Without a cap a long text would grow into
        /// one endless line, since a fitted layout asks every text for its unwrapped width.
        /// </summary>
        private static void LimitWidth(TMP_Text text, float maxWidth)
        {
            if (!text.TryGetComponent(out LayoutElement layout))
                layout = text.gameObject.AddComponent<LayoutElement>();

            float preferred = text.GetPreferredValues(text.text, float.PositiveInfinity, float.PositiveInfinity).x;
            layout.preferredWidth = Mathf.Min(preferred, maxWidth);
        }

        private bool ShowText(TMP_Text text, string content)
        {
            if (text == null)
                return false;

            bool hasContent = !string.IsNullOrEmpty(content);
            text.gameObject.SetActive(hasContent);

            if (!hasContent)
                return false;

            text.text = content;
            LimitWidth(text, maxTextWidth);

            return true;
        }

        private bool ShowIcon(TooltipEntry entry)
        {
            if (icon == null)
                return false;

            bool hasIcon = entry.Icon != null;
            icon.gameObject.SetActive(hasIcon);

            if (!hasIcon)
                return false;

            icon.sprite = entry.Icon;
            icon.color = entry.IconColor;

            return true;
        }
    }
}