using Tables.Editor.Utilities;
using Tables.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements.Cells
{
    public class IntCell : Cell<int>
    {
        private VisualElement _body;
        private bool _isEditing;

        public IntCell(int row, int col, int value, ColumnMetadata metadata, SerializedProperty rowProperty) : base(row, col, value, metadata, rowProperty)
        {
            if (rowProperty != null) rowProperty.FindPropertyRelative(metadata.Name).intValue = value;
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
            _body = new Label { text = Value.ToString() };
            Add(_body);
        }

        private void StartEditing(int value)
        {
            _isEditing = true;
            StartEdit();

            var integerField = new IntegerField { value = value, };

            if (CellProperty != null)
            {
                CellProperty.intValue = value;
                CellProperty.serializedObject.ApplyModifiedProperties();
                integerField.BindProperty(CellProperty);
            }

            integerField.RegisterCallback<FocusOutEvent>(_ => EndEditing(integerField));

            AddToClassList("input-cell");
            _body.RemoveFromHierarchy();
            Add(integerField);
            integerField.Focus();
        }

        private void EndEditing(IntegerField integerField)
        {
            var prev = Value;
            Value = integerField.value;
            ValueChangeFromEdit(prev, Value);
            integerField.RemoveFromHierarchy();
            RemoveFromClassList("input-cell");
            Add(_body);
            _isEditing = false;
            EndEdit();
        }
    }
}
