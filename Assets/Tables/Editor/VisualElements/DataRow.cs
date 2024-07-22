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
        public Cell[] Cells { get; }
        public new Cell this[int index] => Cells[index];

        public DataRow(int index, ColumnMetadata[] metadata, object data, object[] values, SerializedProperty rowProperty)
        {
            AddToClassList("row");

            Index = index;
            Data = data;
            IndexCell = new IndexCell(Index);
            Add(IndexCell);

            Cells = new Cell[metadata.Length];
            for (var i = 0; i < metadata.Length; i++)
            {
                var md = metadata[i];
                var cell = Cell.Create(Index, i, values?[i], md, rowProperty);
                Cells[i] = cell;
                Add(cell);
            }
        }

        public void SetData(object data) => Data = data;
    }
}
