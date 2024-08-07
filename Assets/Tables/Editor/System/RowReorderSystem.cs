using Tables.Editor.VisualElements;
using UnityEngine.UIElements;

namespace Tables.Editor.System
{
    public class RowReorderSystem
    {
        private readonly Table _table;
        private readonly SelectSystem _selectSystem;

        public bool IsReordering { get; private set; }

        private ReorderShadow _reorderShadow;
        private ReorderLine _reorderLine;

        public RowReorderSystem(VisualElement rootVisualElement, Table table, SelectSystem selectSystem)
        {
            _table = table;
            _selectSystem = selectSystem;

            _reorderShadow = new ReorderShadow(rootVisualElement);
            _reorderLine = new ReorderLine(rootVisualElement);
        }

        public void StartReordering()
        {
            IsReordering = true;
        }

        public void Reordering(DataRow dataRow)
        {
            if (!IsReordering) return;
        }

        public void EndReordering(DataRow dataRow)
        {
            if (!IsReordering) return;
            if (_selectSystem.IsSelected(dataRow)) return;

            _table.MoveDataRow(_selectSystem.SelectedRows, dataRow.Index);

            IsReordering = false;
        }
    }
}
