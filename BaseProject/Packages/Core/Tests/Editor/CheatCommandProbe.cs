using System;

namespace Base.CorePackage.Tests
{
    /// <summary>
    /// The object the cheat console tests point their commands at. One method per case the console has
    /// to handle: no arguments, typed arguments, an enum, a return value and a failure.
    /// </summary>
    /// <remarks>
    /// The methods are public because the console resolves them through reflection and invokes them on
    /// this instance, the same way a real cheat command is reached.
    /// </remarks>
    public sealed class CheatCommandProbe
    {
        /// <summary>The failure message the throwing command reports.</summary>
        public const string FailureMessage = "Command exploded.";

        /// <summary>The answer the simple command gives.</summary>
        public const string PongAnswer = "Pong";

        /// <summary>True once the command without a return value has run.</summary>
        public bool WasToggled { get; private set; }

        /// <summary>A command that answers with a value.</summary>
        /// <returns>A fixed answer the test recognizes.</returns>
        public string Ping() => PongAnswer;

        /// <summary>A command that answers with nothing, so the console reports it ran.</summary>
        public void Toggle() => WasToggled = true;

        /// <summary>A command with typed arguments.</summary>
        /// <param name="first">The first summand.</param>
        /// <param name="second">The second summand.</param>
        /// <returns>The sum, as text.</returns>
        public string Add(int first, int second) => (first + second).ToString();

        /// <summary>A command that hands its argument straight back.</summary>
        /// <param name="text">The text to echo.</param>
        /// <returns>The text it was given.</returns>
        public string Echo(string text) => text;

        /// <summary>A command taking an enum, so the text has to be parsed into one.</summary>
        /// <param name="mode">The mode to report.</param>
        /// <returns>The name of the mode.</returns>
        public string SetMode(EProbeMode mode) => mode.ToString();

        /// <summary>A command that fails, so the console has to catch it.</summary>
        /// <returns>Never returns.</returns>
        /// <exception cref="InvalidOperationException">Always.</exception>
        public string Fail() => throw new InvalidOperationException(FailureMessage);
    }
}