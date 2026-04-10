using Utility;
using UnityEditor;
using UnityEngine;

namespace UI.Confirmation
{
    /// <summary>
    /// Closes the game or stops the editor when clicked.
    /// </summary>
    public class ConfirmedQuitButton : BaseConfirmationButton
    {
        protected override void OnClick() => ShowConfirmationBox();

        protected override void OnConfirm() => QuitApplication();

        private static void QuitApplication()
        {
            if (Platform.IsUnityEditor)
                EditorApplication.isPlaying = false;
            else
                Application.Quit();
        }
    }
}