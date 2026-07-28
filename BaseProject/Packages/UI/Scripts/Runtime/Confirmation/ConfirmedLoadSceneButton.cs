using Base.AttributePackage;
using Base.UIPackage.Utility;
using UnityEngine;

namespace Base.UIPackage.Confirmation
{
    /// <summary>
    /// Loads the given scene after the player confirms the prompt.
    /// </summary>
    public sealed class ConfirmedLoadSceneButton : BaseConfirmationButton
    {
        [SceneName] [NotNullOrEmpty] [SerializeField] private string sceneNameToLoad;

        protected override void OnClick() => ShowConfirmationBox();

        protected override void OnConfirm() => _ = SceneLoader.LoadSceneAsync(sceneNameToLoad, this);
    }
}