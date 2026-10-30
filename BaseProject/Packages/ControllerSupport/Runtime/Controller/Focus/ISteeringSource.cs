using Base.ServicesPackage;

namespace Base.ControllerSupportPackage.Controller.Focus
{
    /// <summary>
    /// Tells the <see cref="FocusWatchdog"/> whether the player steers the UI right now, with a gamepad or with the
    /// keyboard alone. Register one when keyboard players should keep a focus like gamepad players do; the device
    /// tracker only knows gamepads. Without one, the watchdog guards the focus while a gamepad is in use.
    /// </summary>
    public interface ISteeringSource : IGameService
    {
        /// <summary>Whether the player steers with a gamepad or the keyboard right now, rather than pointing.</summary>
        bool IsSteering { get; }
    }
}