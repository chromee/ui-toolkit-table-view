using System.Globalization;
using Editor.Utilities;
using UnityEngine.UIElements;

namespace Editor.VisualElements.Cells
{
    public class FloatCell : Cell<float>
    {
        private VisualElement _body;
        private bool _isEditing;

        public FloatCell(int row, int col, float value, float width = 100) : base(row, col, value, width) { }

        public override void StartEditing() => StartEditing(Value);

        public override void StartEditingByKeyDown(KeyDownEvent evt)
        {
            if (_isEditing) return;

            var num = evt.keyCode.GetNumericValue();
            if (num < 0) return;
            this.ExecAfter1Frame(() => StartEditing(num));
        }

        protected override void RefreshView()
        {
            Clear();
            _body = new Label { text = Value.ToString(CultureInfo.InvariantCulture) };
            Add(_body);
        }

        private void StartEditing(float value)
        {
            _isEditing = true;

            var floatField = new FloatField { value = value, };
            floatField.style.width = Width;
            AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Add(floatField);

            floatField.RegisterCallback<FocusInEvent>(_ => this.ExecAfter1Frame(() => floatField.SelectRange(floatField.text.Length, floatField.text.Length)));
            floatField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = floatField.value;
                OnValueChanged(prev, Value);
                floatField.RemoveFromHierarchy();
                RemoveFromClassList("input-cell");
                Add(_body);
                _isEditing = false;
            });

            floatField.Focus();
        }
    }
}
