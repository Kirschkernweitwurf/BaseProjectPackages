namespace Base.ControllerSupportPackage.InputPrompts
{
    /// <summary>On which devices an <see cref="InputPromptView"/> shows, so each device sees only what it can press.</summary>
    public enum EPromptVisibility : byte
    {
        Always = 0,
        GamepadOnly = 1,
        MouseKeyboardOnly = 2
    }
}