using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SpreadsheetEditorWindow : EditorWindow
{
    private int columns = 5;
    private int rows = 5;
    private float[] columnWidths;
    private VisualElement[] headerCells;
    private VisualElement[][] cells;
    private VisualElement table;
    private VisualElement headerRow;

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

        columnWidths = new float[columns];
        headerCells = new VisualElement[columns];
        cells = new VisualElement[rows][];

        // Initialize column widths
        for (var i = 0; i < columns; i++) columnWidths[i] = 100f;

        // Create a table
        table = new VisualElement();
        table.style.flexDirection = FlexDirection.Column;
        table.style.flexGrow = 1;

        // Create header row with resizable columns
        headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        for (var j = 0; j < columns; j++)
        {
            var headerCell = CreateHeaderCell(j);
            headerCells[j] = headerCell;
            headerRow.Add(headerCell);
        }

        table.Add(headerRow);

        // Create data rows
        for (var i = 0; i < rows; i++)
        {
            var row = CreateDataRow(i);
            table.Add(row);
        }

        rootVisualElement.Add(table);

        // Add buttons for adding rows and columns
        var addButtonRow = new VisualElement();
        addButtonRow.style.flexDirection = FlexDirection.Row;

        var addRowButton = new Button(AddRow) { text = "Add Row", };
        addButtonRow.Add(addRowButton);

        var addColumnButton = new Button(AddColumn) { text = "Add Column", };
        addButtonRow.Add(addColumnButton);

        rootVisualElement.Add(addButtonRow);
    }

    private VisualElement CreateHeaderCell(int columnIndex)
    {
        var headerCell = new VisualElement();
        headerCell.AddToClassList("header-cell");
        headerCell.style.width = columnWidths[columnIndex];

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
        cells[rowIndex] = new VisualElement[columns];
        for (var j = 0; j < columns; j++)
        {
            var cell = new Label($"Cell {rowIndex},{j}");
            cell.AddToClassList("cell");
            cell.style.width = columnWidths[j];
            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectCell(cell);
                if (evt.clickCount >= 2) StartEditing(cell);
            });
            row.Add(cell);
            cells[rowIndex][j] = cell;
        }

        return row;
    }

    private bool isResizing;
    private int resizingColumnIndex = -1;
    private Vector2 initialMousePosition;
    private float initialColumnWidth;

    private void StartResizing(MouseDownEvent evt, int columnIndex)
    {
        isResizing = true;
        resizingColumnIndex = columnIndex;
        initialMousePosition = evt.mousePosition;
        initialColumnWidth = columnWidths[columnIndex];
        rootVisualElement.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        rootVisualElement.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (isResizing)
        {
            var delta = evt.mousePosition.x - initialMousePosition.x;
            columnWidths[resizingColumnIndex] = Mathf.Max(50, initialColumnWidth + delta);

            // Update the header cell width
            headerCells[resizingColumnIndex].style.width = columnWidths[resizingColumnIndex];

            // Update all cells in the same column
            for (var i = 0; i < rows; i++) cells[i][resizingColumnIndex].style.width = columnWidths[resizingColumnIndex];
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (isResizing)
        {
            isResizing = false;
            rootVisualElement.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            rootVisualElement.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }
    }

    private void StartEditing(Label cell)
    {
        var textField = new TextField { value = cell.text, };
        var columnIndex = cell.parent.IndexOf(cell);
        textField.style.width = columnWidths[columnIndex];
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
        rows++;
        var row = CreateDataRow(rows - 1);
        table.Add(row);
    }

    private void AddColumn()
    {
        columns++;
        Array.Resize(ref columnWidths, columns);
        Array.Resize(ref headerCells, columns);

        // Add new header cell
        var headerCell = CreateHeaderCell(columns - 1);
        headerCells[columns - 1] = headerCell;
        headerRow.Add(headerCell);

        // Add new cells to existing rows
        foreach (var row in cells)
        {
            var cell = new Label($"Cell {Array.IndexOf(cells, row)},{columns - 1}");
            cell.AddToClassList("cell");
            cell.style.width = columnWidths[columns - 1];
            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectCell(cell);
                if (evt.clickCount >= 2) StartEditing(cell);
            });
            row[Array.IndexOf(row, row[0])].parent.Add(cell);
        }

        // Resize cells array to include the new column
        for (var i = 0; i < rows; i++)
        {
            Array.Resize(ref cells[i], columns);
            var cell = new Label($"Cell {i},{columns - 1}");
            cells[i][columns - 1] = cell;
            cell.AddToClassList("cell");
            cell.style.width = columnWidths[columns - 1];
            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectCell(cell);
                if (evt.clickCount >= 2) StartEditing(cell);
            });
            table[i + 1].Add(cell); // +1 to account for the header row
        }
    }

    private VisualElement _selector;

    private void SelectCell(VisualElement cell)
    {
        if (_selector == null)
        {
            _selector = new VisualElement();
            _selector.AddToClassList("selector");
            _selector.pickingMode = PickingMode.Ignore;
            _selector.style.position = Position.Absolute;
        }

        var targetRect = GetElementRelativeBound(cell, rootVisualElement);

        _selector.style.left = targetRect.x - 1;
        _selector.style.top = targetRect.y - 1;
        _selector.style.width = targetRect.width;
        _selector.style.height = targetRect.height;

        rootVisualElement.Add(_selector);
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
