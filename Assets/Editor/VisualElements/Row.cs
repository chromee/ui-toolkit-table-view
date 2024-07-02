using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Editor.VisualElements
{
    public class Row : VisualElement
    {
        public int Index;
        public readonly List<Cell> Cells = new();

        public Row(int index)
        {
            Index = index;
            AddToClassList("row");
        }

        public void AddCell(Cell cell)
        {
            Cells.Add(cell);
            Add(cell);
        }

        public new Cell this[int index] => Cells[index];
    }
}
