using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SpreadsheetEditorWindow : EditorWindow
{
    private int _columns = 5;
    private int _rows = 5;
    private float[] _columnWidths;
    private VisualElement[] _headerCells;
    private VisualElement[][] _cells;
    private VisualElement _table;
    private VisualElement _headerRow;

    private VisualElement _selectedCell;
    private VisualElement _selectMarker;

    private VisualElement _copiedCell;
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
        _cells = new VisualElement[_rows][];

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

        // Add buttons for adding rows and columns
        var addButtonRow = new VisualElement();
        addButtonRow.style.flexDirection = FlexDirection.Row;

        var addRowButton = new Button(AddRow) { text = "Add Row", };
        addButtonRow.Add(addRowButton);

        var addColumnButton = new Button(AddColumn) { text = "Add Column", };
        addButtonRow.Add(addColumnButton);

        rootVisualElement.Add(addButtonRow);
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
        else if (_pressedKeys.Contains(KeyCode.Escape)) CancelCopy();
    }

    private void OnKeyUp(KeyUpEvent ev)
    {
        if (_pressedKeys.Contains(ev.keyCode)) _pressedKeys.Remove(ev.keyCode);
    }

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

        _selectedCell.Q<Label>().text = _copiedCell.Q<Label>().text;
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
        _selectedCell.Q<Label>().text = string.Empty;
    }

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
        _cells[rowIndex] = new VisualElement[_columns];
        for (var j = 0; j < _columns; j++)
        {
            var cell = new Label($"Cell {rowIndex},{j}");
            cell.AddToClassList("cell");
            cell.style.width = _columnWidths[j];
            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectCell(cell);
                if (evt.clickCount >= 2) StartEditing(cell);
            });
            row.Add(cell);
            _cells[rowIndex][j] = cell;
        }

        return row;
    }

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
        rootVisualElement.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        rootVisualElement.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (!_isResizing) return;

        var delta = evt.mousePosition.x - _initialMousePosition.x;
        _columnWidths[_resizingColumnIndex] = Mathf.Max(50, _initialColumnWidth + delta);

        // Update the header cell width
        _headerCells[_resizingColumnIndex].style.width = _columnWidths[_resizingColumnIndex];

        // Update all cells in the same column
        for (var i = 0; i < _rows; i++) _cells[i][_resizingColumnIndex].style.width = _columnWidths[_resizingColumnIndex];

        _selectMarker?.RemoveFromHierarchy();
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (_isResizing)
        {
            _isResizing = false;
            rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            rootVisualElement.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }
    }

    private void StartEditing(Label cell)
    {
        var textField = new TextField { value = cell.text, };
        var columnIndex = cell.parent.IndexOf(cell);
        textField.style.width = _columnWidths[columnIndex];
        textField.AddToClassList("input-cell");

        cell.parent.Insert(cell.parent.IndexOf(cell), textField);
        cell.RemoveFromHierarchy();

        textField.RegisterCallback<FocusOutEvent>(evt =>
        {
            cell.text = textField.value;
            textField.parent.Insert(textField.parent.IndexOf(textField), cell);
            textField.RemoveFromHierarchy();
        });

        textField.Focus();
    }

    private void AddRow()
    {
        _rows++;
        var row = CreateDataRow(_rows - 1);
        _table.Add(row);
    }

    private void AddColumn()
    {
        _columns++;
        Array.Resize(ref _columnWidths, _columns);
        Array.Resize(ref _headerCells, _columns);

        // Add new header cell
        var headerCell = CreateHeaderCell(_columns - 1);
        _headerCells[_columns - 1] = headerCell;
        _headerRow.Add(headerCell);

        // Add new cells to existing rows
        foreach (var row in _cells)
        {
            var cell = new Label($"Cell {Array.IndexOf(_cells, row)},{_columns - 1}");
            cell.AddToClassList("cell");
            cell.style.width = _columnWidths[_columns - 1];
            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectCell(cell);
                if (evt.clickCount >= 2) StartEditing(cell);
            });
            row[Array.IndexOf(row, row[0])].parent.Add(cell);
        }

        // Resize cells array to include the new column
        for (var i = 0; i < _rows; i++)
        {
            Array.Resize(ref _cells[i], _columns);
            var cell = new Label($"Cell {i},{_columns - 1}");
            _cells[i][_columns - 1] = cell;
            cell.AddToClassList("cell");
            cell.style.width = _columnWidths[_columns - 1];
            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectCell(cell);
                if (evt.clickCount >= 2) StartEditing(cell);
            });
            _table[i + 1].Add(cell); // +1 to account for the header row
        }
    }

    private void SelectCell(VisualElement cell)
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

    private void FitToCell(VisualElement fit, VisualElement cell)
    {
        var targetRect = GetElementRelativeBound(cell, rootVisualElement);

        fit.style.left = targetRect.x - 1;
        fit.style.top = targetRect.y - 1;
        fit.style.width = targetRect.width;
        fit.style.height = targetRect.height;
    }

    private Rect GetElementRelativeBound(VisualElement element, VisualElement relativeTo)
    {
        var worldBound = element.worldBound;

        var localBound = worldBound;
        var containerWorldBound = relativeTo.worldBound;
        localBound.x -= containerWorldBound.x;
        localBound.y -= containerWorldBound.y;

        return new Rect(localBound.x, localBound.y, localBound.width, localBound.height);
    }
}
