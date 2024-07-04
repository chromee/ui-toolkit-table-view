using System.Linq;
using Editor.System;
using Editor.Utilities;
using Editor.VisualElements;
using UnityEngine.UIElements;

namespace Editor
{
    public class TableManager
    {
        private readonly VisualElement _rootVisualElement;

        public readonly Table Table;
        public readonly UndoRedoSystem UndoRedoSystem;
        public readonly CopyPasteSystem CopyPasteSystem;
        public readonly ResizeColSystem ResizeColSystem;
        public readonly SelectSystem SelectSystem;
        public readonly ShortcutKeySystem ShortcutKeySystem;

        public TableManager(VisualElement rootVisualElement, ColInfo[] colInfos, object[][] rowValues = null)
        {
            _rootVisualElement = rootVisualElement;
            Table = new Table(colInfos, rowValues);
            _rootVisualElement.Add(Table);

            UndoRedoSystem = new UndoRedoSystem();
            SelectSystem = new SelectSystem(_rootVisualElement, Table);
            CopyPasteSystem = new CopyPasteSystem(_rootVisualElement, SelectSystem, UndoRedoSystem);
            ResizeColSystem = new ResizeColSystem(_rootVisualElement, Table, colInfos, SelectSystem, CopyPasteSystem);
            ShortcutKeySystem = new ShortcutKeySystem(_rootVisualElement, CopyPasteSystem, UndoRedoSystem, SelectSystem);

            foreach (var headerCell in Table.HeaderRow.Cells)
            {
                headerCell.Resizer.RegisterCallback<MouseDownEvent>(evt => ResizeColSystem.StartResizing(evt, headerCell.ColumnIndex));
            }

            foreach (var dataRow in Table.DataRows)
            foreach (var cell in dataRow.Cells)
            {
                RegisterCellCallback(cell);
            }

            Table.EmptyRow.AddRowButton.clicked += () =>
            {
                var dataRow = Table.AddDataRow(colInfos, Table.EmptyRow.Cells.Select(cell => cell.Val).ToArray());
                foreach (var cell in dataRow.Cells) RegisterCellCallback(cell);
            };
            foreach (var cell in Table.EmptyRow.Cells)
            {
                RegisterCellCallback(cell);
            }

            rootVisualElement.RegisterCallback<MouseUpEvent>(SelectSystem.EndSelecting);
        }

        private void RegisterCellCallback(Cell cell)
        {
            cell.OnValueChangedFromEdit += (from, to) =>
            {
                UndoRedoSystem.AddUndoCommand(cell, from, to);
                _rootVisualElement.ExecAfter1Frame(() => _rootVisualElement.Focus());
            };

            cell.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 1) SelectSystem.StartSelecting(cell);
                if (evt.clickCount >= 2) cell.StartEditing();
            });

            cell.RegisterCallback<MouseEnterEvent>(_ => SelectSystem.Selecting(cell));
        }
    }
}
