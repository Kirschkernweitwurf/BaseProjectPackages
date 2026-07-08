using Base.CorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.CorePackage.Audio.OnEvent
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