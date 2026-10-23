using System.Collections.Generic;
using Base.UtilityPackage.Contracts;
using Object = UnityEngine.Object;

namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// Resets stateful children that implement <see cref="IMenuResettable"/> whenever the owning menu
    /// closes, so it opens fresh next time. The content roots of this menu and of every menu nested in it
    /// are skipped, since each menu drives that animation itself.
    /// </summary>
    /// <remarks>
    /// A nested menu, like a tab, often starts closing in the same frame as its parent. Resetting its content
    /// root would stop that close animation, so the nested menu would never finish closing.
    /// </remarks>
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
            HashSet<IMenuResettable> menuRoots = CollectMenuRoots();
            IMenuResettable[] found = OwnerMenu.GetComponentsInChildren<IMenuResettable>(includeInactive: true);
            List<IMenuResettable> filtered = new(found.Length);

            foreach (IMenuResettable resettable in found)
            {
                // Content roots are driven by their menus' open and close, never by this reset.
                if (menuRoots.Contains(resettable))
                    continue;

                filtered.Add(resettable);
            }

            _resettables = filtered.ToArray();
        }

        private HashSet<IMenuResettable> CollectMenuRoots()
        {
            HashSet<IMenuResettable> roots = new();

            // Includes the owner menu itself, since GetComponentsInChildren also searches this GameObject.
            foreach (Menu menu in OwnerMenu.GetComponentsInChildren<Menu>(includeInactive: true))
            {
                if (menu.ContentRoot == null)
                    continue;

                roots.Add(menu.ContentRoot);
            }

            return roots;
        }
    }
}