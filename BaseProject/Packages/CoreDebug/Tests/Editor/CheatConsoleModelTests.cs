using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Base.CorePackage.DebugMenu.CheatConsole;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.CorePackage.Debug.Tests
{
    /// <summary>
    /// Covers the console's job between the text a player types and the method that runs: splitting
    /// the line, converting each argument to the type the method wants, and turning whatever comes
    /// back, including a failure, into a message the console can show.
    /// </summary>
    public sealed class CheatConsoleModelTests
    {
        private const string ActionlessCommand = "empty";
        private const string AddCommand = "add";
        private const string EchoCommand = "echo";
        private const string FailCommand = "fail";
        private const string ModeCommand = "mode";
        private const string PingCommand = "ping";
        private const string ToggleCommand = "toggle";
        private const string UnknownCommand = "nonsense";

        private CheatCommandProbe _probe;
        private CheatConsoleModel _model;

        /// <summary>Every test starts from a console holding the same set of commands.</summary>
        [SetUp]
        public void Build()
        {
            _probe = new CheatCommandProbe();

            _model = new CheatConsoleModel(new List<CheatCommandInfo>
            {
                Command(PingCommand, nameof(CheatCommandProbe.Ping)),
                Command(ToggleCommand, nameof(CheatCommandProbe.Toggle)),
                Command(AddCommand, nameof(CheatCommandProbe.Add)),
                Command(EchoCommand, nameof(CheatCommandProbe.Echo)),
                Command(ModeCommand, nameof(CheatCommandProbe.SetMode)),
                Command(FailCommand, nameof(CheatCommandProbe.Fail))
            });
        }

        /// <summary>The registered commands are the ones that were handed in.</summary>
        [Test]
        public void TheRegisteredCommandsAreAvailable()
        {
            Assert.That(_model.Commands.Count, Is.EqualTo(6));
            Assert.That(_model.Commands.ContainsKey(PingCommand), Is.True);
        }

        /// <summary>A command answers with whatever its method returned.</summary>
        [Test]
        public void ACommandAnswersWithItsReturnValue()
        {
            CheatConsoleResult result = _model.Execute(PingCommand);

            Assert.That(result.MessageType, Is.EqualTo(ECheatConsoleMessageType.Info));
            Assert.That(result.Message, Is.EqualTo(CheatCommandProbe.PongAnswer));
        }

        /// <summary>A command without a return value still reports that it ran.</summary>
        [Test]
        public void ACommandWithoutAReturnValueReportsThatItRan()
        {
            CheatConsoleResult result = _model.Execute(ToggleCommand);

            Assert.That(_probe.WasToggled, Is.True);
            Assert.That(result.MessageType, Is.EqualTo(ECheatConsoleMessageType.Info));
            Assert.That(result.Message, Does.Contain(ToggleCommand));
        }

        /// <summary>Typing a command in any case finds it.</summary>
        [Test]
        public void CommandLookupIgnoresCase()
        {
            CheatConsoleResult result = _model.Execute(PingCommand.ToUpperInvariant());

            Assert.That(result.MessageType, Is.EqualTo(ECheatConsoleMessageType.Info));
        }

        /// <summary>Surrounding whitespace is not part of the command.</summary>
        [Test]
        public void SurroundingWhitespaceIsIgnored()
        {
            CheatConsoleResult result = _model.Execute($"   {PingCommand}   ");

            Assert.That(result.Message, Is.EqualTo(CheatCommandProbe.PongAnswer));
        }

        /// <summary>Arguments are converted to the types the method asked for.</summary>
        [Test]
        public void ArgumentsAreConvertedToTheParameterTypes()
        {
            CheatConsoleResult result = _model.Execute($"{AddCommand} 2 3");

            Assert.That(result.MessageType, Is.EqualTo(ECheatConsoleMessageType.Info));
            Assert.That(result.Message, Is.EqualTo("5"));
        }

        /// <summary>An enum argument is parsed by name, in any case.</summary>
        [Test]
        public void AnEnumArgumentIsParsedByName()
        {
            CheatConsoleResult result = _model.Execute($"{ModeCommand} slow");

            Assert.That(result.Message, Is.EqualTo(nameof(EProbeMode.Slow)));
        }

        /// <summary>Quotes hold a value with spaces together as one argument.</summary>
        [Test]
        public void QuotesKeepAnArgumentTogether()
        {
            CheatConsoleResult result = _model.Execute($"{EchoCommand} \"hello there\"");

            Assert.That(result.Message, Is.EqualTo("hello there"));
        }

        /// <summary>An unknown command is reported rather than guessed at.</summary>
        [Test]
        public void AnUnknownCommandIsRejected()
        {
            CheatConsoleResult result = _model.Execute(UnknownCommand);

            Assert.That(result.MessageType, Is.EqualTo(ECheatConsoleMessageType.Error));
            Assert.That(result.Message, Does.Contain(UnknownCommand));
        }

        /// <summary>An empty line is not a command and does not reach anything.</summary>
        [Test]
        public void AnEmptyLineIsNotACommand()
        {
            Assert.That(_model.Execute(null).MessageType, Is.EqualTo(ECheatConsoleMessageType.Warning));
            Assert.That(_model.Execute(string.Empty).MessageType, Is.EqualTo(ECheatConsoleMessageType.Warning));
            Assert.That(_model.Execute("   ").MessageType, Is.EqualTo(ECheatConsoleMessageType.Warning));
        }

        /// <summary>The wrong number of arguments answers with how the command is used.</summary>
        [Test]
        public void TheWrongArgumentCountAnswersWithTheUsage()
        {
            CheatConsoleResult result = _model.Execute($"{AddCommand} 2");

            Assert.That(result.MessageType, Is.EqualTo(ECheatConsoleMessageType.Warning));
            Assert.That(result.Message, Does.Contain(AddCommand));
        }

        /// <summary>
        /// A command that fails is caught, so one bad cheat cannot take the console down with it.
        /// </summary>
        /// <remarks>
        /// Only the message type is asserted. Reflection wraps whatever the command threw, so the text
        /// the console reports describes the invocation rather than the cause.
        /// </remarks>
        [Test]
        public void AFailingCommandIsCaught()
        {
            CheatConsoleResult result = _model.Execute(FailCommand);

            Assert.That(result.MessageType, Is.EqualTo(ECheatConsoleMessageType.Error));
            Assert.That(result.Message, Is.Not.Empty);
        }

        /// <summary>Walking back through the history hands back what was typed, newest first.</summary>
        [Test]
        public void HistoryWalksBackwardsFromTheNewestEntry()
        {
            _model.Execute(PingCommand);
            _model.Execute(ToggleCommand);

            Assert.That(_model.GetPreviousHistory(), Is.EqualTo(ToggleCommand));
            Assert.That(_model.GetPreviousHistory(), Is.EqualTo(PingCommand));
        }

        /// <summary>Walking back past the oldest entry stays on it.</summary>
        [Test]
        public void HistoryStopsAtTheOldestEntry()
        {
            _model.Execute(PingCommand);

            Assert.That(_model.GetPreviousHistory(), Is.EqualTo(PingCommand));
            Assert.That(_model.GetPreviousHistory(), Is.EqualTo(PingCommand));
        }

        /// <summary>Walking forward again ends on an empty line, ready for new input.</summary>
        [Test]
        public void HistoryWalksForwardBackToAnEmptyLine()
        {
            _model.Execute(PingCommand);
            _model.Execute(ToggleCommand);
            _model.GetPreviousHistory();
            _model.GetPreviousHistory();

            Assert.That(_model.GetNextHistory(), Is.EqualTo(ToggleCommand));
            Assert.That(_model.GetNextHistory(), Is.Empty);
        }

        /// <summary>An empty history has nothing to hand back.</summary>
        [Test]
        public void AnEmptyHistoryAnswersNothing()
        {
            Assert.That(_model.GetPreviousHistory(), Is.Null);
            Assert.That(_model.GetNextHistory(), Is.Null);
        }

        /// <summary>Suggestions are the commands that start with what was typed.</summary>
        [Test]
        public void SuggestionsMatchThePrefix()
        {
            List<string> suggestions = _model.GetSuggestions("p");

            Assert.That(suggestions, Does.Contain(PingCommand));
            Assert.That(suggestions, Does.Not.Contain(AddCommand));
        }

        /// <summary>A fully typed command is not suggested back to the person typing it.</summary>
        [Test]
        public void AFullyTypedCommandIsNotSuggested()
            => Assert.That(_model.GetSuggestions(PingCommand), Does.Not.Contain(PingCommand));

        /// <summary>Nothing typed means nothing to suggest.</summary>
        [Test]
        public void AnEmptyInputSuggestsNothing()
        {
            Assert.That(_model.GetSuggestions(null), Is.Empty);
            Assert.That(_model.GetSuggestions("   "), Is.Empty);
        }

        /// <summary>A built in command is registered and runs like any other.</summary>
        [Test]
        public void ABuiltinCommandIsRegisteredAndRuns()
        {
            _model.RegisterBuiltinCommand("greet", "Says hello.", () => "hello");

            CheatConsoleResult result = _model.Execute("greet");

            Assert.That(result.MessageType, Is.EqualTo(ECheatConsoleMessageType.Info));
            Assert.That(result.Message, Is.EqualTo("hello"));
        }

        /// <summary>A name that is already taken is reported instead of silently replacing.</summary>
        [Test]
        public void ABuiltinCommandCannotTakeAnExistingName()
        {
            LogAssert.Expect(LogType.Warning, new Regex(PingCommand));

            _model.RegisterBuiltinCommand(PingCommand, "Collides.", () => "replaced");

            Assert.That(_model.Execute(PingCommand).Message, Is.EqualTo(CheatCommandProbe.PongAnswer));
        }

        /// <summary>A command without a name could never be typed.</summary>
        [Test]
        public void ABuiltinCommandWithoutANameIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex("without a name"));

            _model.RegisterBuiltinCommand("   ", "No name.", () => "nothing");

            Assert.That(_model.Commands.Count, Is.EqualTo(6));
        }

        /// <summary>A command without an action would have nothing to run.</summary>
        [Test]
        public void ABuiltinCommandWithoutAnActionIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(ActionlessCommand));

            _model.RegisterBuiltinCommand(ActionlessCommand, "No action.", null);

            Assert.That(_model.Commands.Count, Is.EqualTo(6));
        }

        /// <summary>A console built without commands reports it and stays usable.</summary>
        [Test]
        public void AConsoleWithoutCommandsIsReported()
        {
            LogAssert.Expect(LogType.Error, new Regex("collection is null"));

            CheatConsoleModel empty = new(null);

            Assert.That(empty.Commands, Is.Empty);
            Assert.That(empty.Execute(PingCommand).MessageType, Is.EqualTo(ECheatConsoleMessageType.Error));
        }

        /// <summary>A command claiming a name that is taken is reported at construction.</summary>
        [Test]
        public void ADuplicateCommandNameIsReportedAtConstruction()
        {
            LogAssert.Expect(LogType.Warning, new Regex(PingCommand));

            CheatConsoleModel duplicated = new(new List<CheatCommandInfo>
            {
                Command(PingCommand, nameof(CheatCommandProbe.Ping)),
                Command(PingCommand, nameof(CheatCommandProbe.Ping))
            });

            Assert.That(duplicated.Commands.Count, Is.EqualTo(1));
        }

        private CheatCommandInfo Command(string command, string methodName)
        {
            MethodInfo method = typeof(CheatCommandProbe).GetMethod(methodName);

            if (method == null)
                throw new MissingMethodException(nameof(CheatCommandProbe), methodName);

            return new CheatCommandInfo(new CheatCommandAttribute(command), method, _probe);
        }
    }
}