namespace Base.CorePackage.Debug.Tests
{
    /// <summary>
    /// The enum a cheat console test passes as an argument, so the conversion from typed text to a
    /// typed parameter is covered.
    /// </summary>
    /// <remarks>
    /// Public rather than internal because the console reaches the command through reflection from
    /// another assembly, and an internal parameter type would not be reachable from there.
    /// </remarks>
    public enum EProbeMode : byte
    {
        Fast = 0,
        Slow = 1
    }
}