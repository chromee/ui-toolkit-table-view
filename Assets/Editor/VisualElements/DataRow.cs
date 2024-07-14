using System;
using System.Collections.Generic;
using Editor.Data;
using UnityEditor;
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

        public DataRow(int index, ColumnMetadata[] metadata, object[] values, SerializedProperty dataProperty)
        {
            AddToClassList("row");

            Index = index;
            IndexCell = new IndexCell(Index);
            Add(IndexCell);

            _cells = new Cell[metadata.Length];
            for (var i = 0; i < metadata.Length; i++)
            {
                var md = metadata[i];
                var cell = Cell.Create(Index, i, values?[i], md, dataProperty);
                _cells[i] = cell;
                Add(cell);
            }
        }
    }
}
