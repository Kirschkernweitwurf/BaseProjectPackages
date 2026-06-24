using Base.SystemsCorePackage.MenuManaging;
using Base.SystemsCorePackage.MenuManaging.Modules;
using Base.SystemsCorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupport.Focus
{
    /// <summary>
    /// Defines the default focus target for a <see cref="Menu"/> and optionally restores the element
    /// that was focused last time the menu was open. Registers with the <see cref="FocusWatchdog"/>
    /// while open so focus can always be recovered.
    /// </summary>
    public sealed class ScreenFocusModule : MenuModule, IFocusContext
    {
        [Tooltip("Selectable focused when the menu opens and no remembered selection exists.")]
        [SerializeField] private Selectable defaultSelectable;

        [Tooltip("If true, reopening the menu restores the previously focused element.")]
        [SerializeField] private bool rememberLastSelected = true;

        private FocusWatchdog _focusWatchdog;
        private GameObject _lastSelected;
        private bool _isOpen;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();
            ServiceLocator.TryGet(out _focusWatchdog);
        }

        private void LateUpdate()
        {
            if (!_isOpen || !rememberLastSelected || EventSystem.current == null)
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null && current.transform.IsChildOf(transform))
                _lastSelected = current;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _focusWatchdog?.DeregisterContext(this);
        }
#endregion

        /// <summary>Selects the remembered element if it is still valid, otherwise the default.</summary>
        public void RestoreFocus()
        {
            if (EventSystem.current == null)
                return;

            Selectable target = ResolveTarget();

            if (target == null)
            {
                CustomLogger.LogWarning("ScreenFocusEntry has no valid selectable to focus.", this);
                return;
            }

            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }

        protected override void OnMenuOpened()
        {
            _isOpen = true;
            _focusWatchdog?.RegisterContext(this);
            RestoreFocus();
        }

        protected override void OnMenuClosed()
        {
            _isOpen = false;
            _focusWatchdog?.DeregisterContext(this);
        }

        private Selectable ResolveTarget()
        {
            if (!rememberLastSelected || _lastSelected == null || !_lastSelected.activeInHierarchy)
                return defaultSelectable;

            Selectable remembered = _lastSelected.GetComponent<Selectable>();

            if (remembered != null && remembered.IsInteractable())
                return remembered;

            return defaultSelectable;
        }
    }
}