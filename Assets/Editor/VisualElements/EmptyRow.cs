using System;
using System.Collections.Generic;
using Editor.Data;
using UnityEngine.UIElements;

namespace Editor.VisualElements
{
    public class EmptyRow : VisualElement
    {
        public Button AddRowButton { get; }
        private readonly Cell[] _cells;
        public IReadOnlyList<Cell> Cells => _cells;

        public EmptyRow(int rowIndex, ColumnMetadata[] metadata)
        {
            AddToClassList("row");

            AddRowButton = new Button { text = "+" };
            AddRowButton.AddToClassList("add-row-button");
            Add(AddRowButton);

            _cells = new Cell[metadata.Length];
            for (var i = 0; i < metadata.Length; i++)
            {
                var md = metadata[i];
                var value =
                    md.Type.IsValueType ? Activator.CreateInstance(md.Type) :
                    Type.GetTypeCode(md.Type) == TypeCode.String ? string.Empty :
                    null;
                var cell = Cell.Create(rowIndex, i, value, md, null, null);
                _cells[i] = cell;
                Add(cell);
            }
        }
    }
}
