using Tables.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements.Cells
{
    public class BoolCell : Cell<bool>
    {
        public BoolCell(int row, int col, bool value, ColumnMetadata metadata, SerializedProperty dataProperty) : base(row, col, value, metadata, dataProperty)
        {
            var toggle = new Toggle { text = string.Empty, value = Value };

            toggle.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = evt.newValue;
                ValueChangeFromEdit(prev, Value);
            });

            if (DataProperty != null)
            {
                var property = DataProperty.FindPropertyRelative(metadata.Name);
                property.boolValue = value;
                toggle.BindProperty(DataProperty.FindPropertyRelative(metadata.Name));
            }

            AddToClassList("input-cell");

            Add(toggle);
        }

        public override void StartEditing() { }
        public override void StartEditingByKeyDown(KeyDownEvent evt) { }
        protected override void RefreshView() { }
    }
}
