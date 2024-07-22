using System;
using Tables.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements.Cells
{
    public class EnumCell : Cell<Enum>
    {
        public EnumCell(int row, int col, Enum value, ColumnMetadata metadata, SerializedProperty rowProperty) : base(row, col, value, metadata, rowProperty)
        {
            var enumField = new EnumField();
            enumField.Init(Value);

            enumField.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = evt.newValue;
                ValueChangeFromEdit(prev, Value);
            });

            if (CellProperty != null)
            {
                CellProperty.enumValueIndex = Convert.ToInt32(value);
                enumField.BindProperty(CellProperty);
            }

            AddToClassList("input-cell");

            Add(enumField);
        }

        public override void StartEditing() { }
        public override void StartEditingByKeyDown(KeyDownEvent evt) { }
        protected override void RefreshView() { }
    }
}
