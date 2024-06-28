using System;
using System.Collections.Generic;
using System.Globalization;
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

        private Cell _selectedCell;
        private VisualElement _selectMarker;

        private Cell _copiedCell;
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
        }

        private void OnEnable()
        {
            RegisterShortcuts();
            SetupRootVisualElementForKeyboardInput();
        }

        private void OnDisable()
        {
            UnregisterShortcuts();
        }

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
            var cell = Cell.Create(value, _columnWidths[colIndex]);

            cell.Element.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectCell(cell);
                if (evt.clickCount >= 2) cell.StartEditing();
            });

            _cells[rowIndex][colIndex] = cell;

            return cell;
        }

        #endregion

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
            if (_selectedCell == null) return;

            _selectedCell.PasteFrom(_copiedCell);
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
            _selectedCell.Clear();
        }

        #endregion

        #region select

        private void SelectCell(Cell cell)
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
                if (cell == _selectedCell) FitToCell(_selectMarker, cell);
                if (cell == _copiedCell) FitToCell(_copyMarker, cell);
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

        #region marker util

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

        #endregion
    }

    #region cells

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

        public Cell(T value, float width = 100f) : base(width)
        {
            Value = value;
        }

        private void RefreshView()
        {
            Element.Clear();

            if (typeof(T) == typeof(string)) Body = new Label { text = Convert.ToString(Value) };
            else if (typeof(T) == typeof(int)) Body = new Label { text = Convert.ToInt32(Value).ToString() };
            else if (typeof(T) == typeof(float)) Body = new Label { text = Convert.ToSingle(Value).ToString(CultureInfo.InvariantCulture) };
            else if (typeof(T) == typeof(bool)) Body = new Toggle { text = string.Empty, value = Convert.ToBoolean(Value) };

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

    public abstract class Cell
    {
        public readonly VisualElement Element;

        public Cell<T> As<T>() => this as Cell<T>;

        public float Width
        {
            get => Element.resolvedStyle.width;
            set => Element.style.width = value;
        }

        public static Cell Create<T>(T value, float width = 100f)
        {
            return value switch
            {
                string sv => new Cell<string>(sv, width),
                int iv => new Cell<int>(iv, width),
                float fv => new Cell<float>(fv, width),
                bool bv => new Cell<bool>(bv, width),
                _ => new Cell<string>(value.ToString(), width),
            };
        }

        protected Cell(float width = 100f)
        {
            Element = new VisualElement();
            Element.AddToClassList("cell");
            Width = width;
        }

        public abstract void StartEditing();
        public abstract void PasteFrom(Cell from);
        public abstract void Clear();
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
