using System;
using System.Globalization;
using Editor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.VisualElements
{
    public abstract class Cell : VisualElement
    {
        public int Row;
        public int Col;
        public Vector2 Position => new(Col, Row);

        public abstract object Val { get; }
        public event Action<object, object> OnValueChangedFromEdit;

        public Cell<T> As<T>() => this as Cell<T>;

        public float Width
        {
            get => resolvedStyle.width;
            set => style.width = value;
        }

        public static Cell Create<T>(int row, int col, T value, float width = 100f)
        {
            return value switch
            {
                string sv => new StringCell(row, col, sv, width),
                int iv => new IntCell(row, col, iv, width),
                float fv => new FloatCell(row, col, fv, width),
                bool bv => new BoolCell(row, col, bv, width),
                Enum ev => new EnumCell(row, col, ev, width),
                _ => new StringCell(row, col, value.ToString(), width),
            };
        }

        protected Cell(int row, int col, float width = 100f)
        {
            AddToClassList("cell");
            Row = row;
            Col = col;
            Width = width;
        }

        public abstract void StartEditing();
        public abstract void StartEditingByKeyDown(KeyDownEvent evt);
        public abstract bool TryPaste(Cell from);
        public abstract void ClearValue();

        protected void OnValueChanged(object prev, object current) => OnValueChangedFromEdit?.Invoke(prev, current);
    }

    public abstract class Cell<T> : Cell
    {
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                RefreshView();
            }
        }

        public override object Val => Value;

        protected Cell(int row, int col, T value, float width = 100f) : base(row, col, width)
        {
            Value = value;
        }

        protected abstract void RefreshView();

        public override void ClearValue() => Value = default;

        public override bool TryPaste(Cell from)
        {
            if (from.GetType() != GetType()) return false;
            var v = from.As<T>().Value;
            Value = v;
            return true;
        }
    }

    public class StringCell : Cell<string>
    {
        private VisualElement _body;
        private bool _isEditing;

        public StringCell(int row, int col, string value, float width = 100) : base(row, col, value, width) { }

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
            textField.style.width = Width;
            AddToClassList("input-cell");

            textField.RegisterCallback<FocusInEvent>(_ => this.ExecAfter1Frame(() => textField.SelectRange(textField.text.Length, textField.text.Length)));
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

    public class IntCell : Cell<int>
    {
        private VisualElement _body;
        private bool _isEditing;

        public IntCell(int row, int col, int value, float width = 100) : base(row, col, value, width) { }

        public override void StartEditing() => StartEditing(Value);

        public override void StartEditingByKeyDown(KeyDownEvent evt)
        {
            if (_isEditing) return;

            var num = evt.keyCode.GetNumericValue();
            this.ExecAfter1Frame(() => StartEditing(num));
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

            var integerField = new IntegerField { value = value, };
            integerField.style.width = Width;
            AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Add(integerField);

            integerField.RegisterCallback<FocusInEvent>(_ => this.ExecAfter1Frame(() => integerField.SelectRange(integerField.text.Length, integerField.text.Length)));
            integerField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = integerField.value;
                OnValueChanged(prev, Value);
                integerField.RemoveFromHierarchy();
                RemoveFromClassList("input-cell");
                Add(_body);
                _isEditing = false;
            });
            integerField.Focus();
        }
    }

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
