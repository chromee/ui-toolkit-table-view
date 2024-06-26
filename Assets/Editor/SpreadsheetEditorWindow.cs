using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class SpreadsheetEditorWindow : EditorWindow
    {
        private int _columns = 5;
        private int _rows = 5;
        private float[] _columnWidths;
        private VisualElement[] _headerCells;
        private StringCell[][] _cells;
        private VisualElement _table;
        private VisualElement _headerRow;

        private StringCell _selectedCell;
        private VisualElement _selectMarker;

        private StringCell _copiedCell;
        private VisualElement _copyMarker;

        private readonly HashSet<KeyCode> _pressedKeys = new();

        [MenuItem("Window/Spreadsheet Editor")]
        public static void ShowExample()
        {
            var wnd = GetWindow<SpreadsheetEditorWindow>();
            wnd.titleContent = new GUIContent("Spreadsheet Editor");
        }

        public void CreateGUI()
        {
            // Load USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/SpreadsheetEditor.uss");
            rootVisualElement.styleSheets.Add(styleSheet);

            _columnWidths = new float[_columns];
            _headerCells = new VisualElement[_columns];
            _cells = new StringCell[_rows][];

            // Initialize column widths
            for (var i = 0; i < _columns; i++) _columnWidths[i] = 100f;

            // Create a table
            _table = new VisualElement();
            _table.style.flexDirection = FlexDirection.Column;
            _table.style.flexGrow = 1;

            // Create header row with resizable columns
            _headerRow = new VisualElement();
            _headerRow.style.flexDirection = FlexDirection.Row;
            for (var j = 0; j < _columns; j++)
            {
                var headerCell = CreateHeaderCell(j);
                _headerCells[j] = headerCell;
                _headerRow.Add(headerCell);
            }

            _table.Add(_headerRow);

            // Create data rows
            for (var i = 0; i < _rows; i++)
            {
                var row = CreateDataRow(i);
                _table.Add(row);
            }

            rootVisualElement.Add(_table);
        }

        private void OnEnable()
        {
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            rootVisualElement.RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
            rootVisualElement.focusable = true;
            rootVisualElement.pickingMode = PickingMode.Position;
            rootVisualElement.Focus();
        }

        private void OnDisable()
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            rootVisualElement.UnregisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
        }

        private void OnKeyDown(KeyDownEvent ev)
        {
            _pressedKeys.Add(ev.keyCode);
            if (_pressedKeys.Contains(KeyCode.LeftControl) && _pressedKeys.Contains(KeyCode.C)) CopyCell();
            else if (_pressedKeys.Contains(KeyCode.LeftControl) && _pressedKeys.Contains(KeyCode.V)) PasteCell();
            else if (_pressedKeys.Contains(KeyCode.Delete)) DeleteCell();
            else if (_pressedKeys.Contains(KeyCode.Escape))
            {
                CancelCopy();
                CancelSelect();
            }
        }

        private void OnKeyUp(KeyUpEvent ev)
        {
            if (_pressedKeys.Contains(ev.keyCode)) _pressedKeys.Remove(ev.keyCode);
        }

        #region copy

        private void CopyCell()
        {
            if (_copyMarker == null)
            {
                _copyMarker = new VisualElement();
                _copyMarker.AddToClassList("copy-marker");
                _copyMarker.pickingMode = PickingMode.Ignore;
                _copyMarker.style.position = Position.Absolute;
                rootVisualElement.Add(_copyMarker);
            }

            _copiedCell = _selectedCell;
            FitToCell(_copyMarker, _copiedCell);
        }

        private void PasteCell()
        {
            if (_copiedCell == null) return;

            _selectedCell.Value = _copiedCell.Value;
            _copiedCell = null;
            _copyMarker.RemoveFromHierarchy();
            _copyMarker = null;
        }

        private void CancelCopy()
        {
            if (_copiedCell == null) return;

            _copiedCell = null;
            _copyMarker.RemoveFromHierarchy();
            _copyMarker = null;
        }

        private void DeleteCell()
        {
            _selectedCell.Value = string.Empty;
        }

        #endregion

        #region select

        private void SelectCell(StringCell cell)
        {
            if (_selectMarker == null)
            {
                _selectMarker = new VisualElement();
                _selectMarker.AddToClassList("select-marker");
                _selectMarker.pickingMode = PickingMode.Ignore;
                _selectMarker.style.position = Position.Absolute;
                rootVisualElement.Add(_selectMarker);
            }

            _selectedCell = cell;
            FitToCell(_selectMarker, _selectedCell);
        }

        private void CancelSelect()
        {
            _selectedCell = null;
            _selectMarker.RemoveFromHierarchy();
            _selectMarker = null;
        }

        #endregion

        private VisualElement CreateHeaderCell(int columnIndex)
        {
            var headerCell = new VisualElement();
            headerCell.AddToClassList("header-cell");
            headerCell.style.width = _columnWidths[columnIndex];

            var headerLabel = new Label($"Header {columnIndex}");
            headerLabel.AddToClassList("header-label");
            headerCell.Add(headerLabel);

            var resizer = new VisualElement();
            resizer.AddToClassList("resizer");
            resizer.RegisterCallback<MouseDownEvent>(evt => StartResizing(evt, columnIndex));

            headerCell.Add(resizer);

            return headerCell;
        }

        private VisualElement CreateDataRow(int rowIndex)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            _cells[rowIndex] = new StringCell[_columns];
            for (var j = 0; j < _columns; j++)
            {
                var cell = new StringCell(rowIndex, j, $"Cell {rowIndex},{j}", _columnWidths[j]);
                cell.Element.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.clickCount == 1) SelectCell(cell);
                    if (evt.clickCount >= 2) cell.StartEditing();
                });
                row.Add(cell.Element);
                _cells[rowIndex][j] = cell;
            }

            return row;
        }

        #region resize

        private bool _isResizing;
        private int _resizingColumnIndex = -1;
        private Vector2 _initialMousePosition;
        private float _initialColumnWidth;

        private void StartResizing(MouseDownEvent evt, int columnIndex)
        {
            _isResizing = true;
            _resizingColumnIndex = columnIndex;
            _initialMousePosition = evt.mousePosition;
            _initialColumnWidth = _columnWidths[columnIndex];
            rootVisualElement.RegisterCallback<MouseMoveEvent>(Resizing);
            rootVisualElement.RegisterCallback<MouseUpEvent>(StopResizing);
        }

        private void Resizing(MouseMoveEvent evt)
        {
            if (!_isResizing) return;

            var delta = evt.mousePosition.x - _initialMousePosition.x;
            _columnWidths[_resizingColumnIndex] = Mathf.Max(50, _initialColumnWidth + delta);

            // Update the header cell width
            _headerCells[_resizingColumnIndex].style.width = _columnWidths[_resizingColumnIndex];

            // Update all cells in the same column
            for (var i = 0; i < _rows; i++) _cells[i][_resizingColumnIndex].Width = _columnWidths[_resizingColumnIndex];
        }

        private void StopResizing(MouseUpEvent evt)
        {
            if (!_isResizing) return;

            _isResizing = false;
            rootVisualElement.UnregisterCallback<MouseMoveEvent>(Resizing);
            rootVisualElement.UnregisterCallback<MouseUpEvent>(StopResizing);
        }

        #endregion

        private void FitToCell(VisualElement fit, Cell cell)
        {
            var targetRect = GetElementRelativeBound(cell, rootVisualElement);

            fit.style.left = targetRect.x - 1;
            fit.style.top = targetRect.y - 1;
            fit.style.width = targetRect.width;
            fit.style.height = targetRect.height;
        }

        private Rect GetElementRelativeBound(Cell cell, VisualElement relativeTo)
        {
            var worldBound = cell.Element.worldBound;

            var localBound = worldBound;
            var containerWorldBound = relativeTo.worldBound;
            localBound.x -= containerWorldBound.x;
            localBound.y -= containerWorldBound.y;

            return new Rect(localBound.x, localBound.y, localBound.width, localBound.height);
        }
    }

    #region cells

    public class StringCell : Cell
    {
        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                if (Label != null) Label.text = value;
            }
        }

        public readonly Label Label;

        public StringCell(int row, int column, string value, float width = 100f) : base(row, column, width)
        {
            Value = value;
            Label = new Label { text = Value };
            Element.Add(Label);
        }

        public void StartEditing()
        {
            var textField = new TextField { value = Value, };
            textField.style.width = Width;
            Element.AddToClassList("input-cell");

            Label.RemoveFromHierarchy();
            Element.Add(textField);

            textField.RegisterCallback<FocusOutEvent>(evt =>
            {
                Value = textField.value;
                Element.Add(Label);
                textField.RemoveFromHierarchy();
                Element.RemoveFromClassList("input-cell");
            });

            textField.Focus();
        }
    }

    public class Cell
    {
        public Cell(int row, int column, float width = 100f)
        {
            Row = row;
            Column = column;
            Element = new VisualElement();
            Width = width;
            Element.AddToClassList("cell");
        }

        public readonly int Row;
        public readonly int Column;
        public readonly VisualElement Element;

        public float Width
        {
            get => Element.resolvedStyle.width;
            set => Element.style.width = value;
        }
    }

    #endregion
}
