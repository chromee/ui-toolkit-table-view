using System.Linq;
using Editor.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.System
{
    public class SelectSystem
    {
        private readonly Table _table;

        public bool IsSelecting { get; private set; }
        public Cell StartSelectedCell { get; private set; }
        public Cell EndSelectedCell { get; private set; }
        public DataRow StartSelectedRow { get; private set; }
        public DataRow EndSelectedRow { get; private set; }

        private readonly Marker _selectMarker;
        private readonly Marker _selectRangeMarker;

        public SelectSystem(VisualElement rootVisualElement, Table table)
        {
            _table = table;
            _selectMarker = new Marker(rootVisualElement, "select-marker");
            _selectRangeMarker = new Marker(rootVisualElement, "select-range-marker");
        }

        public void StartSelecting(Cell cell)
        {
            StartSelectedCell = cell;
            _selectMarker.Fit(cell);
            _selectMarker.IsVisible = true;
            _selectRangeMarker.IsVisible = false;
            IsSelecting = true;
        }

        public void Selecting(Cell cell)
        {
            if (!IsSelecting || StartSelectedCell == null || cell == null) return;

            EndSelectedCell = cell;
            _selectRangeMarker.Fit(StartSelectedCell, EndSelectedCell);
            _selectRangeMarker.IsVisible = true;
        }

        public void EndSelecting()
        {
            if (!IsSelecting) return;
            IsSelecting = false;
        }

        public void StartRowSelecting(DataRow row)
        {
            StartSelectedRow = row;
            _selectMarker.Fit(row.Cells.First());
            _selectMarker.IsVisible = true;
            _selectRangeMarker.Fit(row.Cells.First(), row.Cells.Last());
            _selectRangeMarker.IsVisible = true;
            IsSelecting = true;
        }

        public void RowSelecting(DataRow cell)
        {
            if (!IsSelecting || StartSelectedRow == null || cell == null) return;

            EndSelectedRow = cell;
            var topRow = StartSelectedRow.Index < EndSelectedRow.Index ? StartSelectedRow : EndSelectedRow;
            var bottomRow = StartSelectedRow.Index < EndSelectedRow.Index ? EndSelectedRow : StartSelectedRow;
            _selectRangeMarker.Fit(topRow.Cells.First(), bottomRow.Cells.Last());
            _selectRangeMarker.IsVisible = true;
        }

        public void EndRowSelecting()
        {
            if (!IsSelecting) return;
            IsSelecting = false;
        }

        public void SelectUp()
        {
            EndSelecting();
            if (StartSelectedCell == null) return;
            var row = Mathf.Max(0, StartSelectedCell.Row - 1);
            SelectCell(_table.DataRows[row][StartSelectedCell.Col]);
        }

        public void SelectDown()
        {
            EndSelecting();
            if (StartSelectedCell == null) return;
            var row = Mathf.Min(_table.DataRows.Count - 1, StartSelectedCell.Row + 1);
            SelectCell(_table.DataRows[row][StartSelectedCell.Col]);
        }

        public void SelectLeft()
        {
            EndSelecting();
            if (StartSelectedCell == null) return;
            var row = _table.DataRows[StartSelectedCell.Row];
            var col = Mathf.Max(0, StartSelectedCell.Col - 1);
            SelectCell(row[col]);
        }

        public void SelectRight()
        {
            EndSelecting();
            if (StartSelectedCell == null) return;
            var row = _table.DataRows[StartSelectedCell.Row];
            var col = Mathf.Min(row.Cells.Count - 1, StartSelectedCell.Col + 1);
            SelectCell(_table.DataRows[StartSelectedCell.Row][col]);
        }

        private void SelectCell(Cell cell)
        {
            StartSelectedCell = cell;
            _selectMarker.Fit(cell);
            _selectMarker.IsVisible = true;
            _selectRangeMarker.IsVisible = false;
        }

        public void CancelSelecting()
        {
            StartSelectedCell = null;
            EndSelectedCell = null;
            StartSelectedRow = null;
            EndSelectedRow = null;
            _selectMarker.IsVisible = false;
            _selectRangeMarker.IsVisible = false;
        }

        public Cell[][] GetSelectedCells()
        {
            if (StartSelectedCell == null) return null;
            if (EndSelectedCell == null) return new[] { new[] { StartSelectedCell } };

            var top = Mathf.Min(StartSelectedCell.Row, EndSelectedCell.Row);
            var bottom = Mathf.Max(StartSelectedCell.Row, EndSelectedCell.Row);
            var left = Mathf.Min(StartSelectedCell.Col, EndSelectedCell.Col);
            var right = Mathf.Max(StartSelectedCell.Col, EndSelectedCell.Col);

            var selectedCells = new Cell[bottom - top + 1][];
            var isSelectedEmptyRowCell = bottom == _table.DataRows.Count;
            if (isSelectedEmptyRowCell) bottom--;

            for (var i = top; i <= bottom; i++)
            {
                selectedCells[i - top] = new Cell[right - left + 1];
                for (var j = left; j <= right; j++) selectedCells[i - top][j - left] = _table.DataRows[i][j];
            }

            if (isSelectedEmptyRowCell)
            {
                selectedCells[bottom - top + 1] = new Cell[right - left + 1];
                for (var j = left; j <= right; j++) selectedCells[bottom - top + 1][j - left] = _table.EmptyRow.Cells[j];
            }

            return selectedCells;
        }

        public bool IsSelected(Cell cell)
        {
            if (StartSelectedCell == null) return false;
            if (EndSelectedCell == null) return cell == StartSelectedCell;

            var top = Mathf.Min(StartSelectedCell.Row, EndSelectedCell.Row);
            var bottom = Mathf.Max(StartSelectedCell.Row, EndSelectedCell.Row);
            var left = Mathf.Min(StartSelectedCell.Col, EndSelectedCell.Col);
            var right = Mathf.Max(StartSelectedCell.Col, EndSelectedCell.Col);

            return cell.Row >= top && cell.Row <= bottom &&
                   cell.Col >= left && cell.Col <= right;
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
