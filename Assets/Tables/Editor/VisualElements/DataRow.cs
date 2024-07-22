using System;
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
                var columnMetadata = metadata[i];
                var defaultValue = values?[i] ?? columnMetadata.GetDefaultValue();
                var cell = Cell.Create(Index, i, defaultValue, columnMetadata, rowProperty);
                Cells[i] = cell;
                Add(cell);
            }
        }

        public void SetData(object data) => Data = data;
    }
}
