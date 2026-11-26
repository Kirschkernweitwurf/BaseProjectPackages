using System.Collections.Generic;
using Base.AttributesPackage;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Draws one <see cref="TooltipLink"/> beside the tooltip's main card, as a card of its own: its picture on the
    /// left, then its panels. The <see cref="TooltipView"/> copies the group for every further link. Where a row of
    /// links is too wide for either side of the target, the view stacks them: the panels go below the picture.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TooltipLinkGroup : MonoBehaviour
    {
        [Title("References")]
        [Required]
        [Tooltip("Draws the link's picture, like a card face. Hidden when it can't draw the subject.")]
        [SerializeField] private TooltipSubjectView subject;

        [Required]
        [Tooltip("Column in the row that holds the picture, and the panels below it while the link is stacked.")]
        [SerializeField] private HorizontalOrVerticalLayoutGroup column;

        [Required]
        [Tooltip("Holds the link's panels beside the picture, copies of the view's panel template. Hidden when the "
            + "link has none.")]
        [SerializeField] private Transform card;

        private readonly List<TooltipPanel> _panels = new();

#region Unity Callbacks
        // The view copies a group that already made panels, so a copy starts with the panels it brought along.
        private void Awake() => card.GetComponentsInChildren(true, _panels);
#endregion

        /// <summary>
        /// Shows <paramref name="link"/> with copies of <paramref name="template"/> for its panels and hides the
        /// panels left over from a bigger link before.
        /// </summary>
        /// <returns><c>true</c> when the link showed anything.</returns>
        internal bool Show(TooltipLink link, TooltipPanel template)
        {
            bool hasPicture = link.Subject != null
                && subject.TryShow(link.Subject);
            subject.gameObject.SetActive(hasPicture);

            IReadOnlyList<TooltipEntry> entries = link.Entries;
            card.gameObject.SetActive(entries.Count > 0);

            for (int i = 0; i < entries.Count; i++)
            {
                TooltipPanel panel = PanelAt(template, i);
                panel.gameObject.SetActive(true);
                panel.Show(entries[i]);
                panel.SetSeparated(i > 0
                    && !string.IsNullOrEmpty(entries[i].Title));
            }

            for (int i = entries.Count; i < _panels.Count; i++)
                _panels[i].gameObject.SetActive(false);

            return hasPicture || entries.Count > 0;
        }

        /// <summary>
        /// Puts the panels below the picture while <paramref name="isStacked"/>, which takes less width, else beside
        /// it.
        /// </summary>
        /// <returns><c>true</c> when the panels moved.</returns>
        internal bool SetStacked(bool isStacked)
        {
            Transform parent = isStacked
                ? column.transform
                : transform;
            if (card.parent == parent)
                return false;

            card.SetParent(parent, false);
            card.SetAsLastSibling();
            return true;
        }

        // Copied from the template into the card the first time a panel is needed and reused after that.
        private TooltipPanel PanelAt(TooltipPanel template, int index)
        {
            while (_panels.Count <= index)
                _panels.Add(Instantiate(template, card));

            return _panels[index];
        }
    }
}