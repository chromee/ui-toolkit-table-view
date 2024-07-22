using System;
using Tables.Editor.Utilities;
using Tables.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements.Cells
{
    public class StringCell : Cell<string>
    {
        private VisualElement _body;
        private bool _isEditing;

        public StringCell(int row, int col, string value, ColumnMetadata metadata, SerializedProperty rowProperty) : base(row, col, value, metadata, rowProperty)
        {
            if (rowProperty != null) rowProperty.FindPropertyRelative(metadata.Name).stringValue = value;
        }

        public override void StartEditing()
        {
            StartEditing(Value);
        }

        public override void StartEditingByKeyDown(KeyDownEvent evt)
        {
            if (_isEditing) return;

            var str = evt.keyCode.KeyCodeToString(Event.current.shift);
            if (!string.IsNullOrEmpty(str)) this.ExecAfterFrame(() => StartEditing(str));
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

            var textField = new TextField { value = value, };

            if (CellProperty != null)
            {
                CellProperty.stringValue = value;   
                CellProperty.serializedObject.ApplyModifiedProperties();
                textField.BindProperty(CellProperty);
            }

            textField.RegisterCallback<FocusInEvent>(_ =>
            {
                this.ExecAfterFrame(() => textField.SelectRange(textField.text.Length, textField.text.Length));
            });

            textField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = textField.value;
                ValueChangeFromEdit(prev, Value);
                textField.RemoveFromHierarchy();
                RemoveFromClassList("input-cell");
                Add(_body);
                _isEditing = false;
            });

            AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Add(textField);
            textField.Focus();
        }
    }
}
