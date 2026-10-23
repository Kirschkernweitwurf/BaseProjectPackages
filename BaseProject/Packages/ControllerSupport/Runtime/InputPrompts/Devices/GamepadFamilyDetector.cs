using System;

namespace Base.ControllerSupportPackage.InputPrompts.Devices
{
    /// <summary>
    /// Tells the <see cref="EGamepadFamily"/> of a gamepad from the names its driver reports: product,
    /// manufacturer, layout. Names instead of device classes, because the classes differ per platform and some
    /// gamepads, like the Steam Deck, only show up as a plain gamepad with a telling name.
    /// </summary>
    public static class GamepadFamilyDetector
    {
        private static readonly string[] PlayStationNames =
            { "DualSense", "DualShock", "PlayStation", "Sony", "PS4", "PS5" };

        private static readonly string[] SteamControllerNames = { "Steam Controller" };
        private static readonly string[] SteamDeckNames = { "Steam Deck", "SteamDeck", "Neptune" };
        private static readonly string[] SwitchNames = { "Switch", "Nintendo", "Joy-Con" };
        private static readonly string[] XboxNames = { "Xbox", "XInput", "Microsoft" };

        /// <summary>
        /// The family the given names point to, or <see cref="EGamepadFamily.Generic"/> when none matches. The
        /// Steam devices are checked first, since they also report generic Xbox style layouts.
        /// </summary>
        /// <param name="names">
        /// Product, manufacturer, layout or any other name of the device. Nulls are skipped.
        /// </param>
        public static EGamepadFamily Detect(params string[] names)
        {
            if (names == null)
                return EGamepadFamily.Generic;

            if (ContainsAny(names, SteamDeckNames))
                return EGamepadFamily.SteamDeck;

            if (ContainsAny(names, SteamControllerNames))
                return EGamepadFamily.SteamController;

            if (ContainsAny(names, PlayStationNames))
                return EGamepadFamily.PlayStation;

            if (ContainsAny(names, SwitchNames))
                return EGamepadFamily.Switch;

            return ContainsAny(names, XboxNames)
                ? EGamepadFamily.Xbox
                : EGamepadFamily.Generic;
        }

        private static bool ContainsAny(string[] names, string[] keywords)
        {
            foreach (string name in names)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                foreach (string keyword in keywords)
                {
                    if (name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }

            return false;
        }
    }
}