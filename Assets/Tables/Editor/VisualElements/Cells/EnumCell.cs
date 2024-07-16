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
            var enumField = new EnumField();
            enumField.Init(Value);

            enumField.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = evt.newValue;
                ValueChangeFromEdit(prev, Value);
            });

            if (DataProperty != null)
            {
                var property = DataProperty.FindPropertyRelative(metadata.Name);
                property.enumValueIndex = Convert.ToInt32(value);
                enumField.BindProperty(DataProperty.FindPropertyRelative(metadata.Name));
            }

            AddToClassList("input-cell");

            Add(enumField);
        }

        public override void StartEditing() { }
        public override void StartEditingByKeyDown(KeyDownEvent evt) { }
        protected override void RefreshView() { }
    }
}
