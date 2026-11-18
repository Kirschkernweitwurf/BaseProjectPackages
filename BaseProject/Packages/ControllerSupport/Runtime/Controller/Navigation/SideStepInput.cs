using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Base.ControllerSupportPackage.Controller.Navigation
{
    /// <summary>
    /// Turns left and right on the focused element into steps, like browsing a carousel while the focus stays on its
    /// button. Up and down keep moving the focus as usual. The element needs no left or right neighbors or the
    /// focus would move away at the same time.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SideStepInput : MonoBehaviour, IMoveHandler
    {
        /// <summary>Raised with -1 for left and 1 for right.</summary>
        public event Action<int> Stepped;

#region Unity Callbacks
        private void OnDestroy() => Stepped = null;
#endregion

        /// <summary>Reports a left or right move while this element has the focus.</summary>
        public void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    Stepped?.Invoke(-1);
                    break;
                case MoveDirection.Right:
                    Stepped?.Invoke(1);
                    break;
            }
        }
    }
}