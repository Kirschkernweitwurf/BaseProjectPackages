namespace Base.SystemsCorePackage.GamepadSupport.Devices
{
    /// <summary>
    /// The input device family currently driving the UI. Used to switch behaviour and prompt glyphs.
    /// </summary>
    public enum EInputDeviceType : byte
    {
        Unknown = 0,
        MouseKeyboard = 1,
        Gamepad = 2
    }
}
