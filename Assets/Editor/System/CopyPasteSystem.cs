using System.Linq;
using Editor.VisualElements;
using UnityEngine.UIElements;

namespace Editor.System
{
    public class CopyPasteSystem
    {
        private readonly SelectSystem _selectSystem;
        private readonly UndoRedoSystem _undoRedoSystem;

        public Cell CopiedCell { get; private set; }
        private readonly Marker _copyMarker;

        public CopyPasteSystem(VisualElement rootVisualElement, SelectSystem selectSystem, UndoRedoSystem undoRedoSystem)
        {
            _selectSystem = selectSystem;
            _undoRedoSystem = undoRedoSystem;
            _copyMarker = new Marker(rootVisualElement, "copy-marker");
        }

        public void CopyCell()
        {
            CopiedCell = _selectSystem.StartSelectedCell;
            _copyMarker.Fit(CopiedCell);
            _copyMarker.IsVisible = true;
        }

        public void PasteCell()
        {
            if (CopiedCell == null) return;

            var selectedCells = _selectSystem.GetSelectedCells();
            if (selectedCells == null || !selectedCells.Any()) return;

            var commandSet = new CommandSet();
            foreach (var row in selectedCells)
            foreach (var cell in row)
            {
                var prev = cell.Val;
                if (!cell.TryPaste(CopiedCell)) continue;
                commandSet.Commands.Add(new CommandSet.Command { Cell = cell, From = prev, To = CopiedCell.Val });
            }

            if (commandSet.Commands.Any()) _undoRedoSystem.AddUndoCommand(commandSet);
        }

        public void CancelCopy()
        {
            CopiedCell = null;
            _copyMarker.IsVisible = false;
        }

        public void FitCopyMarker()
        {
            if (CopiedCell == null) return;
            _copyMarker.Fit(CopiedCell);
        }
    }
}
