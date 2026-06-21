using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Base.ControllerSupport.Navigation
{
    /// <summary>
    /// Traps focus inside a panel so the stick cannot wander into elements behind a modal. While
    /// enabled, any selection outside this hierarchy is pulled back to the last valid element inside
    /// it. Composes with <see cref="GridNavigationBuilder"/>, which wires the panel's internal moves.
    /// </summary>
    public sealed class NavigationGroup : MonoBehaviour
    {
        [Tooltip("Selectable focused when the group is enabled and as the fallback re-entry point.")]
        [SerializeField] private Selectable defaultSelectable;

        [Tooltip("If true, the default is selected automatically when the group is enabled.")]
        [SerializeField] private bool selectDefaultOnEnable = true;

        private GameObject _lastInside;

#region Unity Callbacks
        private void LateUpdate()
        {
            if (EventSystem.current == null)
                return;

            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (IsInside(current))
            {
                _lastInside = current;
                return;
            }

            GameObject target = ResolveReentryTarget();

            if (target != null)
                EventSystem.current.SetSelectedGameObject(target);
        }

        private void OnEnable()
        {
            if (selectDefaultOnEnable)
                SelectDefault();
        }
#endregion

        /// <summary>Selects the configured default element and records it as the active focus.</summary>
        public void SelectDefault()
        {
            if (EventSystem.current == null || defaultSelectable == null)
                return;

            EventSystem.current.SetSelectedGameObject(defaultSelectable.gameObject);
            _lastInside = defaultSelectable.gameObject;
        }

        private bool IsInside(GameObject candidate)
        {
            if (candidate == null || !candidate.activeInHierarchy)
                return false;

            return candidate.transform.IsChildOf(transform);
        }

        private GameObject ResolveReentryTarget()
        {
            if (_lastInside != null && _lastInside.activeInHierarchy)
                return _lastInside;

            return defaultSelectable != null
                ? defaultSelectable.gameObject
                : null;
        }
    }
}