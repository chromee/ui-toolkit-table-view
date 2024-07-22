using System.Globalization;
using Tables.Editor.Utilities;
using Tables.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements.Cells
{
    public class FloatCell : Cell<float>
    {
        private VisualElement _body;
        private bool _isEditing;

        public FloatCell(int row, int col, float value, ColumnMetadata metadata, SerializedProperty rowProperty) : base(row, col, value, metadata, rowProperty)
        {
            if (rowProperty != null) rowProperty.FindPropertyRelative(metadata.Name).floatValue = value;
        }

        public override void StartEditing() => StartEditing(Value);

        public override void StartEditingByKeyDown(KeyDownEvent evt)
        {
            if (_isEditing) return;

            var num = evt.keyCode.GetNumericValue();
            if (num < 0) return;
            this.ExecAfterFrame(() => StartEditing(num));
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
            StartEdit();

            var floatField = new FloatField { value = value, };

            if (CellProperty != null)
            {
                CellProperty.floatValue = value;
                CellProperty.serializedObject.ApplyModifiedProperties();
                floatField.BindProperty(CellProperty);
            }

            floatField.RegisterCallback<FocusOutEvent>(_ => EndEditing(floatField));

            AddToClassList("input-cell");
            _body.RemoveFromHierarchy();
            Add(floatField);
            floatField.Focus();
        }

        private void EndEditing(FloatField floatField)
        {
            var prev = Value;
            Value = floatField.value;
            ValueChangeFromEdit(prev, Value);
            floatField.RemoveFromHierarchy();
            RemoveFromClassList("input-cell");
            Add(_body);
            _isEditing = false;
            EndEdit();
        }
    }
}
