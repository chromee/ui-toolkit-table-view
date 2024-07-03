using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Editor.VisualElements
{
    public class EmptyRow : VisualElement
    {
        public Button AddRowButton { get; }
        private readonly Cell[] _cells;
        public IReadOnlyList<Cell> Cells => _cells;

        public EmptyRow(int rowIndex, ColInfo[] colInfos)
        {
            AddToClassList("row");

            AddRowButton = new Button { text = "+" };
            AddRowButton.AddToClassList("add-row-button");
            Add(AddRowButton);

            _cells = new Cell[colInfos.Length];
            for (var i = 0; i < colInfos.Length; i++)
            {
                var colInfo = colInfos[i];

                var cell = Type.GetTypeCode(colInfo.Type) switch
                {
                    TypeCode.String => Cell.Create(rowIndex, i, string.Empty, colInfo.Width),
                    TypeCode.Int32 => Cell.Create(rowIndex, i, 0, colInfo.Width),
                    TypeCode.Single => Cell.Create(rowIndex, i, 0f, colInfo.Width),
                    TypeCode.Boolean => Cell.Create(rowIndex, i, false, colInfo.Width),
                    _ => Cell.Create(0, i, string.Empty, colInfo.Width),
                };

                _cells[i] = cell;
                Add(cell);
            }
        }
    }
}
