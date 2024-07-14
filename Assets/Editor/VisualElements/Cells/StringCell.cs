using System;
using Editor.Data;
using Editor.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.VisualElements.Cells
{
    public class StringCell : Cell<string>
    {
        private VisualElement _body;
        private bool _isEditing;

        public StringCell(int row, int col, string value, ColumnMetadata metadata, SerializedProperty dataProperty) : base(row, col, value, metadata, dataProperty) { }

        public override void StartEditing()
        {
            StartEditing(Value);
        }

        public override void StartEditingByKeyDown(KeyDownEvent evt)
        {
            if (_isEditing) return;

            var str = evt.keyCode.KeyCodeToString(Event.current.shift);
            if (!string.IsNullOrEmpty(str)) this.ExecAfter1Frame(() => StartEditing(str));
        }

        protected override void RefreshView()
        {
            Clear();
            _body = new Label { text = Convert.ToString(Value) };
            Add(_body);
        }

        private void StartEditing(string value)
        {
            _isEditing = true;

            _body.RemoveFromHierarchy();

            var textField = new TextField { value = value, };
            if (DataProperty != null) textField.BindProperty(DataProperty.FindPropertyRelative(Metadata.Name));
            AddToClassList("input-cell");

            textField.RegisterCallback<FocusInEvent>(_ =>
            {
                this.ExecAfter1Frame(() => textField.SelectRange(textField.text.Length, textField.text.Length));
            });

            textField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = textField.value;
                OnValueChanged(prev, Value);
                textField.RemoveFromHierarchy();
                RemoveFromClassList("input-cell");
                Add(_body);
                _isEditing = false;
            });

            Add(textField);
            textField.Focus();
        }
    }
}
