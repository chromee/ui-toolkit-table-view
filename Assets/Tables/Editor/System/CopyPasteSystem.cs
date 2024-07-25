using System.Linq;
using Tables.Editor.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tables.Editor.System
{
    public class CopyPasteSystem
    {
        private readonly Table _table;
        private readonly SelectSystem _selectSystem;
        private readonly UndoRedoSystem _undoRedoSystem;
        private readonly Marker _copyMarker;

        public Vector2Int CopyCellPosition { get; private set; }
        public bool IsExistCopyCell => CopyCellPosition != InvalidPosition;
        public bool IsCopyCell(Cell cell) => CopyCellPosition == cell.Position;
        public Cell CopiedCell => IsExistCopyCell ? _table.DataRows[CopyCellPosition.y][CopyCellPosition.x] : null;

        private static readonly Vector2Int InvalidPosition = new(-1, -1);

        public CopyPasteSystem(VisualElement rootVisualElement, Table table, SelectSystem selectSystem, UndoRedoSystem undoRedoSystem)
        {
            _table = table;
            _selectSystem = selectSystem;
            _undoRedoSystem = undoRedoSystem;
            _copyMarker = new Marker(rootVisualElement, "copy-marker");
        }

        public void CopyCell()
        {
            CopyCellPosition = _selectSystem.StartSelectedCellPosition;
            _copyMarker.Fit(CopiedCell);
            _copyMarker.IsVisible = true;
        }

        public void PasteCell()
        {
            if (!IsExistCopyCell) return;

            var selectedCells = _selectSystem.SelectedCells;
            if (selectedCells == null || !selectedCells.Any()) return;

            var copyCell = CopiedCell;
            var copyValue = copyCell.GetValue();

            var commandSet = new CommandSet();
            foreach (var row in selectedCells)
            foreach (var cell in row)
            {
                var prev = cell.GetValue();
                if (!cell.TryPaste(copyCell)) continue;
                commandSet.Commands.Add(new CommandSet.Command { Cell = cell, From = prev, To = copyValue });
            }

            if (commandSet.Commands.Any()) _undoRedoSystem.AddUndoCommand(commandSet);
        }

        public void CancelCopy()
        {
            CopyCellPosition = InvalidPosition;
            _copyMarker.IsVisible = false;
        }

        public void FitCopyMarker()
        {
            if (!IsExistCopyCell) return;
            _copyMarker.Fit(CopiedCell);
        }
    }
}
