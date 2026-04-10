using Systems.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Logging;

namespace Systems.Audio.OnEvent
{
    /// <summary>
    /// Plays an AudioContainer sound when the UI element is hovered over.
    /// </summary>
    public class PlayAudioOnHover : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private AudioContainer hoverSound;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverSound == null)
            {
                CustomLogger.LogWarning("No hover sound set for " + gameObject.name, this);
                return;
            }

            if (ServiceLocator.TryGet(out AudioManager audioManager))
                audioManager.PlaySound(hoverSound);
        }
    }
}