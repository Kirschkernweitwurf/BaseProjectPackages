using System.Collections;
using UnityEngine;

namespace Components.Activation
{
    /// <summary>
    /// Activates a target GameObject after a number of frames.
    /// </summary>
    public class ActivateAfterFrames : MonoBehaviour
    {
        [SerializeField] [Min(0)] private int frames = 1;
        [SerializeField] private GameObject target;

#region Unity Callbacks
        private void Start() => StartCoroutine(ActivateRoutine());
#endregion

        private IEnumerator ActivateRoutine()
        {
            SetActive(false);

            for (int index = 0; index < frames; index++)
                yield return null;

            if (target == null)
                yield break;

            SetActive(true);
        }

        private void SetActive(bool active) => target.SetActive(active);
    }
}