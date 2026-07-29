using System.Collections.Generic;
using Base.AttributePackage;
using Base.ControllerSupport.Controller.Focus;
using Base.CorePackage.Services;
using Base.CorePackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupport.Controller.Navigation
{
    /// <summary>
    /// A self-contained navigation context. Collects the <see cref="NavigableElement"/>s beneath it,
    /// wires explicit navigation between them by proximity and exposes a default focus target. While
    /// active, it registers with the <see cref="FocusWatchdog"/> so its default can be restored when the
    /// gamepad loses its selection. Knows nothing about menus or any specific game layer.
    /// </summary>
    public sealed class NavigableGroup : MonoBehaviour
    {
        /// <summary>Serialized name of the auto activate field, for editor tooling.</summary>
        public const string AutoActivateFieldName = nameof(autoActivate);

        /// <summary>Serialized name of the priority field, for editor tooling.</summary>
        public const string PriorityFieldName = nameof(priority);

        [Title("Focus")]
        [Tooltip("Element selected when this group gains focus and no element is remembered.")]
        [Required]
        [SerializeField] private NavigableElement defaultElement;

        [Tooltip("Higher priority groups win focus restoration when several are active at once.")]
        [SerializeField] private EPriority priority;

        [Tooltip("If true, the group activates itself when its GameObject is enabled at runtime.")]
        [SerializeField] private bool autoActivate = true;

        [Tooltip("If true, focus returns to the element used last instead of the default.")]
        [SerializeField] private bool rememberLastSelected = true;

        [Title("Wiring")]
        [Tooltip("If true, navigation loops around the edges of the group.")]
        [SerializeField] private bool wrap;

        /// <summary>Focus priority used by the watchdog to choose between active groups.</summary>
        public EPriority Priority => priority;

        /// <summary>Whether the group activates itself in OnEnable instead of being driven externally.</summary>
        public bool AutoActivate => autoActivate;

        private readonly List<NavigableElement> _elements = new();

        private bool _hasWarnedNoTarget;
        private bool _isActive;
        private FocusWatchdog _focusWatchdog;
        private GameObject _lastSeenSelection;
        private GameObject _lastSelected;

#region Unity Callbacks
        // The watchdog is optional: without it the group still wires and remembers, it just loses the
        // focus safety net. Resolved once here so activation does not retry the lookup every time.
        private void Awake() => ServiceLocator.TryGet(out _focusWatchdog);

        private void OnEnable()
        {
            if (autoActivate)
                Activate();
        }

        private void LateUpdate()
        {
            if (!_isActive
                || !rememberLastSelected
                || EventSystem.current == null)
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (current == _lastSeenSelection)
                return;

            _lastSeenSelection = current;

            if (Contains(current))
                _lastSelected = current;
        }

        private void OnDisable() => Deactivate();
#endregion

        /// <summary>Registers the group with the watchdog so its default can be restored on focus loss.</summary>
        public void Activate()
        {
            if (_isActive)
                return;

            _isActive = true;
            _hasWarnedNoTarget = false;

            if (_focusWatchdog == null)
                return;

            _focusWatchdog.RegisterGroup(this);
        }

        /// <summary>Removes the group from the watchdog. Its elements stop being focus targets.</summary>
        public void Deactivate()
        {
            if (!_isActive)
                return;

            _isActive = false;

            if (_focusWatchdog == null)
                return;

            _focusWatchdog.DeregisterGroup(this);
        }

        /// <summary>Selects the remembered element if it is still valid, otherwise the default.</summary>
        public void RestoreFocus()
        {
            // A missing EventSystem is already reported by the watchdog, so stay quiet here.
            if (EventSystem.current == null)
                return;

            Selectable target = ResolveFocusTarget();
            if (target == null)
            {
                // The watchdog retries every frame, so warn only once per activation instead of spamming.
                if (_hasWarnedNoTarget)
                    return;

                _hasWarnedNoTarget = true;
                CustomLogger.LogWarning($"Navigable group \"{name}\" has no valid element to focus.", this);

                return;
            }

            _hasWarnedNoTarget = false;
            EventSystem.current.SetSelectedGameObject(target.gameObject);
            _lastSelected = target.gameObject;
        }

        /// <summary>True when the given object lives inside this group's hierarchy.</summary>
        public bool Contains(GameObject candidate) => candidate != null && candidate.transform.IsChildOf(transform);

        /// <summary>
        /// Recollects the child elements and rewires explicit navigation between them. Triggered from the
        /// editor tooling, never automatically, so wiring never changes silently.
        /// </summary>
        public void Rebuild()
        {
            _elements.Clear();
            GetComponentsInChildren(true, _elements);

            NavigationBuilder.Wire(_elements, wrap);
        }

        private Selectable ResolveFocusTarget()
        {
            if (TryResolveRememberedTarget(out Selectable remembered))
                return remembered;

            return defaultElement.IsNavigable()
                ? defaultElement.Selectable
                : null;
        }

        private bool TryResolveRememberedTarget(out Selectable remembered)
        {
            remembered = null;

            if (!rememberLastSelected
                || _lastSelected == null
                || !_lastSelected.activeInHierarchy)
                return false;

            Selectable candidate = _lastSelected.GetComponent<Selectable>();

            if (candidate == null
                || !candidate.IsInteractable())
                return false;

            remembered = candidate;
            return true;
        }
    }
}