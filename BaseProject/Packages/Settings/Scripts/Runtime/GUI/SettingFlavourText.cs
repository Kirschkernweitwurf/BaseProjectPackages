using TMPro;
using UnityEngine;

namespace Base.SettingsPackage.GUI
{
    /// <summary>Displays the title and description of the focused setting element.</summary>
    public sealed class SettingFlavourText : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;

#region Unity Callbacks
        private void OnEnable() => SettingElement.OnHoverFlavourChanged += SetFlavourText;

        private void OnDisable() => SettingElement.OnHoverFlavourChanged -= SetFlavourText;
#endregion

        private void SetFlavourText(string title, string description)
        {
            if (titleText != null)
                titleText.text = title;

            if (descriptionText != null)
                descriptionText.text = description;
        }
    }
}