using Base.AttributesPackage;
using Base.CorePackage.MenuManaging;
using Base.ServicesPackage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Navigation
{
    /// <summary>
    /// Presses its button when the player goes back, so Back leaves a screen the way a click on that button does, with
    /// its animation, like the Play app that resumes from the pause homescreen. Takes Back through the menu manager:
    /// it only acts while its screen is on top and a step claimed later on the same screen, like an open page, goes
    /// first. While the button can't be pressed, Back does nothing on this screen. A button that throws something away,
    /// like Skip on a reward pick, takes the focus on the first Back and is only pressed by a second one, so a reflex
    /// press never costs the player anything.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class BackButton : MonoBehaviour
    {
        [Tooltip("The button Back presses. Auto-assigned from the same GameObject.")]
        [Required] [GetComponent] [SerializeField] private Button button;

        [Tooltip("Back first moves the focus to the button and presses it only while it has the focus. For a button "
            + "that throws something away, like Skip on a reward pick.")]
        [SerializeField] private bool isFocusedFirst;

        private MenuManager _menus;

#region Unity Callbacks
        // The first enable comes before Start, which claims once the manager is known.
        private void OnEnable()
        {
            if (_menus != null)
                Claim();
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _menus))
            {
                enabled = false;
                return;
            }

            Claim();
        }

        private void OnDisable()
        {
            if (_menus != null)
                _menus.ReleaseBack(this);
        }
#endregion

        private void Claim() => _menus.ClaimBack(this, onBack: Press);

        private void Press()
        {
            if (!button.IsActive()
                || !button.IsInteractable())
                return;

            if (isFocusedFirst
                && !HasFocus())
            {
                Focus();
                return;
            }

            button.onClick.Invoke();
        }

        private bool HasFocus() => EventSystem.current != null
            && EventSystem.current.currentSelectedGameObject == button.gameObject;

        // Clearing first makes the button light up even when it held the selection without showing it.
        private void Focus()
        {
            if (EventSystem.current == null)
                return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}