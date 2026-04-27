using UnityEditor;
using UnityEngine;

namespace Base.UIPackage.Confirmation
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
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            return;
#endif

            // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected
            Application.Quit();
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}