#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Base.UIPackage.Confirmation
{
    /// <summary>
    /// Closes the game or stops the editor after the player confirms the prompt.
    /// </summary>
    public class ConfirmedQuitButton : BaseConfirmationButton
    {
        protected override void OnClick() => ShowConfirmationBox();

        protected override void OnConfirm() => QuitApplication();

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
