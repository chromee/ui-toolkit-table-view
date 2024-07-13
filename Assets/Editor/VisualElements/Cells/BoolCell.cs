using System;
using UnityEngine.UIElements;

namespace Editor.VisualElements.Cells
{
    public class BoolCell : Cell<bool>
    {
        public BoolCell(int row, int col, bool value, float width = 100) : base(row, col, value, width)
        {
            var toggle = new Toggle { text = string.Empty, value = Value };
            toggle.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = evt.newValue;
                OnValueChanged(prev, Value);
            });
            Add(toggle);
        }

        public override void StartEditing() { }
        public override void StartEditingByKeyDown(KeyDownEvent evt) { }
        protected override void RefreshView() { }
    }

    public class EnumCell : Cell<Enum>
    {
        public EnumCell(int row, int col, Enum value, float width = 100) : base(row, col, value, width)
        {
            AddToClassList("input-cell");

            var enumField = new EnumField();
            enumField.Init(Value);
            enumField.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = evt.newValue;
                OnValueChanged(prev, Value);
            });
            Add(enumField);
        }

        public override void StartEditing() { }
        public override void StartEditingByKeyDown(KeyDownEvent evt) { }
        protected override void RefreshView() { }
    }
}
