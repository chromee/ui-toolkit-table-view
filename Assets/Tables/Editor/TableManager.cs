using System.Linq;
using Tables.Editor.System;
using Tables.Editor.Utilities;
using Tables.Editor.VisualElements;
using Tables.Runtime;
using UnityEngine.UIElements;

namespace Tables.Editor
{
    public class TableManager
    {
        public readonly Database Database;
        public readonly Table Table;
        public readonly CopyPasteSystem CopyPasteSystem;
        public readonly DeleteSystem DeleteSystem;
        public readonly ResizeColSystem ResizeColSystem;
        public readonly RowReorderSystem RowReorderSystem;
        public readonly SelectSystem SelectSystem;
        public readonly ShortcutKeySystem ShortcutKeySystem;
        public readonly UndoRedoSystem UndoRedoSystem;
        public readonly ValidateSystem ValidateSystem;

        private readonly VisualElement _rootVisualElement;

        public TableManager(Database database, VisualElement rootVisualElement)
        {
            Database = database;
            Table = new Table(database);
            _rootVisualElement = rootVisualElement;

            var body = rootVisualElement.Q<ScrollView>("table-body");
            body.Add(Table);

            UndoRedoSystem = new UndoRedoSystem();
            SelectSystem = new SelectSystem(_rootVisualElement, Table);
            RowReorderSystem = new RowReorderSystem(_rootVisualElement, Table, SelectSystem);
            CopyPasteSystem = new CopyPasteSystem(_rootVisualElement, Table, SelectSystem, UndoRedoSystem);
            ResizeColSystem = new ResizeColSystem(database, _rootVisualElement, body, Table, SelectSystem, CopyPasteSystem);
            DeleteSystem = new DeleteSystem(Table, SelectSystem, UndoRedoSystem);
            ValidateSystem = new ValidateSystem(database, Table);
            ShortcutKeySystem = new ShortcutKeySystem(_rootVisualElement, CopyPasteSystem, UndoRedoSystem, SelectSystem, DeleteSystem);

            foreach (var headerCell in Table.HeaderRow.Cells)
            {
                headerCell.Resizer.RegisterCallback<MouseDownEvent>(evt => ResizeColSystem.StartResizing(evt, headerCell.ColumnIndex));
            }

            foreach (var dataRow in Table.DataRows)
            {
                RegisterIndexCellCallback(dataRow);
                foreach (var cell in dataRow.Cells)
                {
                    RegisterCellCallback(cell);
                }
            }

            Table.FooterRow.AddRowButton.clicked += () =>
            {
                var dataRow = Table.AddDataRow();
                foreach (var cell in dataRow.Cells) RegisterCellCallback(cell);
            };

            rootVisualElement.RegisterCallback<MouseUpEvent>(_ =>
            {
                SelectSystem.EndSelecting();
                SelectSystem.EndRowSelecting();
            });

            ValidateSystem.StartValidate();
        }

        private void RegisterIndexCellCallback(DataRow dataRow)
        {
            dataRow.IndexCell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount != 1) return;
                if (SelectSystem.IsSelected(dataRow)) RowReorderSystem.StartReordering();
                else SelectSystem.StartRowSelecting(dataRow);
            });

            dataRow.IndexCell.RegisterCallback<MouseEnterEvent>(_ =>
            {
                if (RowReorderSystem.IsReordering) RowReorderSystem.Reordering(dataRow);
                else SelectSystem.RowSelecting(dataRow);
            });

            dataRow.IndexCell.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (RowReorderSystem.IsReordering) RowReorderSystem.EndReordering(dataRow);
            });
        }

        private void RegisterCellCallback(Cell cell)
        {
            cell.OnValueChangedFromEdit += (from, to) =>
            {
                UndoRedoSystem.AddUndoCommand(cell, from, to);
                _rootVisualElement.ExecAfterFrame(() => _rootVisualElement.Focus());
            };

            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectSystem.StartSelecting(cell);
                if (evt.clickCount >= 2) cell.StartEditing();
            });

            cell.RegisterCallback<MouseEnterEvent>(_ => SelectSystem.Selecting(cell));

            cell.OnStartEditing += () => ShortcutKeySystem.IsEnabled = false;
            cell.OnEndEditing += () => ShortcutKeySystem.IsEnabled = true;
        }
    }
}
