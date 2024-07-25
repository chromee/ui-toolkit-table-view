using System.Linq;
using Tables.Editor.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tables.Editor.System
{
    public class SelectSystem
    {
        private readonly Table _table;

        public bool IsSelecting { get; private set; }

        public Cell[][] SelectedCells { get; private set; }
        public DataRow[] SelectedRows { get; private set; }

        public Vector2Int StartSelectedCellPosition { get; private set; } = InvalidPosition;
        public Vector2Int EndSelectedCellPosition { get; private set; } = InvalidPosition;
        public int StartSelectedRowIndex { get; private set; } = InvalidRowIndex;
        public int EndSelectedRowIndex { get; private set; } = InvalidRowIndex;

        public Cell StartSelectedCell => IsExistStartSelectedCell ? _table.DataRows[StartSelectedCellPosition.y][StartSelectedCellPosition.x] : null;
        public Cell EndSelectedCell => IsExistEndSelectedCell ? _table.DataRows[EndSelectedCellPosition.y][EndSelectedCellPosition.x] : null;
        public DataRow StartSelectedRow => IsExistStartSelectedRow ? _table.DataRows[StartSelectedRowIndex] : null;
        public DataRow EndSelectedRow => IsExistEndSelectedRow ? _table.DataRows[EndSelectedRowIndex] : null;

        public bool IsExistStartSelectedCell => StartSelectedCellPosition != InvalidPosition;
        public bool IsExistEndSelectedCell => EndSelectedCellPosition != InvalidPosition;
        public bool IsExistStartSelectedRow => StartSelectedRowIndex != InvalidRowIndex;
        public bool IsExistEndSelectedRow => EndSelectedRowIndex != InvalidRowIndex;

        public bool IsStartSelectedCell(Cell cell) => cell.Position == StartSelectedCellPosition;
        public bool IsEndSelectedCell(Cell cell) => cell.Position == EndSelectedCellPosition;
        public bool IsStartSelectedRow(DataRow row) => row.Index == StartSelectedRowIndex;
        public bool IsEndSelectedRow(DataRow row) => row.Index == EndSelectedRowIndex;

        private readonly Marker _selectMarker;
        private readonly Marker _selectRangeMarker;

        private static readonly Vector2Int InvalidPosition = new(-1, -1);
        private const int InvalidRowIndex = -1;

        public SelectSystem(VisualElement rootVisualElement, Table table)
        {
            _table = table;
            _selectMarker = new Marker(rootVisualElement, "select-marker");
            _selectRangeMarker = new Marker(rootVisualElement, "select-range-marker");
        }

        #region cell select

        public void StartSelecting(Cell cell)
        {
            CancelSelecting();

            StartSelectedCellPosition = cell.Position;

            _selectMarker.Fit(cell);
            _selectMarker.IsVisible = true;
            _selectRangeMarker.IsVisible = false;

            IsSelecting = true;
            UpdateSelectedCell();
        }

        public void Selecting(Cell cell)
        {
            if (!IsSelecting || !IsExistStartSelectedCell || cell == null) return;

            EndSelectedCellPosition = cell.Position;

            _selectRangeMarker.Fit(StartSelectedCell, EndSelectedCell);
            _selectRangeMarker.IsVisible = true;

            UpdateSelectedCell();
        }

        public void EndSelecting()
        {
            if (!IsSelecting) return;
            IsSelecting = false;
        }

        private void UpdateSelectedCell()
        {
            SelectedCells = GetSelectedCells();
        }

        // TODO: Spanを使う
        private Cell[][] GetSelectedCells()
        {
            if (IsExistStartSelectedRow)
            {
                if (!IsExistEndSelectedRow) return new[] { StartSelectedRow.Cells };

                var topRowIndex = StartSelectedRowIndex < EndSelectedRowIndex ? StartSelectedRowIndex : EndSelectedRowIndex;
                var bottomRowIndex = StartSelectedRowIndex < EndSelectedRowIndex ? EndSelectedRowIndex : StartSelectedRowIndex;

                var selectedCells = new Cell[bottomRowIndex - topRowIndex + 1][];
                for (var i = topRowIndex; i <= bottomRowIndex; i++)
                {
                    selectedCells[i - topRowIndex] = _table.DataRows[i].Cells;
                }

                return selectedCells;
            }

            if (IsExistStartSelectedCell)
            {
                if (!IsExistEndSelectedCell) return new[] { new[] { StartSelectedCell } };

                var top = Mathf.Min(StartSelectedCellPosition.y, EndSelectedCellPosition.y);
                var bottom = Mathf.Max(StartSelectedCellPosition.y, EndSelectedCellPosition.y);
                var left = Mathf.Min(StartSelectedCellPosition.x, EndSelectedCellPosition.x);
                var right = Mathf.Max(StartSelectedCellPosition.x, EndSelectedCellPosition.x);

                var selectedCells = new Cell[bottom - top + 1][];
                for (var i = top; i <= bottom; i++)
                {
                    selectedCells[i - top] = new Cell[right - left + 1];
                    for (var j = left; j <= right; j++) selectedCells[i - top][j - left] = _table.DataRows[i][j];
                }

                return selectedCells;
            }

            return null;
        }

        #endregion

        #region row select

        public void StartRowSelecting(DataRow row)
        {
            CancelSelecting();

            StartSelectedRowIndex = row.Index;

            _selectMarker.Fit(row.Cells.First());
            _selectMarker.IsVisible = true;
            _selectRangeMarker.Fit(row.Cells.First(), row.Cells.Last());
            _selectRangeMarker.IsVisible = true;

            IsSelecting = true;
            UpdateSelectedRow();
            UpdateSelectedCell();
        }

        public void RowSelecting(DataRow row)
        {
            if (!IsSelecting || !IsExistStartSelectedRow || row == null) return;

            EndSelectedRowIndex = row.Index;

            var topRow = StartSelectedRowIndex < EndSelectedRowIndex ? StartSelectedRow : EndSelectedRow;
            var bottomRow = StartSelectedRowIndex < EndSelectedRowIndex ? EndSelectedRow : StartSelectedRow;
            _selectRangeMarker.Fit(topRow.Cells.First(), bottomRow.Cells.Last());
            _selectRangeMarker.IsVisible = true;

            UpdateSelectedRow();
            UpdateSelectedCell();
        }

        public void EndRowSelecting()
        {
            if (!IsSelecting) return;
            IsSelecting = false;
        }

        private void UpdateSelectedRow()
        {
            SelectedRows = GetSelectedRows();

            foreach (var row in _table.DataRows)
            {
                if (SelectedRows != null && SelectedRows.Contains(row)) row.AddToClassList("selected-row");
                else row.RemoveFromClassList("selected-row");
            }
        }

        private DataRow[] GetSelectedRows()
        {
            if (!IsExistStartSelectedRow) return null;
            if (!IsExistEndSelectedRow) return new[] { StartSelectedRow };

            var topRowIndex = StartSelectedRowIndex < EndSelectedRowIndex ? StartSelectedRowIndex : EndSelectedRowIndex;
            var bottomRowIndex = StartSelectedRowIndex < EndSelectedRowIndex ? EndSelectedRowIndex : StartSelectedRowIndex;

            var selectedRows = new DataRow[bottomRowIndex - topRowIndex + 1];
            for (var i = topRowIndex; i <= bottomRowIndex; i++) selectedRows[i - topRowIndex] = _table.DataRows[i];

            return selectedRows;
        }

        #endregion

        #region arraw select

        public void SelectUp()
        {
            EndSelecting();
            if (!IsExistStartSelectedCell) return;
            var row = Mathf.Max(0, StartSelectedCellPosition.y - 1);
            SelectCell(_table.DataRows[row][StartSelectedCellPosition.x]);
        }

        public void SelectDown()
        {
            EndSelecting();
            if (!IsExistStartSelectedCell) return;
            var row = Mathf.Min(_table.DataRows.Count - 1, StartSelectedCellPosition.y + 1);
            SelectCell(_table.DataRows[row][StartSelectedCellPosition.x]);
        }

        public void SelectLeft()
        {
            EndSelecting();
            if (!IsExistStartSelectedCell) return;
            var row = _table.DataRows[StartSelectedCellPosition.y];
            var col = Mathf.Max(0, StartSelectedCellPosition.x - 1);
            SelectCell(row[col]);
        }

        public void SelectRight()
        {
            EndSelecting();
            if (!IsExistStartSelectedCell) return;
            var row = _table.DataRows[StartSelectedCellPosition.y];
            var col = Mathf.Min(row.Cells.Length - 1, StartSelectedCellPosition.x + 1);
            SelectCell(_table.DataRows[StartSelectedCellPosition.y][col]);
        }

        private void SelectCell(Cell cell)
        {
            StartSelectedCellPosition = cell.Position;
            _selectMarker.Fit(cell);
            _selectMarker.IsVisible = true;
            _selectRangeMarker.IsVisible = false;
        }

        #endregion

        public void CancelSelecting()
        {
            StartSelectedCellPosition = InvalidPosition;
            EndSelectedCellPosition = InvalidPosition;
            StartSelectedRowIndex = InvalidRowIndex;
            EndSelectedRowIndex = InvalidRowIndex;
            _selectMarker.IsVisible = false;
            _selectRangeMarker.IsVisible = false;
            UpdateSelectedCell();
            UpdateSelectedRow();
        }

        public bool IsSelected(Cell cell)
        {
            if (!IsExistStartSelectedCell) return false;
            if (!IsExistStartSelectedCell) return cell.Position == StartSelectedCellPosition;

            var top = Mathf.Min(StartSelectedCellPosition.y, EndSelectedCellPosition.y);
            var bottom = Mathf.Max(StartSelectedCellPosition.y, EndSelectedCellPosition.y);
            var left = Mathf.Min(StartSelectedCellPosition.x, EndSelectedCellPosition.x);
            var right = Mathf.Max(StartSelectedCellPosition.x, EndSelectedCellPosition.x);

            return cell.Row >= top && cell.Row <= bottom &&
                   cell.Col >= left && cell.Col <= right;
        }

        public bool IsSelected(DataRow dataRow)
        {
            if (!IsExistEndSelectedRow) return false;
            if (!IsExistEndSelectedRow) return dataRow.Index == StartSelectedRowIndex;

            var top = Mathf.Min(StartSelectedRowIndex, EndSelectedRowIndex);
            var bottom = Mathf.Max(StartSelectedRowIndex, EndSelectedRowIndex);

            return dataRow.Index >= top && dataRow.Index <= bottom;
        }

        public void FitSelectMarker()
        {
            _selectMarker.Fit(StartSelectedCell);
        }

        public void FitRangeMarker()
        {
            _selectMarker.Fit(StartSelectedCell, EndSelectedCell);
        }
    }
}
