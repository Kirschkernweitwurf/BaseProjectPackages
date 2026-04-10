using UnityEngine;
using Utility.Logging;

namespace UI.Buttons
{
    /// <summary>
    /// Opens a specified URL in the default web browser when the button is clicked.
    /// </summary>
    public class OpenLinkOnClick : CustomButton
    {
        [SerializeField] private string url = "https://example.com";

        protected override void OnClick()
        {
            if (string.IsNullOrEmpty(url))
            {
                CustomLogger.LogError("URL is null or empty. Cannot open link.", this);
                return;
            }

            Application.OpenURL(url);
        }
    }
}