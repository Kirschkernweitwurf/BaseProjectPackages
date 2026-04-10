using System;
using System.Threading.Tasks;
using Attributes;
using SceneManagement;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace UI.Confirmation
{
    /// <summary>
    /// Closes the game or stops the editor when clicked.
    /// </summary>
    public class ConfirmedLoadSceneButton : BaseConfirmationButton
    {
        [SceneName] [SerializeField] private string sceneNameToLoad;

        protected override void OnClick() => ShowConfirmationBox();

        protected override void OnConfirm() => _ = LoadScene();

        /// <summary>
        /// Unloads all scenes and additively and asynchronously loads a desired scene.
        /// </summary>
        private async Task LoadScene()
        {
            try
            {
                if (!ServiceLocator.TryGet(out SceneLoadingManager sceneLoadingManager))
                    return;

                await sceneLoadingManager.LoadSceneAsync(sceneNameToLoad);
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Ran into an error {e}, while loading the scene {sceneNameToLoad}.", this);
            }
        }
    }
}