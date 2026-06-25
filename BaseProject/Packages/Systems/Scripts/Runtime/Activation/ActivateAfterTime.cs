using System.Collections;
using UnityEngine;

namespace Components.Activation
{
    /// <summary>
    /// Activates a target GameObject after a delay in seconds.
    /// </summary>
    public class ActivateAfterTime : MonoBehaviour
    {
        [SerializeField] [Min(0f)] private float delay = 1f;
        [SerializeField] private bool useUnscaledTime;
        [SerializeField] private GameObject target;

#region Unity Callbacks
        private void Start() => StartCoroutine(ActivateRoutine());
#endregion

        private IEnumerator ActivateRoutine()
        {
            SetActive(false);

            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return new WaitForSeconds(delay);

            if (target == null)
                yield break;

            SetActive(true);
        }

        private void SetActive(bool active) => target.SetActive(active);
    }
}