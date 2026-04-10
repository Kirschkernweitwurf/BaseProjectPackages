namespace Systems.MenuManaging
{
    /// <summary>
    /// Unique identifiers for different types of menus in the game.
    /// </summary>
    public enum EMenuIdentifier : byte
    {
        None = 0,
        Main = 1,
        Pause = 2,
        Credits = 3,
        Confirmation = 4,
        LoadingScreen = 5,
        CheatConsole = 6,
        WinLose = 7,
        StarterDeckSelection = 8,
        StarterDeckOverview = 9,
        BossSelection = 10,
    }
}