using Base.AttributesPackage;
using Base.CorePackage.MenuManaging.Identifier;
using Base.CorePackage.MenuManaging.Modules;
using UnityEngine;

namespace Base.UIPackage.Buttons
{
    /// <summary>
    /// Selects a tab of a <see cref="MenuGroupModule"/> on button click and shows whether that tab is the
    /// selected one.
    /// </summary>
    public sealed class MenuTabButton : CustomButton
    {
        [Tooltip("The group the tab belongs to. It sits on the menu that holds the tabs.")]
        [Required] [SerializeField] private MenuGroupModule group;

        [Tooltip("The tab this button selects.")]
        [Required] [SerializeField] private MenuIdentifier tab;

        [Tooltip("Optional. Active while the tab is selected, e.g. a highlight or an underline.")]
        [SerializeField] private GameObject selectedIndicator;

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            group.ActiveTabChanged += RefreshIndicator;
            RefreshIndicator(group.ActiveTab);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // The group can be destroyed first on a scene unload.
            if (group == null)
                return;

            group.ActiveTabChanged -= RefreshIndicator;
        }
#endregion

        protected override void OnClick() => group.Select(tab);

        private void RefreshIndicator(MenuIdentifier activeTab)
        {
            if (selectedIndicator == null)
                return;

            selectedIndicator.SetActive(activeTab == tab);
        }
    }
}