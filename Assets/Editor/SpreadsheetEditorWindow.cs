using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class SpreadsheetEditorWindow : EditorWindow
    {
        private VisualElement[] _headerCells;
        private readonly List<Cell[]> _dataRows = new();
        private Cell[] _emptyRow;
        private float[] _columnWidths;

        private bool _isSelecting;
        private Cell _startSelectedCell;
        private Cell _endSelectedCell;
        private Marker _selectMarker;
        private Marker _selectRangeMarker;

        private Cell _copiedCell;
        private Marker _copyMarker;

        private readonly HashSet<KeyCode> _pressedKeys = new();

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
                new ColInfo(typeof(string), "String", 100f),
                new ColInfo(typeof(int), "Int", 100f),
                new ColInfo(typeof(float), "Float", 100f),
                new ColInfo(typeof(bool), "Bool", 100f),
                new ColInfo(typeof(string), "String", 100f),
            });

            rootVisualElement.Add(CreateTable(tableInfo));

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

        #endregion

        #region create

        private VisualElement CreateTable(TableInfo tableInfo, int rowCount = 10)
        {
            var table = new VisualElement();
            table.AddToClassList("table");

            var colLength = tableInfo.ColInfos.Length;

            _headerCells = new VisualElement[colLength];
            _columnWidths = new float[colLength];
            for (var i = 0; i < colLength; i++) _columnWidths[i] = 100f;

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

        private VisualElement CreateDataRow(ColInfo[] colInfos)
        {
            _dataRows.Add(new Cell[colInfos.Length]);
            var rowIndex = _dataRows.Count - 1;

            var row = new VisualElement();
            row.AddToClassList("row");

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
                    TypeCode.String => CreateCell(rowIndex, i, $"Cell {rowIndex},{i}"),
                    TypeCode.Int32 => CreateCell(rowIndex, i, rowIndex),
                    TypeCode.Single => CreateCell(rowIndex, i, rowIndex * 1.1f),
                    TypeCode.Boolean => CreateCell(rowIndex, i, rowIndex % 2 == 0),
                    _ => CreateCell(rowIndex, i, $"Cell {rowIndex},{i}"),
                };

                _dataRows[rowIndex][i] = cell;

                row.Add(cell.Element);
                cell.OnValueChangedFromEdit += (from, to) => AddUndoCommand(cell, from, to);
            }

            return row;
        }

        private VisualElement CreateEmptyRow(ColInfo[] colInfos)
        {
            _emptyRow = new Cell[colInfos.Length];

            var row = new VisualElement();
            row.AddToClassList("row");

            var addRowButton = new Button { text = "+" };
            addRowButton.AddToClassList("add-row-button");
            row.Add(addRowButton);

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

                row.Add(cell.Element);
                cell.OnValueChangedFromEdit += (from, to) => AddUndoCommand(cell, from, to);
            }

            return row;
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

            cell.Element.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) StartSelecting(cell);
                if (evt.clickCount >= 2) cell.StartEditing();
            });

            cell.Element.RegisterCallback<MouseEnterEvent>(_ => { Selecting(cell); });

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
                commandSet.Commands.Add(new CommandSet.Command { Cell = cell, From = cell.Val, To = _copiedCell.Val });
                cell.PasteFrom(_copiedCell);
            }

            AddUndoCommand(commandSet);
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

                    if (From is string fromS) Cell.As<string>().Value = fromS;
                    else if (From is int fromI) Cell.As<int>().Value = fromI;
                    else if (From is float fromF) Cell.As<float>().Value = fromF;
                    else if (From is bool fromB) Cell.As<bool>().Value = fromB;
                }

                public void Redo()
                {
                    if (Cell == null) return;

                    if (To is string toS) Cell.As<string>().Value = toS;
                    else if (To is int toI) Cell.As<int>().Value = toI;
                    else if (To is float toF) Cell.As<float>().Value = toF;
                    else if (To is bool toB) Cell.As<bool>().Value = toB;
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
            rootVisualElement.RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
        }

        private void UnregisterShortcuts()
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            rootVisualElement.UnregisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
        }

        private void OnKeyDown(KeyDownEvent ev)
        {
            _pressedKeys.Add(ev.keyCode);
            if (_pressedKeys.Contains(KeyCode.LeftControl) && _pressedKeys.Contains(KeyCode.C)) CopyCell();
            else if (_pressedKeys.Contains(KeyCode.LeftControl) && _pressedKeys.Contains(KeyCode.V)) PasteCell();
            else if (_pressedKeys.Contains(KeyCode.LeftControl) && _pressedKeys.Contains(KeyCode.LeftShift) && _pressedKeys.Contains(KeyCode.Z)) Redo();
            else if (_pressedKeys.Contains(KeyCode.LeftControl) && _pressedKeys.Contains(KeyCode.Z)) Undo();
            else if (_pressedKeys.Contains(KeyCode.LeftControl) && _pressedKeys.Contains(KeyCode.Y)) Redo();
            else if (_pressedKeys.Contains(KeyCode.Escape)) CancelAll();
        }

        private void OnKeyUp(KeyUpEvent ev)
        {
            if (_pressedKeys.Contains(ev.keyCode)) _pressedKeys.Remove(ev.keyCode);
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

    #region elements

    public abstract class Cell
    {
        public readonly VisualElement Element;
        public readonly int Row;
        public readonly int Col;
        public Vector2 Position => new(Col, Row);

        public abstract object Val { get; }
        public abstract event Action<object, object> OnValueChangedFromEdit;

        public Cell<T> As<T>() => this as Cell<T>;

        public float Width
        {
            get => Element.resolvedStyle.width;
            set => Element.style.width = value;
        }

        public static Cell Create<T>(int row, int col, T value, float width = 100f)
        {
            return value switch
            {
                string sv => new Cell<string>(row, col, sv, width),
                int iv => new Cell<int>(row, col, iv, width),
                float fv => new Cell<float>(row, col, fv, width),
                bool bv => new Cell<bool>(row, col, bv, width),
                _ => new Cell<string>(row, col, value.ToString(), width),
            };
        }

        protected Cell(int row, int col, float width = 100f)
        {
            Element = new VisualElement();
            Element.AddToClassList("cell");
            Row = row;
            Col = col;
            Width = width;
        }

        public abstract void StartEditing();
        public abstract void PasteFrom(Cell from);
        public abstract void Clear();
    }

    public class Cell<T> : Cell
    {
        private VisualElement _body;

        public override event Action<object, object> OnValueChangedFromEdit;

        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                RefreshView();
            }
        }

        public override object Val => Value;

        public Cell(int row, int col, T value, float width = 100f) : base(row, col, width)
        {
            Value = value;
        }

        private void RefreshView()
        {
            Element.Clear();

            if (typeof(T) == typeof(string)) _body = new Label { text = Convert.ToString(Value) };
            else if (typeof(T) == typeof(int)) _body = new Label { text = Convert.ToInt32(Value).ToString() };
            else if (typeof(T) == typeof(float)) _body = new Label { text = Convert.ToSingle(Value).ToString(CultureInfo.InvariantCulture) };
            else if (typeof(T) == typeof(bool)) _body = CreateEditingBodyAsBool();
            else _body = new Label { text = Convert.ToString(Value) };

            Element.Add(_body);
        }

        public override void Clear() => Value = default;

        public override void PasteFrom(Cell from)
        {
            if (from.GetType() != GetType()) return;
            var v = from.As<T>().Value;
            Value = v;
        }

        #region create body

        private VisualElement CreateEditingBodyAsBool()
        {
            // Bool は常時 Toggle
            var toggle = new Toggle { text = string.Empty, value = Convert.ToBoolean(Value) };
            toggle.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = (T)(object)evt.newValue;
                OnValueChangedFromEdit?.Invoke(prev, Value);
            });

            return toggle;
        }

        #endregion

        #region editing

        public override void StartEditing()
        {
            if (typeof(T) == typeof(string)) StartEditingAsString();
            else if (typeof(T) == typeof(int)) StartEditingAsInt();
            else if (typeof(T) == typeof(float)) StartEditingAsFloat();
            // else if (typeof(T) == typeof(bool)) StartEditingAsBool(); // Bool は常時 Toggle なので切り替え不要
        }

        private void StartEditingAsString()
        {
            var textField = new TextField { value = Convert.ToString(Value), };
            textField.style.width = Width;
            Element.AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Element.Add(textField);

            textField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = (T)(object)textField.value;
                OnValueChangedFromEdit?.Invoke(prev, Value);
                textField.RemoveFromHierarchy();
                Element.RemoveFromClassList("input-cell");
                Element.Add(_body);
            });

            textField.Focus();
        }

        private void StartEditingAsInt()
        {
            var integerField = new IntegerField { value = Convert.ToInt32(Value), };
            integerField.style.width = Width;
            Element.AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Element.Add(integerField);

            integerField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = (T)(object)integerField.value;
                OnValueChangedFromEdit?.Invoke(prev, Value);
                integerField.RemoveFromHierarchy();
                Element.RemoveFromClassList("input-cell");
                Element.Add(_body);
            });

            integerField.Focus();
        }

        private void StartEditingAsFloat()
        {
            var floatField = new FloatField { value = Convert.ToSingle(Value), };
            floatField.style.width = Width;
            Element.AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Element.Add(floatField);

            floatField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = (T)(object)floatField.value;
                OnValueChangedFromEdit?.Invoke(prev, Value);
                floatField.RemoveFromHierarchy();
                Element.RemoveFromClassList("input-cell");
                Element.Add(_body);
            });

            floatField.Focus();
        }

        #endregion
    }

    public class Marker
    {
        public readonly VisualElement Element;
        private readonly VisualElement _rootVisualElement;

        public bool IsVisible
        {
            get => Element.style.display == DisplayStyle.Flex;
            set => Element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public Marker(VisualElement rootVisualElement, string className)
        {
            _rootVisualElement = rootVisualElement;
            Element = new VisualElement();
            Element.AddToClassList(className);
            Element.pickingMode = PickingMode.Ignore;
            IsVisible = false;
            _rootVisualElement.Add(Element);
        }

        public void Fit(Cell cell)
        {
            if (cell == null) return;
            FitTopLeftToBotRight(cell, cell);
        }

        public void Fit(Cell startCell, Cell endCell)
        {
            if (startCell == null) return;

            if (endCell == null) Fit(startCell);
            else if (startCell.Position == endCell.Position) Fit(startCell);
            else if (startCell.Row <= endCell.Row) // startが上
            {
                if (startCell.Col <= endCell.Col) FitTopLeftToBotRight(startCell, endCell);
                else FitTopRightToBotLeft(startCell, endCell);
            }
            else // endが上
            {
                if (endCell.Col <= startCell.Col) FitTopLeftToBotRight(endCell, startCell);
                else FitTopRightToBotLeft(endCell, startCell);
            }
        }

        private void FitTopLeftToBotRight(Cell leftTop, Cell rightBot)
        {
            if (leftTop == null || rightBot == null) return;

            var startPos = leftTop.Element.worldBound.position;
            var endPos = rightBot.Element.worldBound.position + rightBot.Element.worldBound.size;
            var rootBound = _rootVisualElement.worldBound;

            Element.style.left = startPos.x - rootBound.x;
            Element.style.top = startPos.y - rootBound.y;
            Element.style.width = endPos.x - startPos.x - 1;
            Element.style.height = endPos.y - startPos.y - 1;
        }

        private void FitTopRightToBotLeft(Cell rightTop, Cell leftBot)
        {
            if (rightTop == null || leftBot == null) return;

            var leftBotPos = leftBot.Element.worldBound.position;
            var rightTopPos = rightTop.Element.worldBound.position;
            var startPos = new Vector2(leftBotPos.x, rightTopPos.y);
            var endPos = new Vector2(rightTopPos.x + rightTop.Element.worldBound.size.x, leftBotPos.y + leftBot.Element.worldBound.size.y);
            var rootBound = _rootVisualElement.worldBound;

            Element.style.left = startPos.x - rootBound.x;
            Element.style.top = startPos.y - rootBound.y;
            Element.style.width = endPos.x - startPos.x - 1;
            Element.style.height = endPos.y - startPos.y - 1;
        }
    }

    #endregion
}
