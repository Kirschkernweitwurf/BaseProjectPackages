using System.Collections.Generic;

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
            if (_resettables == null)
                return;

            foreach (IMenuResettable resettable in _resettables)
                resettable?.ResetState();
        }

        /// <summary>Recollects the resettable children. Call after adding or removing them at runtime.</summary>
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