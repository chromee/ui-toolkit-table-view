using System;
using System.Collections.Generic;
using Editor.Data;
using UnityEngine.UIElements;

namespace Editor.VisualElements
{
    public class DataRow : VisualElement
    {
        public readonly int Index;

        public IndexCell IndexCell { get; private set; }
        private readonly Cell[] _cells;
        public IReadOnlyList<Cell> Cells => _cells;
        public new Cell this[int index] => _cells[index];

        public DataRow(int index, ColumnMetadata[] colInfos, object[] values = null)
        {
            AddToClassList("row");

            Index = index;
            IndexCell = new IndexCell(Index);
            Add(IndexCell);

            _cells = new Cell[colInfos.Length];
            for (var i = 0; i < colInfos.Length; i++)
            {
                var colInfo = colInfos[i];
                var cell = Cell.Create(Index, i, values?[i], colInfo.Width);
                _cells[i] = cell;
                Add(cell);
            }
        }
    }
}
