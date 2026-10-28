using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Base.UtilityPackage.Logging;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Model for the cheat console, responsible for command registration, parsing,
    /// execution, and history.
    /// </summary>
    public sealed class CheatConsoleModel
    {
        private const string ExecutedFormat = "Executed '{0}'.";
        private const string NoCommandMessage = "No command entered.";
        private const string UnknownCommandFormat = "Unknown command: {0}";
        private const string UsageFormat = "Usage: {0}({1})";

        /// <summary>
        /// Gets a read-only view of the registered commands.
        /// </summary>
        public IReadOnlyDictionary<string, CheatCommandInfo> Commands => _commands;

        private readonly Dictionary<string, CheatCommandInfo> _commands;
        private readonly List<string> _discoveredNames;
        private readonly List<string> _history;

        private int _historyIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatConsoleModel"/> class.
        /// </summary>
        /// <param name="commands">The initial set of cheat commands.</param>
        public CheatConsoleModel(IEnumerable<CheatCommandInfo> commands)
        {
            _commands = new Dictionary<string, CheatCommandInfo>(StringComparer.OrdinalIgnoreCase);
            _discoveredNames = new List<string>();
            _history = new List<string>();
            _historyIndex = -1;

            if (commands == null)
            {
                CustomLogger.LogError("Cheat command collection is null. No commands were registered.", null);
                return;
            }

            foreach (CheatCommandInfo command in commands)
                RegisterCommand(command);
        }

        /// <summary>
        /// Executes a raw input string as a cheat command.
        /// </summary>
        /// <param name="input">The raw input string typed into the console.</param>
        /// <returns>The result of executing the command.</returns>
        public CheatConsoleResult Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new CheatConsoleResult(NoCommandMessage, ECheatConsoleMessageType.Warning);

            string trimmed = input.Trim();
            AddToHistory(trimmed);

            return Run(trimmed);
        }

        /// <summary>
        /// Runs a command line without adding it to the history, so a macro's lines don't push the typed
        /// commands out of reach of the arrow keys.
        /// </summary>
        /// <param name="input">The command line, with its arguments.</param>
        /// <returns>The result of executing the command.</returns>
        internal CheatConsoleResult Run(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new CheatConsoleResult(NoCommandMessage, ECheatConsoleMessageType.Warning);

            string[] tokens = SplitArguments(input.Trim());
            if (tokens.Length == 0)
                return new CheatConsoleResult(NoCommandMessage, ECheatConsoleMessageType.Warning);

            string commandName = tokens[0];
            string[] arguments = tokens[1..];

            if (!_commands.TryGetValue(commandName, out CheatCommandInfo commandInfo))
                return new CheatConsoleResult(string.Format(UnknownCommandFormat, commandName),
                    ECheatConsoleMessageType.Error);

            try
            {
                return new CheatConsoleResult(InvokeCommand(commandInfo, arguments), ECheatConsoleMessageType.Info);
            }
            catch (TargetParameterCountException exception)
            {
                string message = $"Command '{commandInfo.Attribute.Command}' was called with the wrong number "
                    + $"of arguments. Try to use it like this:\n{exception.Message}";

                return new CheatConsoleResult(message, ECheatConsoleMessageType.Warning);
            }
            catch (Exception exception)
            {
                return new CheatConsoleResult("Command failed: " + exception.Message, ECheatConsoleMessageType.Error);
            }
        }

        /// <summary>
        /// Gets the previous command from history, if any.
        /// </summary>
        /// <returns>The previous command text, or null if none available.</returns>
        public string GetPreviousHistory()
        {
            if (_history.Count == 0)
                return null;

            if (_historyIndex < 0)
                _historyIndex = _history.Count;

            _historyIndex--;
            if (_historyIndex < 0)
                _historyIndex = 0;

            return _history[_historyIndex];
        }

        /// <summary>
        /// Gets the next command from history, if any.
        /// </summary>
        /// <returns>The next command text, or null if none available.</returns>
        public string GetNextHistory()
        {
            if (_history.Count == 0)
                return null;

            if (_historyIndex < 0)
                return null;

            _historyIndex++;
            if (_historyIndex < _history.Count)
                return _history[_historyIndex];

            _historyIndex = _history.Count;
            return string.Empty;
        }

        /// <summary>
        /// Gets a list of command suggestions based on the current input.
        /// </summary>
        /// <param name="currentInput">The text currently typed into the console.</param>
        /// <returns>All commands starting with the input, without the input itself.</returns>
        public List<string> GetSuggestions(string currentInput)
        {
            List<string> results = new();
            if (string.IsNullOrWhiteSpace(currentInput))
                return results;

            string trimmed = currentInput.Trim();

            foreach (string command in _commands.Keys)
            {
                if (command.StartsWith(trimmed, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(command, trimmed, StringComparison.OrdinalIgnoreCase))
                    results.Add(command);
            }

            results.Sort(StringComparer.OrdinalIgnoreCase);
            return results;
        }

        /// <summary>
        /// Completes <paramref name="currentInput"/> like a shell: up to the end of the next dot-separated word that
        /// every matching command shares, so "date.a" becomes "date.affection" when all its matches go on with
        /// "affection". Where the matches part ways right after the input, like "date.", the first match's next word
        /// is filled in. A single match is completed whole.
        /// </summary>
        /// <param name="currentInput">The text currently typed into the console.</param>
        /// <returns>The completed text, or the input itself when no command starts with it.</returns>
        public string Complete(string currentInput)
        {
            List<string> matches = GetSuggestions(currentInput);
            if (matches.Count == 0)
                return currentInput;

            int typedLength = currentInput.Trim().Length;
            string shared = NextWord(matches[0], typedLength);

            foreach (string match in matches)
                shared = SharedStart(shared, NextWord(match, typedLength));

            return shared.Length > typedLength
                ? shared
                : NextWord(matches[0], typedLength);
        }

        /// <summary>
        /// Swaps the commands given so far, in the constructor or here, for <paramref name="commands"/>. Built-in
        /// commands and the history stay, so the console can discover again whenever the objects change.
        /// </summary>
        /// <param name="commands">The cheat commands that can be executed now.</param>
        public void ReplaceDiscoveredCommands(IEnumerable<CheatCommandInfo> commands)
        {
            if (commands == null)
            {
                CustomLogger.LogError("Cheat command collection is null. The commands were left as they are.", null);
                return;
            }

            foreach (string name in _discoveredNames)
                _commands.Remove(name);

            _discoveredNames.Clear();

            foreach (CheatCommandInfo command in commands)
                RegisterCommand(command);
        }

        /// <summary>
        /// Registers a built-in command with the specified name and action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="action">The action to execute for the command.</param>
        public void RegisterBuiltinCommand(string name, string description, Func<string> action) =>
            AddBuiltinCommand(name, description, usage: null, action);

        /// <summary>
        /// Registers a built-in command that takes one argument.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="usage">How to type it, shown in help.</param>
        /// <param name="action">The action to execute for the command, given the argument.</param>
        internal void RegisterBuiltinCommand(string name, string description, string usage,
            Func<string, string> action) =>
            AddBuiltinCommand(name, description, usage, action);

        private static string InvokeCommand(CheatCommandInfo commandInfo, string[] arguments)
        {
            ParameterInfo[] parameters = commandInfo.Method.GetParameters();

            if (parameters.Length != arguments.Length)
            {
                string parameterList = string.Join(", ", parameters.Select(parameter => parameter.Name));

                string usage = commandInfo.Attribute.Usage
                    ?? string.Format(UsageFormat, commandInfo.Attribute.Command, parameterList);

                throw new TargetParameterCountException(usage);
            }

            object[] convertedArguments = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                convertedArguments[i] = ConvertArgument(arguments[i], parameters[i].ParameterType);

            object result = commandInfo.Method.Invoke(commandInfo.Target, convertedArguments);

            return result == null
                ? string.Format(ExecutedFormat, commandInfo.Attribute.Command)
                : result.ToString();
        }

        // A dot right at the start belongs to the word after it, so "date" goes on to "date.affection".
        private static string NextWord(string command, int start)
        {
            int dot = command.IndexOf('.', start + 1);

            return dot < 0
                ? command
                : command[..dot];
        }

        private static string SharedStart(string first, string second)
        {
            int length = 0;
            int maxLength = Math.Min(first.Length, second.Length);

            while (length < maxLength
                && char.ToLowerInvariant(first[length]) == char.ToLowerInvariant(second[length]))
                length++;

            return first[..length];
        }

        private static object ConvertArgument(string argument, Type targetType)
        {
            if (targetType == typeof(string))
                return argument;

            if (string.IsNullOrEmpty(argument))
                return null;

            Type underlyingType = Nullable.GetUnderlyingType(targetType);
            if (underlyingType != null)
                targetType = underlyingType;

            return targetType.IsEnum
                ? Enum.Parse(targetType, argument, true)
                : Convert.ChangeType(argument, targetType, CultureInfo.InvariantCulture);
        }

        private static string[] SplitArguments(string input)
        {
            List<string> result = new();
            bool inQuotes = false;
            int startIndex = 0;

            for (int i = 0; i < input.Length; i++)
            {
                char character = input[i];

                if (character == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (char.IsWhiteSpace(character) && !inQuotes)
                {
                    if (i > startIndex)
                        result.Add(Unquote(input[startIndex..i]));

                    startIndex = i + 1;
                }
            }

            if (input.Length > startIndex)
                result.Add(Unquote(input[startIndex..]));

            return result.ToArray();
        }

        private static string Unquote(string token)
        {
            string trimmed = token.Trim();

            return trimmed.Length >= 2 && trimmed[0] == '"' && trimmed[^1] == '"'
                ? trimmed[1..^1]
                : trimmed;
        }

        private void AddBuiltinCommand(string name, string description, string usage, Delegate action)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                CustomLogger.LogError("Cannot register a command without a name.", null);
                return;
            }

            if (action == null)
            {
                CustomLogger.LogError($"Cannot register command '{name}' without an action.", null);
                return;
            }

            if (_commands.ContainsKey(name))
            {
                CustomLogger.LogWarning($"Command '{name}' is already registered.", null);
                return;
            }

            CheatCommandAttribute attribute = new(name)
            {
                Description = description,
                Usage = usage
            };

            _commands.Add(name, new CheatCommandInfo(attribute, action.Method, action.Target));
        }

        private void RegisterCommand(CheatCommandInfo commandInfo)
        {
            if (commandInfo == null)
            {
                CustomLogger.LogError("Cannot register a null cheat command.", null);
                return;
            }

            string command = commandInfo.Attribute.Command;
            if (string.IsNullOrWhiteSpace(command))
            {
                CustomLogger.LogError("Cheat command has no command name.", null);
                return;
            }

            if (!_commands.TryAdd(command, commandInfo))
            {
                CustomLogger.LogWarning($"Cheat command '{command}' is already registered.", null);
                return;
            }

            _discoveredNames.Add(command);
        }

        private void AddToHistory(string input)
        {
            _history.Add(input);
            _historyIndex = _history.Count;
        }
    }
}