using Base.AttributesPackage;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Opens a specified URL in the default web browser when the button is clicked.
    /// </summary>
    public sealed class OpenLinkOnClick : CustomButton
    {
        [Tooltip("The URL that gets opened in the default browser.")]
        [NotNullOrEmpty] [SerializeField] private string url;

        protected override void OnClick() => Application.OpenURL(url);
    }
}