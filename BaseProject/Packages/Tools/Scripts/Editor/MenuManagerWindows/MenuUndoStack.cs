using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Undo and redo for the menu manager windows. Unity's own undo does not reach the registry
    /// asset and the overlay singleton, so the stack keeps whole tree snapshots instead. Every
    /// change pushes one before it runs, which also drops the redo history.
    /// </summary>
    internal sealed class MenuUndoStack
    {
        private const int MaxSteps = 100;
        private const string RedoCommand = "Redo";
        private const string UndoCommand = "Undo";

        /// <summary>Whether there is a step to go back to.</summary>
        public bool CanUndo => _undoStates.Count > 0;

        /// <summary>Whether an undone step can be replayed.</summary>
        public bool CanRedo => _redoStates.Count > 0;

        private readonly Action<MenuUndoState> _apply;
        private readonly Func<MenuUndoState> _capture;
        private readonly List<MenuUndoState> _redoStates = new();
        private readonly List<MenuUndoState> _undoStates = new();

        /// <summary>Creates a stack around the two operations that read and write the trees.</summary>
        /// <param name="capture">Takes a snapshot of the current state.</param>
        /// <param name="apply">Writes a snapshot back over the current state.</param>
        public MenuUndoStack(Func<MenuUndoState> capture, Action<MenuUndoState> apply)
        {
            _capture = capture ?? throw new ArgumentNullException(nameof(capture));
            _apply = apply ?? throw new ArgumentNullException(nameof(apply));
        }

        /// <summary>Remembers the current state. Call this before a change, never after.</summary>
        public void Push()
        {
            _undoStates.Add(_capture.Invoke());

            if (_undoStates.Count > MaxSteps)
                _undoStates.RemoveAt(0);

            _redoStates.Clear();
        }

        /// <summary>
        /// Throws the newest step away again. Used by the commands that push first and then find
        /// out they had nothing to change, so an empty step never lands on the stack.
        /// </summary>
        public void DropLast()
        {
            if (_undoStates.Count == 0)
                return;

            _undoStates.RemoveAt(_undoStates.Count - 1);
        }

        /// <summary>Goes one step back, if there is one.</summary>
        public void Undo()
        {
            if (_undoStates.Count == 0)
                return;

            _redoStates.Add(_capture.Invoke());
            MenuUndoState state = _undoStates[^1];
            _undoStates.RemoveAt(_undoStates.Count - 1);
            _apply.Invoke(state);
        }

        /// <summary>Replays the step that was undone last, if there is one.</summary>
        public void Redo()
        {
            if (_redoStates.Count == 0)
                return;

            _undoStates.Add(_capture.Invoke());
            MenuUndoState state = _redoStates[^1];
            _redoStates.RemoveAt(_redoStates.Count - 1);
            _apply.Invoke(state);
        }

        /// <summary>
        /// Takes the editor wide undo and redo shortcuts over, so they act on these trees instead
        /// of on whatever Unity has on its own stack while this window has focus.
        /// </summary>
        /// <param name="current">The event being processed.</param>
        public void HandleCommands(Event current)
        {
            if (current.type == EventType.ValidateCommand
                && (current.commandName == UndoCommand || current.commandName == RedoCommand))
            {
                current.Use();
                return;
            }

            if (current.type != EventType.ExecuteCommand)
                return;

            if (current.commandName == UndoCommand)
            {
                Undo();
                current.Use();
            }
            else if (current.commandName == RedoCommand)
            {
                Redo();
                current.Use();
            }
        }
    }
}