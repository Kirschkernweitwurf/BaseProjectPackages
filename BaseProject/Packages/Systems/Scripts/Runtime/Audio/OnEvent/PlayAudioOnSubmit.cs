using Systems.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Logging;

namespace Systems.Audio.OnEvent
{
    /// <summary>
    /// Plays an AudioContainer sound when the UI element is activated (click, keyboard or gamepad).
    /// </summary>
    public class PlayAudioOnSubmit : MonoBehaviour, ISubmitHandler
    {
        [SerializeField] private AudioContainer submitSound;

        public void OnSubmit(BaseEventData eventData)
        {
            if (submitSound == null)
            {
                CustomLogger.LogWarning("No submit sound set for " + gameObject.name, this);
                return;
            }

            if (ServiceLocator.TryGet(out AudioManager audioManager))
                audioManager.PlaySound(submitSound);
        }
    }
}