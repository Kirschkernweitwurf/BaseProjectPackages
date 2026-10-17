namespace Base.CorePackage.MenuManaging.Modules
{
    /// <summary>
    /// What a <see cref="MenuGroupModule"/> does when its open tab is closed from outside the group, mostly by
    /// the Back button.
    /// </summary>
    public enum EMenuGroupBackBehavior : byte
    {
        /// <summary>Closes the menu that holds the tabs, together with the tab.</summary>
        CloseContainer = 0,

        /// <summary>
        /// Goes back to the default tab first, like the home screen of a phone. Closing the default tab then
        /// closes the menu that holds the tabs.
        /// </summary>
        ReturnToDefaultTab = 1
    }
}