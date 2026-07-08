using Base.CorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.CorePackage.Audio.OnEvent
{
    /// <summary>
    /// Plays an AudioContainer sound when the UI element is clicked.
    /// </summary>
    public class PlayAudioOnClick : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private AudioContainer clickSound;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickSound == null)
            {
                CustomLogger.LogWarning("No click sound set for " + gameObject.name, this);
                return;
            }

            if (ServiceLocator.TryGet(out AudioManager audioManager))
                audioManager.PlaySound(clickSound);
        }
    }
}