using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Utility.Logging;

namespace Systems.CheatConsole
{
    /// <summary>
    /// Model for the cheat console, responsible for command registration, parsing,
    /// execution, and history.
    /// </summary>
    public sealed class CheatConsoleModel
    {
        private readonly List<string> _history;
        private readonly Dictionary<string, CheatCommandInfo> _commands;

        private int _historyIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheatConsoleModel"/> class.
        /// </summary>
        /// <param name="commands">The initial set of cheat commands.</param>
        public CheatConsoleModel(IEnumerable<CheatCommandInfo> commands)
        {
            if (commands == null)
            {
                CustomLogger.LogError("CheatConsoleModel initialization failed: commands collection is null.", null);
                return;
            }

            this._commands = new Dictionary<string, CheatCommandInfo>(StringComparer.OrdinalIgnoreCase);
            _history = new List<string>();
            _historyIndex = -1;

            foreach (CheatCommandInfo command in commands)
                RegisterCommand(command);
        }

        /// <summary>
        /// Gets a read-only view of the registered commands.
        /// </summary>
        public IReadOnlyDictionary<string, CheatCommandInfo> Commands => _commands;

        /// <summary>
        /// Executes a raw input string as a cheat command.
        /// </summary>
        /// <param name="input">The raw input string typed into the console.</param>
        /// <returns>The result of executing the command.</returns>
        public CheatConsoleResult Execute(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new CheatConsoleResult("No command entered.",
                    CheatConsoleMessageType.Warning);

            string trimmed = input.Trim();
            AddToHistory(trimmed);

            string[] tokens = SplitArguments(trimmed);
            if (tokens.Length == 0)
                return new CheatConsoleResult("No command entered.",
                    CheatConsoleMessageType.Warning);

            string commandName = tokens[0];
            string[] arguments = new string[tokens.Length - 1];
            for (int i = 1; i < tokens.Length; i++)
                arguments[i - 1] = tokens[i];

            if (!_commands.TryGetValue(commandName, out CheatCommandInfo commandInfo))
                return new CheatConsoleResult("Unknown command: " + commandName,
                    CheatConsoleMessageType.Error);

            try
            {
                string executionMessage = InvokeCommand(commandInfo, arguments);
                return new CheatConsoleResult(executionMessage, CheatConsoleMessageType.Info);
            }
            catch (TargetParameterCountException exception)
            {
                string usage = exception.Message;
                string message = $"Command '{commandInfo.Attribute.Command}' called with incorrect number of " +
                                 $"arguments. Try to use it like this:\n{usage}";

                return new CheatConsoleResult(message, CheatConsoleMessageType.Warning);
            }
            catch (Exception exception)
            {
                string message = "Command failed: " + exception.Message;
                return new CheatConsoleResult(message, CheatConsoleMessageType.Error);
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
                {
                    results.Add(command);
                }
            }

            results.Sort(StringComparer.OrdinalIgnoreCase);
            return results;
        }

        /// <summary>
        /// Registers a built-in command with the specified name and action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name = "description">The description of the command.</param>
        /// <param name="action">The action to execute for the command.</param>
        public void RegisterBuiltinCommand(string name, string description, Func<string> action)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                CustomLogger.LogError("Cannot register command with null or empty name.", null);
                return;
            }

            if (_commands.ContainsKey(name))
            {
                CustomLogger.LogWarning("Command '" + name + "' is already registered.", null);
                return;
            }

            CheatCommandAttribute attribute = new(name)
            {
                Description = description
            };

            MethodInfo invokeMethod = action.Method;
            object target = action.Target;

            _commands.Add(name, new CheatCommandInfo(attribute, invokeMethod, target));
        }

        private static string InvokeCommand(CheatCommandInfo commandInfo, string[] arguments)
        {
            ParameterInfo[] parameters = commandInfo.Method.GetParameters();

            if (parameters.Length == 0)
            {
                object result = commandInfo.Method.Invoke(commandInfo.Target, null);
                if (result == null)
                    return "Executed '" + commandInfo.Attribute.Command + "'.";

                return result.ToString();
            }

            if (parameters.Length != arguments.Length)
            {
                string usage = commandInfo.Attribute.Usage ?? $"Usage: {commandInfo.Attribute.Command}" +
                    $"({string.Join(", ", parameters.Select(p => p.Name))})";

                throw new TargetParameterCountException(usage);
            }

            object[] convertedArguments = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                string argumentString = arguments[i];

                convertedArguments[i] = ConvertArgument(argumentString, parameter.ParameterType);
            }

            object invocationResult = commandInfo.Method.Invoke(commandInfo.Target, convertedArguments);
            if (invocationResult == null)
                return "Executed '" + commandInfo.Attribute.Command + "'.";

            return invocationResult.ToString();
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
                char c = input[i];

                if (c == '"')
                    inQuotes = !inQuotes;
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (i > startIndex)
                    {
                        string token = input.Substring(startIndex, i - startIndex);
                        result.Add(Unquote(token));
                    }

                    startIndex = i + 1;
                }
            }

            if (input.Length > startIndex)
            {
                string token = input.Substring(startIndex);
                result.Add(Unquote(token));
            }

            return result.ToArray();
        }

        private static string Unquote(string token)
        {
            string trimmed = token.Trim();
            if (trimmed.Length >= 2 && trimmed[0] == '"' && trimmed[^1] == '"')
                return trimmed.Substring(1, trimmed.Length - 2);

            return trimmed;
        }

        private void RegisterCommand(CheatCommandInfo commandInfo)
        {
            if (commandInfo == null)
            {
                CustomLogger.LogError("Cannot register null cheat command.", null);
                return;
            }

            string command = commandInfo.Attribute.Command;
            if (string.IsNullOrWhiteSpace(command))
            {
                CustomLogger.LogError("Cheat command has null or empty command name.", null);
                return;
            }

            if (!_commands.TryAdd(command, commandInfo))
                CustomLogger.LogWarning("Cheat command '" + command + "' is already registered.", null);
        }

        private void AddToHistory(string input)
        {
            _history.Add(input);
            _historyIndex = _history.Count;
        }
    }
}