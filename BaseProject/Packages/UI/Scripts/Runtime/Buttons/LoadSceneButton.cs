using System;
using Base.AttributePackage.References;
using Base.CorePackage.SceneManagement;
using Base.CorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UIPackage.Buttons
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
                if (ServiceLocator.TryGet(out SceneLoadingManager sceneLoader))
                    await sceneLoader.LoadSceneAsync(sceneNameToLoad);
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Ran into an error {e}, while loading the scene {sceneNameToLoad}.", this);
            }
        }
    }
}