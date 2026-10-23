using Base.ControllerSupportPackage.InputPrompts.Devices;
using NUnit.Framework;

namespace Base.ControllerSupportPackage.Tests
{
    /// <summary>
    /// Covers telling the kind of gamepad from the names its driver reports. A wrong guess shows buttons that are
    /// not on the player's gamepad, so the names real drivers report have to land in the right family.
    /// </summary>
    public sealed class GamepadFamilyDetectorTests
    {
        /// <summary>Real product and layout names land in their family.</summary>
        [TestCase("DualSense Wireless Controller", EGamepadFamily.PlayStation)]
        [TestCase("Wireless Controller", EGamepadFamily.Generic)]
        [TestCase("DualShock4GamepadHID", EGamepadFamily.PlayStation)]
        [TestCase("Xbox Wireless Controller", EGamepadFamily.Xbox)]
        [TestCase("XInputControllerWindows", EGamepadFamily.Xbox)]
        [TestCase("Pro Controller", EGamepadFamily.Generic)]
        [TestCase("SwitchProControllerHID", EGamepadFamily.Switch)]
        [TestCase("Steam Deck Controller", EGamepadFamily.SteamDeck)]
        [TestCase("Steam Controller", EGamepadFamily.SteamController)]
        public void NamesLandInTheirFamily(string name, EGamepadFamily expected)
            => Assert.That(GamepadFamilyDetector.Detect(name), Is.EqualTo(expected));

        /// <summary>One telling name among several is enough, like a vague product with a clear manufacturer.</summary>
        [Test]
        public void AnyTellingNameDecides()
            => Assert.That(GamepadFamilyDetector.Detect("Wireless Controller", "Sony Interactive Entertainment"),
                Is.EqualTo(EGamepadFamily.PlayStation));

        /// <summary>The Steam Deck reports an Xbox style layout, but its own name wins.</summary>
        [Test]
        public void SteamDeckWinsOverItsXboxLayout()
            => Assert.That(GamepadFamilyDetector.Detect("Steam Deck", "XInputController"),
                Is.EqualTo(EGamepadFamily.SteamDeck));

        /// <summary>Missing names fall back to the generic family.</summary>
        [Test]
        public void MissingNamesAreGeneric()
            => Assert.That(GamepadFamilyDetector.Detect(null, string.Empty), Is.EqualTo(EGamepadFamily.Generic));
    }
}