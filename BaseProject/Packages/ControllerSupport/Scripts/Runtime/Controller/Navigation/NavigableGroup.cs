using System.Collections.Generic;
using Base.ControllerSupport.Controller.Focus;
using Base.CorePackage.Services;
using Base.CorePackage.Tracking;
using Base.UtilityPackage.Logging;
using UnityEditor;
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
        [Header("Focus")]

        [Tooltip("Element selected when this group gains focus and no element is remembered.")]
        [SerializeField] private NavigableElement defaultElement;

        [Tooltip("Higher priority groups win focus restoration when several are active at once.")]
        [SerializeField] private EPriority priority;

        [Tooltip("If true, the group activates itself when its GameObject is enabled at runtime.")]
        [SerializeField] private bool autoActivate = true;

        [Tooltip("If true, focus returns to the element used last instead of the default.")]
        [SerializeField] private bool rememberLastSelected = true;

        [Header("Wiring")]

        [Tooltip("If true, navigation loops around the edges of the group.")]
        [SerializeField] private bool wrap;

        /// <summary>Focus priority used by the watchdog to choose between active groups.</summary>
        public EPriority Priority => priority;

        private readonly List<NavigableElement> _elements = new();
        private readonly List<Selectable> _validationBuffer = new();

        private bool _isActive;

        private FocusWatchdog _focusWatchdog;
        private GameObject _lastSelected;

#region Unity Callbacks
        private void OnEnable()
        {
            if (autoActivate)
                Activate();
        }

        private void LateUpdate()
        {
            if (!_isActive || !rememberLastSelected || EventSystem.current == null)
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current != null && current.transform.IsChildOf(transform))
                _lastSelected = current;
        }

        private void OnDisable() => Deactivate();
#endregion

        /// <summary>Registers the group with the watchdog so its default can be restored on focus loss.</summary>
        public void Activate()
        {
            if (_isActive)
                return;

            if (_focusWatchdog == null)
                ServiceLocator.TryGet(out _focusWatchdog);

            _isActive = true;
            _focusWatchdog?.RegisterGroup(this);
        }

        /// <summary>Removes the group from the watchdog. Its elements stop being focus targets.</summary>
        public void Deactivate()
        {
            if (!_isActive)
                return;

            _isActive = false;
            _focusWatchdog?.DeregisterGroup(this);
        }

        /// <summary>Selects the remembered element if it is still valid, otherwise the default.</summary>
        public void RestoreFocus()
        {
            if (EventSystem.current == null)
                return;

            Selectable target = ResolveFocusTarget();

            if (target == null)
            {
                CustomLogger.LogWarning($"Navigable group \"{name}\" has no valid element to focus.", this);
                return;
            }

            EventSystem.current.SetSelectedGameObject(target.gameObject);
            _lastSelected = target.gameObject;
        }

        /// <summary>True when the given object lives inside this group's hierarchy.</summary>
        public bool Contains(GameObject candidate) => candidate != null && candidate.transform.IsChildOf(transform);

        /// <summary>Recollects child elements and rewires explicit navigation. Validates in the editor.</summary>
        [ContextMenu("Rebuild Navigation")]
        public void Rebuild()
        {
#if UNITY_EDITOR

            // Add any missing element first so newly added ones are wired in this same pass.
            if (!Application.isPlaying)
                NavigationValidator.Validate(transform, _validationBuffer);
#endif

            _elements.Clear();
            GetComponentsInChildren(true, _elements);

            NavigationBuilder.Wire(_elements, wrap);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                MarkElementsDirty();
#endif
        }

        private Selectable ResolveFocusTarget()
        {
            if (rememberLastSelected && _lastSelected != null && _lastSelected.activeInHierarchy)
            {
                Selectable remembered = _lastSelected.GetComponent<Selectable>();

                if (remembered != null && remembered.IsInteractable())
                    return remembered;
            }

            return defaultElement != null && defaultElement.IsNavigable()
                ? defaultElement.Selectable
                : null;
        }

#if UNITY_EDITOR
        private void MarkElementsDirty()
        {
            foreach (NavigableElement element in _elements)
            {
                if (element != null && element.Selectable != null)
                    EditorUtility.SetDirty(element.Selectable);
            }
        }
#endif
    }
}