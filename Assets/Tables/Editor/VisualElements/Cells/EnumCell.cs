using System;
using Tables.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements.Cells
{
    public class EnumCell : Cell<Enum>
    {
        public EnumCell(int row, int col, Enum value, ColumnMetadata metadata, SerializedObject serializedObject, SerializedProperty dataProperty) : base(row, col, value, metadata, serializedObject, dataProperty)
        {
            if (dataProperty != null) dataProperty.FindPropertyRelative(metadata.Name).enumValueIndex = Convert.ToInt32(value);
            AddToClassList("input-cell");

            var enumField = new EnumField();
            enumField.Init(Value);
            enumField.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = evt.newValue;
                OnValueChanged(prev, Value);
            });
            if (DataProperty != null) enumField.BindProperty(DataProperty.FindPropertyRelative(metadata.Name));
            Add(enumField);
        }

        public override void StartEditing() { }
        public override void StartEditingByKeyDown(KeyDownEvent evt) { }
        protected override void RefreshView() { }
    }
}
