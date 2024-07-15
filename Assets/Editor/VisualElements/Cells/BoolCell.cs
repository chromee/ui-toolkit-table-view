using Editor.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.VisualElements.Cells
{
    public class BoolCell : Cell<bool>
    {
        public BoolCell(int row, int col, bool value, ColumnMetadata metadata, SerializedProperty dataProperty) : base(row, col, value, metadata, dataProperty)
        {
            if (dataProperty != null) dataProperty.FindPropertyRelative(metadata.Name).boolValue = value;
            AddToClassList("input-cell");
            
            var toggle = new Toggle { text = string.Empty, value = Value };
            toggle.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = evt.newValue;
                OnValueChanged(prev, Value);
            });
            if (DataProperty != null) toggle.BindProperty(DataProperty.FindPropertyRelative(metadata.Name));
            Add(toggle);
        }

        public override void StartEditing() { }
        public override void StartEditingByKeyDown(KeyDownEvent evt) { }
        protected override void RefreshView() { }
    }
}
