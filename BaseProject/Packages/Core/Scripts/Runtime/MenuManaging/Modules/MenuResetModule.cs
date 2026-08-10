using System.Collections.Generic;
using Base.CorePackage.Contracts;
using Object = UnityEngine.Object;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Resets stateful children that implement <see cref="IMenuResettable"/> whenever the owning menu
    /// closes, so it opens fresh next time. The menu's own content root is skipped, since the menu
    /// drives that animation itself.
    /// </summary>
    public sealed class MenuResetModule : MenuModule
    {
        private IMenuResettable[] _resettables;

#region Unity Callbacks
        private void Awake() => Recache();
#endregion

        protected override void OnMenuClosed()
        {
            foreach (IMenuResettable resettable in _resettables)
            {
                // Children can be destroyed while the menu lives on, so skip anything that is gone.
                if (resettable is Object unityObject && unityObject == null)
                    continue;

                resettable.ResetState();
            }
        }

        /// <summary>
        /// Collects the resettable children once. They are cached because a menu can close often and
        /// the hierarchy does not change between closes.
        /// </summary>
        private void Recache()
        {
            IMenuResettable[] found = OwnerMenu.GetComponentsInChildren<IMenuResettable>(includeInactive: true);
            List<IMenuResettable> filtered = new(found.Length);

            foreach (IMenuResettable resettable in found)
            {
                // Skip the menu's content root: its open/close animation is driven by the menu itself.
                if (ReferenceEquals(resettable, OwnerMenu.ContentRoot))
                    continue;

                filtered.Add(resettable);
            }

            _resettables = filtered.ToArray();
        }
    }
}