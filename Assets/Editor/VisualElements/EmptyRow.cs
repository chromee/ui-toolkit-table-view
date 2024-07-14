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

        public EmptyRow(int rowIndex, ColumnMetadata[] colInfos)
        {
            AddToClassList("row");

            AddRowButton = new Button { text = "+" };
            AddRowButton.AddToClassList("add-row-button");
            Add(AddRowButton);

            _cells = new Cell[colInfos.Length];
            for (var i = 0; i < colInfos.Length; i++)
            {
                var colInfo = colInfos[i];
                var value =
                    colInfo.Type.IsValueType ? Activator.CreateInstance(colInfo.Type) :
                    Type.GetTypeCode(colInfo.Type) == TypeCode.String ? string.Empty :
                    null;
                var cell = Cell.Create(rowIndex, i, value, colInfo.Width);
                _cells[i] = cell;
                Add(cell);
            }
        }
    }
}
