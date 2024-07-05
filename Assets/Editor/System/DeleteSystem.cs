using System.Linq;
using Editor.VisualElements;

namespace Editor.System
{
    public class DeleteSystem
    {
        private readonly SelectSystem _selectSystem;
        private readonly UndoRedoSystem _undoRedoSystem;

        public DeleteSystem(SelectSystem selectSystem, UndoRedoSystem undoRedoSystem)
        {
            _selectSystem = selectSystem;
            _undoRedoSystem = undoRedoSystem;
        }

        public void DeleteSelected()
        {
            var selectedCells = _selectSystem.GetSelectedCells();
            if (selectedCells == null || !selectedCells.Any()) return;

            var commandSet = new CommandSet();
            foreach (var row in selectedCells)
            foreach (var cell in row)
            {
                var prev = cell.Val;
                cell.ClearValue();
                commandSet.Commands.Add(new CommandSet.Command { Cell = cell, From = prev, To = cell.Val });
            }

            if (commandSet.Commands.Any()) _undoRedoSystem.AddUndoCommand(commandSet);
        }
    }
}
