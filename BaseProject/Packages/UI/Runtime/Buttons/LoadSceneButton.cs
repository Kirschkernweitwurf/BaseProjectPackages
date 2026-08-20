using Base.AttributePackage;
using Base.UIPackage.Utility;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Unloads all scenes and additively and asynchronously loads a desired scene.
    /// </summary>
    public sealed class LoadSceneButton : CustomButton
    {
        [SceneName] [NotNullOrEmpty] [SerializeField] private string sceneNameToLoad;

        protected override void OnClick() => _ = SceneLoader.LoadSceneAsync(sceneNameToLoad, this);
    }
}