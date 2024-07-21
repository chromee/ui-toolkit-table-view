using System.Collections.Generic;
using Tables.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class DataRow : VisualElement
    {
        public readonly int Index;
        public object Data { get; private set; }
        
        public IndexCell IndexCell { get; }
        private readonly Cell[] _cells;
        public IReadOnlyList<Cell> Cells => _cells;
        public new Cell this[int index] => _cells[index];

        public DataRow(int index, ColumnMetadata[] metadata, object data, object[] values, SerializedProperty dataProperty)
        {
            AddToClassList("row");

            Index = index;
            Data = data;
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
        
        public void SetData(object data) => Data = data;
    }
}
