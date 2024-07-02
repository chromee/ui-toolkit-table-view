using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Editor.Utilities;
using Editor.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class SpreadsheetEditorWindow : EditorWindow
    {
        private Table _table;
        private VisualElement[] _headerCells;
        private readonly List<Row> _dataRows = new();
        private Cell[] _emptyRow;
        private float[] _columnWidths;

        private bool _isSelecting;
        private Cell _startSelectedCell;
        private Cell _endSelectedCell;
        private Marker _selectMarker;
        private Marker _selectRangeMarker;

        private Cell _copiedCell;
        private Marker _copyMarker;

        #region mtd

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

            var tableInfo = new TableInfo("Table", new[]
            {
                new ColInfo(typeof(string), "String Value", 100f),
                new ColInfo(typeof(int), "Int Value", 100f),
                new ColInfo(typeof(float), "Float Value", 100f),
                new ColInfo(typeof(bool), "✓", 30f),
                new ColInfo(typeof(string), "String Value 2", 200f),
            });

            _table = CreateTable(tableInfo);
            rootVisualElement.Add(_table);

            _selectMarker = new Marker(rootVisualElement, "select-marker");
            _selectRangeMarker = new Marker(rootVisualElement, "select-range-marker");
            _copyMarker = new Marker(rootVisualElement, "copy-marker");
        }

        private void OnEnable()
        {
            RegisterShortcuts();
            SetupRootVisualElementForKeyboardInput();
            SetupRootVisualElementForSelection();
        }

        private void OnDisable()
        {
            UnregisterShortcuts();
        }

        // デバッグ用
        // private void OnGUI()
        // {
        //     Debug.Log(rootVisualElement.panel.focusController.focusedElement);
        // }

        #endregion

        #region create

        private Table CreateTable(TableInfo tableInfo, int rowCount = 10)
        {
            var table = new Table();
            var colLength = tableInfo.ColInfos.Length;
            _headerCells = new VisualElement[colLength];
            _columnWidths = new float[colLength];
            for (var i = 0; i < colLength; i++) _columnWidths[i] = tableInfo.ColInfos[i].Width;

            table.Add(CreateHeaderRow(tableInfo.ColInfos));

            // Create data rows
            for (var i = 0; i < rowCount; i++)
            {
                var row = CreateDataRow(tableInfo.ColInfos);
                table.Add(row);
            }

            // Create empty row
            var emptyRow = CreateEmptyRow(tableInfo.ColInfos);
            table.Add(emptyRow);

            return table;
        }

        private VisualElement CreateHeaderRow(ColInfo[] colInfos)
        {
            var headerRow = new VisualElement();
            headerRow.AddToClassList("row");

            var topIndexCell = new VisualElement();
            topIndexCell.AddToClassList("cell");
            topIndexCell.AddToClassList("index-cell");
            topIndexCell.AddToClassList("top-index-cell");
            headerRow.Add(topIndexCell);

            for (var i = 0; i < colInfos.Length; i++)
            {
                var headerCell = CreateHeaderCell(colInfos[i], i);
                _headerCells[i] = headerCell;
                headerRow.Add(headerCell);
            }

            return headerRow;
        }

        private Row CreateDataRow(ColInfo[] colInfos, object[] values = null)
        {
            var rowIndex = _dataRows.Count;

            var row = new Row(rowIndex);
            _dataRows.Add(row);

            var indexCell = new VisualElement();
            indexCell.AddToClassList("cell");
            indexCell.AddToClassList("index-cell");
            indexCell.Add(new Label(_dataRows.Count.ToString()));
            row.Add(indexCell);

            for (var i = 0; i < colInfos.Length; i++)
            {
                var colInfo = colInfos[i];

                var cell = Type.GetTypeCode(colInfo.Type) switch
                {
                    TypeCode.String => CreateCell(rowIndex, i, values != null ? values[i] : $"Cell {rowIndex},{i}"),
                    TypeCode.Int32 => CreateCell(rowIndex, i, values != null ? values[i] : rowIndex),
                    TypeCode.Single => CreateCell(rowIndex, i, values != null ? values[i] : rowIndex * 1.1f),
                    TypeCode.Boolean => CreateCell(rowIndex, i, values != null ? values[i] : rowIndex % 2 == 0),
                    _ => CreateCell(rowIndex, i, values != null ? values[i] : $"Cell {rowIndex},{i}"),
                };

                cell.OnValueChangedFromEdit += (from, to) =>
                {
                    AddUndoCommand(cell, from, to);
                    rootVisualElement.ExecAfter1Frame(() => rootVisualElement.Focus());
                };
                row.AddCell(cell);
            }

            return row;
        }

        private VisualElement CreateEmptyRow(ColInfo[] colInfos)
        {
            _emptyRow = new Cell[colInfos.Length];

            var emptyRow = new VisualElement();
            emptyRow.AddToClassList("row");

            var addRowButton = new Button { text = "+" };
            addRowButton.AddToClassList("add-row-button");
            addRowButton.clicked += () =>
            {
                var newRow = CreateDataRow(colInfos, _emptyRow.Select(cell => cell.Val).ToArray());
                _table.Insert(_table.IndexOf(emptyRow), newRow);
            };
            emptyRow.Add(addRowButton);

            var rowIndex = _dataRows.Count - 1;
            for (var i = 0; i < colInfos.Length; i++)
            {
                var colInfo = colInfos[i];

                var cell = Type.GetTypeCode(colInfo.Type) switch
                {
                    TypeCode.String => CreateCell(rowIndex, i, string.Empty),
                    TypeCode.Int32 => CreateCell(rowIndex, i, 0),
                    TypeCode.Single => CreateCell(rowIndex, i, 0f),
                    TypeCode.Boolean => CreateCell(rowIndex, i, false),
                    _ => CreateCell(0, i, string.Empty),
                };

                _emptyRow[i] = cell;

                emptyRow.Add(cell);
                cell.OnValueChangedFromEdit += (from, to) =>
                {
                    AddUndoCommand(cell, from, to);
                    rootVisualElement.ExecAfter1Frame(() => rootVisualElement.Focus());
                };
            }

            return emptyRow;
        }

        private VisualElement CreateHeaderCell(ColInfo colInfo, int columnIndex)
        {
            var headerCell = new VisualElement();
            headerCell.AddToClassList("header-cell");
            headerCell.style.width = _columnWidths[columnIndex];

            var headerLabel = new Label(colInfo.Name);
            headerCell.Add(headerLabel);

            var resizer = new VisualElement();
            resizer.AddToClassList("header-cell-resize-handle");
            resizer.RegisterCallback<MouseDownEvent>(evt => StartResizing(evt, columnIndex));
            headerCell.Add(resizer);

            return headerCell;
        }

        private Cell CreateCell<T>(int rowIndex, int colIndex, T value)
        {
            var cell = Cell.Create(rowIndex, colIndex, value, _columnWidths[colIndex]);

            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) StartSelecting(cell);
                if (evt.clickCount >= 2) cell.StartEditing();
            });

            cell.RegisterCallback<MouseEnterEvent>(_ => { Selecting(cell); });

            return cell;
        }

        #endregion

        #region select

        private void SetupRootVisualElementForSelection()
        {
            rootVisualElement.RegisterCallback<MouseUpEvent>(EndSelecting);
        }

        private void StartSelecting(Cell cell)
        {
            _startSelectedCell = cell;
            _selectMarker.Fit(cell);
            _selectMarker.IsVisible = true;
            _selectRangeMarker.IsVisible = false;
            _isSelecting = true;
        }

        private void Selecting(Cell cell)
        {
            if (!_isSelecting || _startSelectedCell == null || cell == null) return;

            _endSelectedCell = cell;
            _selectRangeMarker.Fit(_startSelectedCell, _endSelectedCell);
            _selectRangeMarker.IsVisible = true;
        }

        private void EndSelecting(MouseUpEvent _)
        {
            if (!_isSelecting) return;
            _isSelecting = false;
        }

        private void CancelSelecting()
        {
            _startSelectedCell = null;
            _endSelectedCell = null;
            _selectMarker.IsVisible = false;
            _selectRangeMarker.IsVisible = false;
        }

        private Cell[][] GetSelectedCells()
        {
            if (_startSelectedCell == null) return null;
            if (_endSelectedCell == null) return new[] { new[] { _startSelectedCell } };

            var top = Mathf.Min(_startSelectedCell.Row, _endSelectedCell.Row);
            var bottom = Mathf.Max(_startSelectedCell.Row, _endSelectedCell.Row);
            var left = Mathf.Min(_startSelectedCell.Col, _endSelectedCell.Col);
            var right = Mathf.Max(_startSelectedCell.Col, _endSelectedCell.Col);

            var selectedCells = new Cell[bottom - top + 1][];

            for (var i = top; i <= bottom; i++)
            {
                selectedCells[i - top] = new Cell[right - left + 1];
                for (var j = left; j <= right; j++) selectedCells[i - top][j - left] = _dataRows[i][j];
            }

            return selectedCells;
        }

        private bool IsSelected(Cell cell)
        {
            if (_startSelectedCell == null) return false;
            if (_endSelectedCell == null) return cell == _startSelectedCell;

            var top = Mathf.Min(_startSelectedCell.Row, _endSelectedCell.Row);
            var bottom = Mathf.Max(_startSelectedCell.Row, _endSelectedCell.Row);
            var left = Mathf.Min(_startSelectedCell.Col, _endSelectedCell.Col);
            var right = Mathf.Max(_startSelectedCell.Col, _endSelectedCell.Col);

            return cell.Row >= top && cell.Row <= bottom &&
                   cell.Col >= left && cell.Col <= right;
        }

        #endregion

        #region copy & paste

        private void CopyCell()
        {
            _copiedCell = _startSelectedCell;
            _copyMarker.Fit(_copiedCell);
            _copyMarker.IsVisible = true;
        }

        private void PasteCell()
        {
            if (_copiedCell == null) return;

            var selectedCells = GetSelectedCells();
            if (selectedCells == null || !selectedCells.Any()) return;

            var commandSet = new CommandSet();
            foreach (var row in selectedCells)
            foreach (var cell in row)
            {
                var prev = cell.Val;
                if (!cell.TryPaste(_copiedCell)) return;
                commandSet.Commands.Add(new CommandSet.Command { Cell = cell, From = prev, To = _copiedCell.Val });
            }

            if (commandSet.Commands.Any()) AddUndoCommand(commandSet);
        }

        private void CancelCopy()
        {
            _copiedCell = null;
            _copyMarker.IsVisible = false;
        }

        #endregion

        #region undo & redo

        private readonly Stack<CommandSet> _undoStack = new();
        private readonly Stack<CommandSet> _redoStack = new();

        private void AddUndoCommand(Cell cell, object from, object to)
        {
            var command = new CommandSet.Command { Cell = cell, From = from, To = to };
            _undoStack.Push(new CommandSet(command));
            _redoStack.Clear();
        }

        private void AddUndoCommand(CommandSet commandSet)
        {
            _undoStack.Push(commandSet);
            _redoStack.Clear();
        }

        private void Undo()
        {
            if (_undoStack.Count == 0) return;

            var commandSet = _undoStack.Pop();
            commandSet.Undo();
            _redoStack.Push(commandSet);
            rootVisualElement.Focus();
        }

        private void Redo()
        {
            if (_redoStack.Count == 0) return;

            var commandSet = _redoStack.Pop();
            commandSet.Redo();
            _undoStack.Push(commandSet);
            rootVisualElement.Focus();
        }

        private class CommandSet
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

        #endregion

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
            _headerCells[_resizingColumnIndex].style.width = _columnWidths[_resizingColumnIndex];
            _emptyRow[_resizingColumnIndex].Width = _columnWidths[_resizingColumnIndex];

            foreach (var row in _dataRows)
            {
                var cell = row[_resizingColumnIndex];
                cell.Width = _columnWidths[_resizingColumnIndex];
                if (_selectMarker.IsVisible) _selectMarker.Fit(_startSelectedCell);
                if (_selectRangeMarker.IsVisible) _selectRangeMarker.Fit(_startSelectedCell, _endSelectedCell);
                if (_copyMarker.IsVisible) _copyMarker.Fit(_copiedCell);
            }
        }

        private void StopResizing(MouseUpEvent evt)
        {
            if (!_isResizing) return;

            _isResizing = false;
            rootVisualElement.UnregisterCallback<MouseMoveEvent>(Resizing);
            rootVisualElement.UnregisterCallback<MouseUpEvent>(StopResizing);
        }

        #endregion

        #region shortcuts

        private void SetupRootVisualElementForKeyboardInput()
        {
            rootVisualElement.focusable = true;
            rootVisualElement.pickingMode = PickingMode.Position;
            rootVisualElement.Focus();
        }

        private void RegisterShortcuts()
        {
            rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void UnregisterShortcuts()
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent ev)
        {
            if (ev.keyCode == KeyCode.C && Event.current.control) CopyCell();
            else if (ev.keyCode == KeyCode.V && Event.current.control) PasteCell();
            else if (ev.keyCode == KeyCode.Z && Event.current.control && Event.current.shift) Redo();
            else if (ev.keyCode == KeyCode.Z && Event.current.control) Undo();
            else if (ev.keyCode == KeyCode.Y && Event.current.control) Redo();
            else if (ev.keyCode == KeyCode.Escape) CancelAll();
            else _startSelectedCell?.StartEditingByKeyDown(ev);
        }

        private void CancelAll()
        {
            CancelCopy();
            CancelSelecting();
        }

        #endregion
    }

    #region info

    public class TableInfo
    {
        public readonly string Name;
        public readonly ColInfo[] ColInfos;

        public TableInfo(string name, ColInfo[] colInfos)
        {
            Name = name;
            ColInfos = colInfos;
        }
    }

    public class ColInfo
    {
        public readonly Type Type;
        public readonly string Name;
        public readonly float Width;

        public ColInfo(Type type, string name, float width)
        {
            Type = type;
            Name = name;
            Width = width;
        }
    }

    #endregion
}
