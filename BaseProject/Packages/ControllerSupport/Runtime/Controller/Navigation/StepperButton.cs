using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Navigation
{
    /// <summary>
    /// A button that steps a value with left and right instead of moving the focus, like a settings row, so it can
    /// sit in a <c>NavigableGroup</c> that wires its sides to other buttons. Up and down move the focus as usual and a
    /// press still clicks. Unlike <see cref="SideStepInput"/> it needs no layout without side neighbors.
    /// </summary>
    public sealed class StepperButton : Button
    {
        private const int Back = -1;
        private const int Forward = 1;

        /// <summary>Raised with -1 for left and 1 for right.</summary>
        public event Action<int> Stepped;

#region Unity Callbacks
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Stepped = null;
        }
#endregion

        /// <summary>Steps on left and right; up and down move the focus.</summary>
        public override void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    Stepped?.Invoke(Back);
                    break;
                case MoveDirection.Right:
                    Stepped?.Invoke(Forward);
                    break;
                default:
                    base.OnMove(eventData);
                    break;
            }
        }
    }
}