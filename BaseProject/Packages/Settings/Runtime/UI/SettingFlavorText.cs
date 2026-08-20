using TMPro;
using UnityEngine;

namespace Base.SettingsPackage.UI
{
    /// <summary>Displays the title and description of the focused setting element.</summary>
    public sealed class SettingFlavorText : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

#region Unity Callbacks
        private void OnEnable() => SettingElement.OnHoverFlavorChanged += SetFlavorText;

        private void OnDisable() => SettingElement.OnHoverFlavorChanged -= SetFlavorText;
#endregion

        private void SetFlavorText(string title, string description)
        {
            if (titleText != null)
                titleText.text = title;

            if (descriptionText != null)
                descriptionText.text = description;
        }
    }
}