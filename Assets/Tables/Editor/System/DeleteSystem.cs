using System.Linq;
using Tables.Editor.VisualElements;

namespace Tables.Editor.System
{
    public class DeleteSystem
    {
        private readonly Table _table;
        private readonly SelectSystem _selectSystem;
        private readonly UndoRedoSystem _undoRedoSystem;

        public DeleteSystem(Table table, SelectSystem selectSystem, UndoRedoSystem undoRedoSystem)
        {
            _table = table;
            _selectSystem = selectSystem;
            _undoRedoSystem = undoRedoSystem;
        }

        public void Delete()
        {
            ClearSelectedCell();
            DeleteSelectedRow();
        }

        private void ClearSelectedCell()
        {
            if (_selectSystem.StartSelectedRow != null) return;

            var selectedCells = _selectSystem.GetSelectedCells();
            if (selectedCells == null || !selectedCells.Any()) return;

            var commandSet = new CommandSet();
            foreach (var row in selectedCells)
            foreach (var cell in row)
            {
                var prev = cell.GetValue();
                cell.ClearValue();
                commandSet.Commands.Add(new CommandSet.Command { Cell = cell, From = prev, To = cell.GetValue() });
            }

            if (commandSet.Commands.Any()) _undoRedoSystem.AddUndoCommand(commandSet);
        }

        public void DeleteSelectedRow()
        {
            var selectedRows = _selectSystem.GetSelectedRows();
            if (selectedRows == null || !selectedRows.Any()) return;

            foreach (var row in selectedRows) _table.RemoveDataRow(row);
        }
    }
}
