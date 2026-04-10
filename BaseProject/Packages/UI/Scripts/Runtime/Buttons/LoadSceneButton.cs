using System;
using Attributes;
using SceneManagement;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

namespace UI.Buttons
{
    /// <summary>
    /// Unloads all scenes and additively and asynchronously loads a desired scene.
    /// </summary>
    public class LoadSceneButton : CustomButton
    {
        [SceneName] [SerializeField] private string sceneNameToLoad;

        protected override async void OnClick()
        {
            try
            {
                if(ServiceLocator.TryGet(out SceneLoadingManager sceneLoader))
                    await sceneLoader.LoadSceneAsync(sceneNameToLoad);
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Ran into an error {e}, while loading the scene {sceneNameToLoad}.", this);
            }
        }
    }
}