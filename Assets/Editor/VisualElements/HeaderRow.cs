using System.Collections.Generic;
using Editor.Data;
using UnityEngine.UIElements;

namespace Editor.VisualElements
{
    public class HeaderRow : VisualElement
    {
        public readonly OriginCell OriginCell;
        private readonly HeaderCell[] _cells;
        public IReadOnlyList<HeaderCell> Cells => _cells;

        public HeaderRow(ColumnMetadata[] colInfos)
        {
            AddToClassList("row");

            OriginCell = new OriginCell();
            Add(OriginCell);

            _cells = new HeaderCell[colInfos.Length];
            for (var i = 0; i < colInfos.Length; i++)
            {
                var colInfo = colInfos[i];
                var headerCell = new HeaderCell(colInfo.Name, colInfo.Width, i);

                _cells[i] = headerCell;
                Add(headerCell);
            }
        }
    }
}
