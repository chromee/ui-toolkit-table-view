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
        private int _columnCount = 5;
        private int _rowCount = 5;
        private float[] _columnWidths;
        private VisualElement[] _headerCells;
        private Cell[][] _cells;
        private VisualElement _table;
        private VisualElement _headerRow;

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

            _columnWidths = new float[_columnCount];
            _headerCells = new VisualElement[_columnCount];
            _cells = new Cell[_rowCount][];

            // Initialize column widths
            for (var i = 0; i < _columnCount; i++) _columnWidths[i] = 100f;

            // Create a table
            _table = new VisualElement();
            _table.style.flexDirection = FlexDirection.Column;
            _table.style.flexGrow = 1;

            // Create header row with resizable columns
            _headerRow = new VisualElement();
            _headerRow.style.flexDirection = FlexDirection.Row;
            for (var j = 0; j < _columnCount; j++)
            {
                var headerCell = CreateHeaderCell(j);
                _headerCells[j] = headerCell;
                _headerRow.Add(headerCell);
            }

            _table.Add(_headerRow);

            // Create data rows
            for (var i = 0; i < _rowCount; i++)
            {
                var row = CreateDataRow(i);
                _table.Add(row);
            }

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

        #region create

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
            _cells[rowIndex] = new Cell[_columnCount];

            for (var colIndex = 0; colIndex < _columnCount; colIndex++)
            {
                Cell cell;

                if (colIndex == 0) cell = CreateCell(rowIndex, colIndex, $"Cell {rowIndex},{colIndex}");
                else if (colIndex == 1) cell = CreateCell(rowIndex, colIndex, rowIndex);
                else if (colIndex == 2) cell = CreateCell(rowIndex, colIndex, rowIndex * 1.1f);
                else cell = CreateCell(rowIndex, colIndex, rowIndex % 2 == 0);

                if (cell != null) row.Add(cell.Element);
            }

            return row;
        }

        private Cell CreateCell<T>(int rowIndex, int colIndex, T value)
        {
            var cell = Cell.Create(rowIndex, colIndex, value, _columnWidths[colIndex]);

            cell.Element.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) StartSelecting(cell);
                if (evt.clickCount >= 2) cell.StartEditing();
            });

            cell.Element.RegisterCallback<MouseEnterEvent>(_ =>
            {
                Selecting(cell);
            });

            _cells[rowIndex][colIndex] = cell;

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
            _selectMarker.Element.style.display = DisplayStyle.Flex;
            _selectRangeMarker.Element.style.display = DisplayStyle.None;
            _isSelecting = true;
        }

        private void Selecting(Cell cell)
        {
            if (!_isSelecting || _startSelectedCell == null || cell == null) return;

            _endSelectedCell = cell;
            _selectRangeMarker.Fit(_startSelectedCell, _endSelectedCell);
            _selectRangeMarker.Element.style.display = DisplayStyle.Flex;
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
            _selectMarker.Element.style.display = DisplayStyle.None;
            _selectRangeMarker.Element.style.display = DisplayStyle.None;
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
                for (var j = left; j <= right; j++) selectedCells[i - top][j - left] = _cells[i][j];
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
            _copyMarker.Element.style.display = DisplayStyle.Flex;
        }

        private void PasteCell()
        {
            if (_copiedCell == null) return;

            var selectedCells = GetSelectedCells();
            if (selectedCells == null || !selectedCells.Any()) return;

            foreach (var row in selectedCells)
            foreach (var cell in row)
            {
                cell.PasteFrom(_copiedCell);
            }
        }

        private void CancelCopy()
        {
            _copiedCell = null;
            _copyMarker.Element.style.display = DisplayStyle.None;
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

            // Update the header cell width
            _headerCells[_resizingColumnIndex].style.width = _columnWidths[_resizingColumnIndex];

            // Update all cells in the same column
            for (var i = 0; i < _rowCount; i++)
            {
                var cell = _cells[i][_resizingColumnIndex];
                cell.Width = _columnWidths[_resizingColumnIndex];
                if (cell == _startSelectedCell) _selectMarker.Fit(cell);
                if (IsSelected(cell)) _selectRangeMarker.Fit(_startSelectedCell, _endSelectedCell);
                if (cell == _copiedCell) _copyMarker.Fit(cell);
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
    }

    #region elements

    public abstract class Cell
    {
        public readonly VisualElement Element;
        public readonly int Row;
        public readonly int Col;
        public Vector2 Position => new(Col, Row);

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
        public VisualElement Body;

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

        public Cell(int row, int col, T value, float width = 100f) : base(row, col, width)
        {
            Value = value;
        }

        private void RefreshView()
        {
            Element.Clear();

            if (typeof(T) == typeof(string)) Body = new Label { text = Convert.ToString(Value) };
            else if (typeof(T) == typeof(int)) Body = new Label { text = Convert.ToInt32(Value).ToString() };
            else if (typeof(T) == typeof(float)) Body = new Label { text = Convert.ToSingle(Value).ToString(CultureInfo.InvariantCulture) };
            else if (typeof(T) == typeof(bool))
            {
                var toggle = new Toggle { text = string.Empty, value = Convert.ToBoolean(Value) };
                toggle.RegisterValueChangedCallback(evt => Value = (T)(object)evt.newValue);
                Body = toggle;
            }

            Element.Add(Body);
        }

        public override void Clear() => Value = default;

        public override void PasteFrom(Cell from)
        {
            if (from.GetType() != GetType()) return;
            var v = from.As<T>().Value;
            Value = v;
        }

        #region editing

        public override void StartEditing()
        {
            if (typeof(T) == typeof(string)) StartEditingAsString();
            else if (typeof(T) == typeof(int)) StartEditingAsInt();
            else if (typeof(T) == typeof(float)) StartEditingAsFloat();
            else if (typeof(T) == typeof(bool)) StartEditingAsBool();
        }

        private void StartEditingAsString()
        {
            var textField = new TextField { value = Convert.ToString(Value), };
            textField.style.width = Width;
            Element.AddToClassList("input-cell");

            Body.RemoveFromHierarchy();
            Element.Add(textField);

            textField.RegisterCallback<FocusOutEvent>(_ =>
            {
                Value = (T)(object)textField.value;
                textField.RemoveFromHierarchy();
                Element.RemoveFromClassList("input-cell");
                Element.Add(Body);
            });

            textField.Focus();
        }

        private void StartEditingAsInt()
        {
            var integerField = new IntegerField { value = Convert.ToInt32(Value), };
            integerField.style.width = Width;
            Element.AddToClassList("input-cell");

            Body.RemoveFromHierarchy();
            Element.Add(integerField);

            integerField.RegisterCallback<FocusOutEvent>(_ =>
            {
                Value = (T)(object)integerField.value;
                integerField.RemoveFromHierarchy();
                Element.RemoveFromClassList("input-cell");
                Element.Add(Body);
            });

            integerField.Focus();
        }

        private void StartEditingAsFloat()
        {
            var floatField = new FloatField { value = Convert.ToSingle(Value), };
            floatField.style.width = Width;
            Element.AddToClassList("input-cell");

            Body.RemoveFromHierarchy();
            Element.Add(floatField);

            floatField.RegisterCallback<FocusOutEvent>(_ =>
            {
                Value = (T)(object)floatField.value;
                floatField.RemoveFromHierarchy();
                Element.RemoveFromClassList("input-cell");
                Element.Add(Body);
            });

            floatField.Focus();
        }

        private static void StartEditingAsBool()
        {
            // Bool は最初から Toggle なので何もしない
        }

        #endregion
    }

    public class Marker
    {
        public readonly VisualElement Element;
        private readonly VisualElement _rootVisualElement;

        public Marker(VisualElement rootVisualElement, string className)
        {
            _rootVisualElement = rootVisualElement;
            Element = new VisualElement();
            Element.AddToClassList(className);
            Element.pickingMode = PickingMode.Ignore;
            Element.style.display = DisplayStyle.None;
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

    public class Col
    {
        public readonly VisualElement Element;
        public readonly List<Cell> Cells = new();

        public float Width
        {
            get => Element.resolvedStyle.width;
            set
            {
                Element.style.width = value;
                foreach (var cell in Cells) cell.Width = value;
            }
        }
    }

    #endregion
}
