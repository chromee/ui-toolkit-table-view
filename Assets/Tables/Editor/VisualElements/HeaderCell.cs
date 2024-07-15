using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class HeaderCell : VisualElement
    {
        public int ColumnIndex { get; }
        public VisualElement Resizer { get; }

        public HeaderCell(string colName, float width, int columnIndex)
        {
            ColumnIndex = columnIndex;
            AddToClassList("header-cell");
            style.width = width;
            Add(new Label(colName));

            Resizer = new VisualElement();
            Resizer.AddToClassList("header-cell-resize-handle");
            Add(Resizer);
        }
    }
}
