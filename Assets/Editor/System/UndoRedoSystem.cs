using System.Collections.Generic;
using Editor.VisualElements;
using UnityEngine.UIElements;

namespace Editor.System
{
    public class UndoRedoSystem
    {
        private readonly Stack<CommandSet> _undoStack = new();
        private readonly Stack<CommandSet> _redoStack = new();

        public void AddUndoCommand(Cell cell, object from, object to)
        {
            var command = new CommandSet.Command { Cell = cell, From = from, To = to };
            _undoStack.Push(new CommandSet(command));
            _redoStack.Clear();
        }

        public void AddUndoCommand(CommandSet commandSet)
        {
            _undoStack.Push(commandSet);
            _redoStack.Clear();
        }

        public void Undo(VisualElement focusTarget = null)
        {
            if (_undoStack.Count == 0) return;

            var commandSet = _undoStack.Pop();
            commandSet.Undo();
            _redoStack.Push(commandSet);
            focusTarget?.Focus();
        }

        public void Redo(VisualElement focusTarget = null)
        {
            if (_redoStack.Count == 0) return;

            var commandSet = _redoStack.Pop();
            commandSet.Redo();
            _undoStack.Push(commandSet);
            focusTarget?.Focus();
        }
    }

    public class CommandSet
    {
        public readonly Command SingleCommand;
        public readonly List<Command> Commands;

        public CommandSet(Command singleCommand) => SingleCommand = singleCommand;
        public CommandSet() => Commands = new List<Command>();

        public class Command
        {
            public Cell Cell;
            public object From;
            public object To;

            public void Undo()
            {
                if (Cell == null) return;

                if (From is string fromS)
                {
                    var cell = Cell.As<string>();
                    if (cell != null) cell.Value = fromS;
                }
                else if (From is int fromI)
                {
                    var cell = Cell.As<int>();
                    if (cell != null) cell.Value = fromI;
                }
                else if (From is float fromF)
                {
                    var cell = Cell.As<float>();
                    if (cell != null) cell.Value = fromF;
                }
                else if (From is bool fromB)
                {
                    var cell = Cell.As<bool>();
                    if (cell != null) cell.Value = fromB;
                }
            }

            public void Redo()
            {
                if (Cell == null) return;

                if (To is string toS)
                {
                    var cell = Cell.As<string>();
                    if (cell != null) cell.Value = toS;
                }
                else if (To is int toI)
                {
                    var cell = Cell.As<int>();
                    if (cell != null) cell.Value = toI;
                }
                else if (To is float toF)
                {
                    var cell = Cell.As<float>();
                    if (cell != null) cell.Value = toF;
                }
                else if (To is bool toB)
                {
                    var cell = Cell.As<bool>();
                    if (cell != null) cell.Value = toB;
                }
            }
        }

        public void Undo()
        {
            if (SingleCommand != null) SingleCommand.Undo();
            else if (Commands != null)
                foreach (var command in Commands)
                    command.Undo();
        }

        public void Redo()
        {
            if (SingleCommand != null) SingleCommand.Redo();
            else if (Commands != null)
                foreach (var command in Commands)
                    command.Redo();
        }
    }
}
