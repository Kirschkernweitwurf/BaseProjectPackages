namespace Base.ControllerSupportPackage.InputPrompts.Devices
{
    /// <summary>
    /// The kind of gamepad in use, so prompts can show the buttons as they are printed on it. Generic covers every
    /// gamepad that is not recognized.
    /// </summary>
    public enum EGamepadFamily : byte
    {
        Generic = 0,
        Xbox = 1,
        PlayStation = 2,
        Switch = 3,
        SteamDeck = 4,
        SteamController = 5
    }
}