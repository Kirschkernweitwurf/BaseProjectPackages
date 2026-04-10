using Systems.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Logging;

namespace Systems.Audio.OnEvent
{
    /// <summary>
    /// Plays an AudioContainer sound when the UI element is selected and stops when deselected.
    /// </summary>
    public class PlayAudioOnSelect : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private AudioContainer selectSound;

        public void OnSelect(BaseEventData eventData)
        {
            if (selectSound == null)
            {
                CustomLogger.LogWarning("No select sound set for " + gameObject.name, this);
                return;
            }

            if (ServiceLocator.TryGet(out AudioManager audioManager))
                audioManager.PlaySound(selectSound);
        }
    }
}