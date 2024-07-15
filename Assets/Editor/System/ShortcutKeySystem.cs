using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.System
{
    public class ShortcutKeySystem
    {
        private readonly VisualElement _rootVisualElement;
        private readonly CopyPasteSystem _copyPasteSystem;
        private readonly UndoRedoSystem _undoRedoSystem;
        private readonly SelectSystem _selectSystem;
        private readonly DeleteSystem _deleteSystem;

        public ShortcutKeySystem(VisualElement rootVisualElement, CopyPasteSystem copyPasteSystem, UndoRedoSystem undoRedoSystem, SelectSystem selectSystem, DeleteSystem deleteSystem)
        {
            this._rootVisualElement = rootVisualElement;
            _copyPasteSystem = copyPasteSystem;
            _undoRedoSystem = undoRedoSystem;
            _selectSystem = selectSystem;
            _deleteSystem = deleteSystem;
        }

        public void SetupRootVisualElementForKeyboardInput()
        {
            _rootVisualElement.focusable = true;
            _rootVisualElement.pickingMode = PickingMode.Position;
            _rootVisualElement.Focus();
        }

        public void RegisterShortcuts()
        {
            _rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        public void UnregisterShortcuts()
        {
            _rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent ev)
        {
            if (ev.keyCode == KeyCode.C && Event.current.control) _copyPasteSystem.CopyCell();
            else if (ev.keyCode == KeyCode.V && Event.current.control) _copyPasteSystem.PasteCell();
            else if (ev.keyCode == KeyCode.Z && Event.current.control && Event.current.shift) _undoRedoSystem.Redo(_rootVisualElement);
            else if (ev.keyCode == KeyCode.Z && Event.current.control) _undoRedoSystem.Undo(_rootVisualElement);
            else if (ev.keyCode == KeyCode.Y && Event.current.control) _undoRedoSystem.Redo(_rootVisualElement);
            else if (ev.keyCode == KeyCode.Escape) CancelAll();
            else if (ev.keyCode == KeyCode.Delete) _deleteSystem.DeleteSelected();
            else if (ev.keyCode == KeyCode.UpArrow) _selectSystem.SelectUp();
            else if (ev.keyCode == KeyCode.DownArrow) _selectSystem.SelectDown();
            else if (ev.keyCode == KeyCode.LeftArrow) _selectSystem.SelectLeft();
            else if (ev.keyCode == KeyCode.RightArrow) _selectSystem.SelectRight();
            else if (ev.keyCode == KeyCode.F2) _selectSystem.StartSelectedCell?.StartEditing();
            else _selectSystem.StartSelectedCell?.StartEditingByKeyDown(ev);
        }

        private void CancelAll()
        {
            _copyPasteSystem.CancelCopy();
            _selectSystem.CancelSelecting();
        }
    }
}
