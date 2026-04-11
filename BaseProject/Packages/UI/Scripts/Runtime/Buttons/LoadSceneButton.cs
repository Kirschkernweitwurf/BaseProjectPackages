using System;
using Base.AttributePackage.Scripts.Runtime;
using Base.SystemsCorePackage.SceneManagement;
using Base.SystemsCorePackage.Services;
using UnityEngine;
using Base.UtilityPackage.Logging;

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