using Tables.Editor.Utilities;
using Tables.Editor.VisualElements;
using Tables.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tables.Editor.System
{
    public class ResizeColSystem
    {
        private readonly Database _database;
        private readonly VisualElement _rootVisualElement;
        private readonly ScrollView _body;
        private readonly Table _table;
        private readonly SelectSystem _selectSystem;
        private readonly CopyPasteSystem _copyPasteSystem;

        private bool _isResizing;
        private int _resizingColumnIndex = -1;
        private Vector2 _initialMousePosition;
        private float _initialColumnWidth;

        public ResizeColSystem(Database database, VisualElement rootVisualElement, ScrollView body, Table table, SelectSystem selectSystem, CopyPasteSystem copyPasteSystem)
        {
            _database = database;
            _rootVisualElement = rootVisualElement;
            _body = body;
            _table = table;
            _selectSystem = selectSystem;
            _copyPasteSystem = copyPasteSystem;
        }

        public void StartResizing(MouseDownEvent evt, int columnIndex)
        {
            _isResizing = true;
            _resizingColumnIndex = columnIndex;
            _initialMousePosition = evt.mousePosition;
            _initialColumnWidth = _database.Columns[columnIndex].Width;
            _rootVisualElement.RegisterCallback<MouseMoveEvent>(Resizing);
            _rootVisualElement.RegisterCallback<MouseUpEvent>(StopResizing);
        }

        private void Resizing(MouseMoveEvent evt)
        {
            if (!_isResizing) return;

            var delta = evt.mousePosition.x - _initialMousePosition.x;
            var width = Mathf.Max(50, _initialColumnWidth + delta);
            _database.Columns[_resizingColumnIndex].Width = width;
            _table.HeaderRow.Cells[_resizingColumnIndex].style.width = width;
            _table.EmptyRow.Cells[_resizingColumnIndex].Width = width;

            foreach (var row in _table.DataRows)
            {
                var cell = row[_resizingColumnIndex];
                cell.Width = width;
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
            _body.ExecAfter1Frame(() => _body.ForceUpdate());
        }
    }
}
