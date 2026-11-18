using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Focus
{
    /// <summary>
    /// Moves the focus to the next step of a screen once it opened, like the Choose button of a detail page. A menu
    /// hands the focus to its default as it opens and blocks its buttons while it fades in, so the request waits until
    /// the target can take the focus and then wins.
    /// </summary>
    public static class FocusRequest
    {
        // Longer than any menu fade; a target that still can't take the focus by then never will.
        private const float MaxWaitSeconds = 1f;

        /// <summary>
        /// Selects <paramref name="target"/> as soon as it can take the focus, from the next frame on. Clearing first
        /// makes it light up even when it held the selection already. Gives up quietly after a second.
        /// </summary>
        public static async Awaitable SelectWhenReadyAsync(Selectable target, CancellationToken token)
        {
            try
            {
                float giveUp = Time.unscaledTime + MaxWaitSeconds;
                do
                {
                    await Awaitable.NextFrameAsync(token);

                    if (target == null)
                        return;
                }
                while (!CanTakeFocus(target)
                    && Time.unscaledTime < giveUp);

                if (!CanTakeFocus(target)
                    || EventSystem.current == null)
                    return;

                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(target.gameObject);
            }
            catch (OperationCanceledException)
            {
                // The screen went away before the target could take the focus.
            }
        }

        private static bool CanTakeFocus(Selectable target) => target.gameObject.activeInHierarchy
            && target.IsInteractable();
    }
}