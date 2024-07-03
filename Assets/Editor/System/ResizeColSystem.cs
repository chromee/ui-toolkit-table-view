using Editor.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.System
{
    public class ResizeColSystem
    {
        private readonly VisualElement _rootVisualElement;
        private readonly Table _table;
        private readonly SelectSystem _selectSystem;
        private readonly CopyPasteSystem _copyPasteSystem;

        private readonly float[] _columnWidths;

        private bool _isResizing;
        private int _resizingColumnIndex = -1;
        private Vector2 _initialMousePosition;
        private float _initialColumnWidth;

        public ResizeColSystem(VisualElement rootVisualElement, Table table, ColInfo[] colInfos, SelectSystem selectSystem, CopyPasteSystem copyPasteSystem)
        {
            _rootVisualElement = rootVisualElement;
            _table = table;
            _selectSystem = selectSystem;
            _copyPasteSystem = copyPasteSystem;
            _columnWidths = new float[colInfos.Length];
        }

        public void StartResizing(MouseDownEvent evt, int columnIndex)
        {
            _isResizing = true;
            _resizingColumnIndex = columnIndex;
            _initialMousePosition = evt.mousePosition;
            _initialColumnWidth = _columnWidths[columnIndex];
            _rootVisualElement.RegisterCallback<MouseMoveEvent>(Resizing);
            _rootVisualElement.RegisterCallback<MouseUpEvent>(StopResizing);
        }

        private void Resizing(MouseMoveEvent evt)
        {
            if (!_isResizing) return;

            var delta = evt.mousePosition.x - _initialMousePosition.x;
            _columnWidths[_resizingColumnIndex] = Mathf.Max(50, _initialColumnWidth + delta);
            _table.HeaderRow.Cells[_resizingColumnIndex].style.width = _columnWidths[_resizingColumnIndex];
            _table.EmptyRow.Cells[_resizingColumnIndex].Width = _columnWidths[_resizingColumnIndex];

            foreach (var row in _table.DataRows)
            {
                var cell = row[_resizingColumnIndex];
                cell.Width = _columnWidths[_resizingColumnIndex];
                if (cell == _selectSystem.StartSelectedCell)
                {
                    if (_selectSystem.EndSelectedCell != null && _selectSystem.IsSelected(cell)) _selectSystem.FitRangeMarker();
                    else _selectSystem.FitSelectMarker();
                }

                if (cell == _copyPasteSystem.CopiedCell) _copyPasteSystem.FitCopyMarker();
            }
        }

        private void StopResizing(MouseUpEvent evt)
        {
            if (!_isResizing) return;

            _isResizing = false;
            _rootVisualElement.UnregisterCallback<MouseMoveEvent>(Resizing);
            _rootVisualElement.UnregisterCallback<MouseUpEvent>(StopResizing);
        }
    }
}
