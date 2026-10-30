using System.Collections.Generic;
using Base.AttributesPackage;
using Base.ControllerSupportPackage.Controller.Focus;
using Base.ServicesPackage;
using Base.ServicesPackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupportPackage.Controller.Navigation
{
    /// <summary>
    /// A self-contained navigation context. Collects the <see cref="NavigableElement"/>s beneath it,
    /// wires explicit navigation between them by proximity and exposes a default focus target. While
    /// active, it registers with the <see cref="FocusWatchdog"/> so its default can be restored when the
    /// gamepad loses its selection. Knows nothing about menus or any specific game layer.
    /// </summary>

    // The loop with FocusWatchdog is by design. The watchdog exists to notice focus leaving a group,
    // and a group is what it hands focus back to, so neither half means anything without the other.
    // Breaking it would put an event or an interface between two types that always ship together.
    public sealed class NavigableGroup : MonoBehaviour
    {
        /// <summary>Serialized name of the auto activate field, for editor tooling.</summary>
        public const string AutoActivateFieldName = nameof(autoActivate);

        /// <summary>Serialized name of the priority field, for editor tooling.</summary>
        public const string PriorityFieldName = nameof(priority);

        // Spreads the elements' states over the signature, so two different sets rarely end on the same number.
        private const int HashFactor = 31;

        private const float NeverMissed = -1f;

        // Elements are often briefly out of reach while a menu fades in or a lock screen hides them. Only a gap that
        // lasts longer than this is worth a warning.
        private const float NoTargetWarnDelay = 1f;

        [Title("Focus")]
        [Tooltip("Optional. Element selected when this group gains focus and no element is remembered. When empty "
            + "or not navigable right now, the first navigable element below the group gets focus.")]
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

        [Tooltip("If true, a direction with nothing lined up takes the nearest element on that side instead of staying "
            + "unwired, for loose layouts like a row of cards above a single button, where the outer cards sit too "
            + "far off to the side to line up with it.")]
        [SerializeField] private bool reachOffAxis;

        [Tooltip("If true, the navigation is rewired each time the group activates and whenever one of its elements "
            + "starts or stops taking the focus, like a card that turns clickable once its intro landed. For groups "
            + "whose elements are spawned or unlocked at runtime, like a row of cards.")]
        [SerializeField] private bool rebuildOnActivate;

        /// <summary>Focus priority used by the watchdog to choose between active groups.</summary>
        public EPriority Priority => priority;

        /// <summary>Whether the group activates itself in OnEnable instead of being driven externally.</summary>
        public bool AutoActivate => autoActivate;

        /// <summary>
        /// While <c>true</c>, the group keeps the focus empty instead of selecting an element, for a screen that waits
        /// for the player's first input before it shows a focus. It still owns the focus while active, so a selection
        /// on a screen behind it is cleared instead of kept.
        /// </summary>
        public bool IsFocusHeld { get; set; }

        private readonly List<NavigableElement> _elements = new();

        private bool _hasWarnedNoTarget;
        private float _noTargetSince = NeverMissed;
        private bool _isActive;
        private int _wiredSignature;
        private FocusWatchdog _focusWatchdog;
        private GameObject _lastSeenSelection;
        private GameObject _lastSelected;

#region Unity Callbacks
        private void OnEnable()
        {
            if (autoActivate)
                Activate();
        }

        private void LateUpdate()
        {
            if (!_isActive)
                return;

            RewireIfNavigabilityChanged();

            if (!rememberLastSelected
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
            _noTargetSince = NeverMissed;

            // Spawned elements only have their final place once the layout ran, so it runs first.
            if (rebuildOnActivate)
            {
                Canvas.ForceUpdateCanvases();
                Rebuild();
            }

            // Looked up on activation, not once in Awake. The watchdog is a scene service: it does not exist yet
            // when a persistent group wakes, and every scene load replaces it. It is also optional. Without it
            // the group still wires and remembers, it just loses the focus safety net.
            if (_focusWatchdog == null
                && !ServiceLocator.TryGetOptional(out _focusWatchdog))
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

        /// <summary>
        /// Selects the remembered element if it is still valid, otherwise the default. While the focus is held, it only
        /// clears a selection outside the group.
        /// </summary>
        public void RestoreFocus()
        {
            // A missing EventSystem is already reported by the watchdog, so stay quiet here.
            if (EventSystem.current == null)
                return;

            if (IsFocusHeld)
            {
                ClearOutsideSelection();
                return;
            }

            Selectable target = ResolveFocusTarget();
            if (target == null)
            {
                WarnIfStillNoTarget();
                return;
            }

            _hasWarnedNoTarget = false;
            _noTargetSince = NeverMissed;
            EventSystem.current.SetSelectedGameObject(target.gameObject);
            _lastSelected = target.gameObject;
        }

        /// <summary>True when the given object lives inside this group's hierarchy.</summary>
        public bool Contains(GameObject candidate) => candidate != null && candidate.transform.IsChildOf(transform);

        /// <summary>
        /// Recollects the child elements and rewires explicit navigation between them. Triggered from the editor
        /// tooling. At runtime only groups with Rebuild On Activate rewire themselves, so authored wiring never
        /// changes silently.
        /// </summary>
        public void Rebuild()
        {
            _elements.Clear();
            GetComponentsInChildren(true, _elements);

            NavigationBuilder.Wire(_elements, wrap, reachOffAxis);
            _wiredSignature = GetNavigabilitySignature();
        }

        // The wiring only links elements that take the focus when it runs. One that turns navigable later, like a card
        // that can't be clicked before its intro landed, would stay unreachable, so the group wires again.
        private void RewireIfNavigabilityChanged()
        {
            if (!rebuildOnActivate
                || GetNavigabilitySignature() == _wiredSignature)
                return;

            Rebuild();
        }

        // Folds in which elements take the focus right now, so any element that starts or stops doing so changes it.
        private int GetNavigabilitySignature()
        {
            int signature = _elements.Count;

            for (int index = 0; index < _elements.Count; index++)
            {
                NavigableElement element = _elements[index];
                bool isNavigable = element != null
                    && element.IsNavigable();

                signature = signature * HashFactor + (isNavigable
                    ? index + 1
                    : 0);
            }

            return signature;
        }

        private void ClearOutsideSelection()
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (current != null
                && !Contains(current))
                EventSystem.current.SetSelectedGameObject(null);
        }

        private Selectable ResolveFocusTarget()
        {
            if (TryResolveRememberedTarget(out Selectable remembered))
                return remembered;

            // A default that cannot take focus right now, like a button behind a lock screen, hands over to the
            // first element that can.
            if (defaultElement != null
                && defaultElement.IsNavigable())
                return defaultElement.Selectable;

            return FindFirstNavigable();
        }

        // The watchdog retries every frame, so the warning waits out short gaps and then comes once per activation.
        private void WarnIfStillNoTarget()
        {
            if (_noTargetSince < 0f)
                _noTargetSince = Time.unscaledTime;

            if (_hasWarnedNoTarget
                || Time.unscaledTime - _noTargetSince < NoTargetWarnDelay)
                return;

            _hasWarnedNoTarget = true;
            CustomLogger.LogWarning($"Navigable group \"{name}\" has no valid element to focus.", this);
        }

        private Selectable FindFirstNavigable()
        {
            if (_elements.Count == 0)
                GetComponentsInChildren(true, _elements);

            foreach (NavigableElement element in _elements)
            {
                if (element != null
                    && element.IsNavigable())
                    return element.Selectable;
            }

            return null;
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