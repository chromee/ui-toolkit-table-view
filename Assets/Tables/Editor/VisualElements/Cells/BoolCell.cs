using Tables.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements.Cells
{
    public class BoolCell : Cell<bool>
    {
        public BoolCell(int row, int col, bool value, ColumnMetadata metadata, SerializedProperty rowProperty) : base(row, col, value, metadata, rowProperty)
        {
            var toggle = new Toggle { text = string.Empty, value = Value };

            toggle.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = evt.newValue;
                ValueChangeFromEdit(prev, Value);
            });

            if (CellProperty != null)
            {
                CellProperty.boolValue = value;
                toggle.BindProperty(CellProperty);
            }

            AddToClassList("input-cell");

            Add(toggle);
        }

        public override void StartEditing() { }
        public override void StartEditingByKeyDown(KeyDownEvent evt) { }
        protected override void RefreshView() { }
    }
}
