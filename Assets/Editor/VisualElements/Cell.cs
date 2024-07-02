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
        public abstract event Action<object, object> OnValueChangedFromEdit;

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
                string sv => new Cell<string>(row, col, sv, width),
                int iv => new Cell<int>(row, col, iv, width),
                float fv => new Cell<float>(row, col, fv, width),
                bool bv => new Cell<bool>(row, col, bv, width),
                _ => new Cell<string>(row, col, value.ToString(), width),
            };
        }

        protected Cell(int row, int col, float width = 100f)
        {
            AddToClassList("cell");
            Row = row;
            Col = col;
            Width = width;
        }

        public void ChangePosition(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public abstract void StartEditing();
        public abstract void StartEditingByKeyDown(KeyDownEvent evt);
        public abstract bool TryPaste(Cell from);
        public abstract void ClearValue();
    }

    public class Cell<T> : Cell
    {
        private VisualElement _body;

        public override event Action<object, object> OnValueChangedFromEdit;

        private T _value;

        private bool _isEditing;

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

        public Cell(int row, int col, T value, float width = 100f) : base(row, col, width)
        {
            Value = value;
        }

        private void RefreshView()
        {
            Clear();

            if (typeof(T) == typeof(string)) _body = new Label { text = Convert.ToString(Value) };
            else if (typeof(T) == typeof(int)) _body = new Label { text = Convert.ToInt32(Value).ToString() };
            else if (typeof(T) == typeof(float)) _body = new Label { text = Convert.ToSingle(Value).ToString(CultureInfo.InvariantCulture) };
            else if (typeof(T) == typeof(bool)) _body = CreateEditingBodyAsBool();
            else _body = new Label { text = Convert.ToString(Value) };

            Add(_body);
        }

        public override void ClearValue() => Value = default;

        public override bool TryPaste(Cell from)
        {
            if (from.GetType() != GetType()) return false;
            var v = from.As<T>().Value;
            Value = v;
            return true;
        }

        #region create body

        private VisualElement CreateEditingBodyAsBool()
        {
            // Bool は常時 Toggle
            var toggle = new Toggle { text = string.Empty, value = Convert.ToBoolean(Value) };
            toggle.RegisterValueChangedCallback(evt =>
            {
                var prev = Value;
                Value = (T)(object)evt.newValue;
                OnValueChangedFromEdit?.Invoke(prev, Value);
            });

            return toggle;
        }

        #endregion

        #region editing

        public override void StartEditing()
        {
            if (typeof(T) == typeof(string)) StartEditingAsString();
            else if (typeof(T) == typeof(int)) StartEditingAsInt();
            else if (typeof(T) == typeof(float)) StartEditingAsFloat();
            // else if (typeof(T) == typeof(bool)) StartEditingAsBool(); // Bool は常時 Toggle なので切り替え不要
        }

        public override void StartEditingByKeyDown(KeyDownEvent evt)
        {
            if (_isEditing) return;

            if (evt.keyCode == KeyCode.F2) StartEditing();
            if (typeof(T) == typeof(string))
            {
                var str = evt.keyCode.KeyCodeToString(Event.current.shift);
                if (!string.IsNullOrEmpty(str)) StartEditingAsString(str);
            }
            else if (typeof(T) == typeof(int) && evt.keyCode.IsNumericKey()) StartEditingAsInt(evt.keyCode.GetNumericValue());
            else if (typeof(T) == typeof(float) && evt.keyCode.IsNumericKey()) StartEditingAsFloat(evt.keyCode.GetNumericValue());
        }

        private void StartEditingAsString(string value = null)
        {
            _isEditing = true;

            var textField = new TextField { value = !string.IsNullOrEmpty(value) ? value : Convert.ToString(Value), };
            textField.style.width = Width;
            AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Add(textField);

            textField.RegisterCallback<FocusInEvent>(_ => this.ExecAfter1Frame(() => textField.SelectRange(textField.text.Length, textField.text.Length)));
            textField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = (T)(object)textField.value;
                OnValueChangedFromEdit?.Invoke(prev, Value);
                textField.RemoveFromHierarchy();
                RemoveFromClassList("input-cell");
                Add(_body);
                _isEditing = false;
            });

            this.ExecAfter1Frame(() => textField.Focus());
        }

        private void StartEditingAsInt(int value = int.MinValue)
        {
            _isEditing = true;

            var integerField = new IntegerField { value = value != int.MinValue ? value : Convert.ToInt32(Value), };
            integerField.style.width = Width;
            AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Add(integerField);

            integerField.RegisterCallback<FocusInEvent>(_ => this.ExecAfter1Frame(() => integerField.SelectRange(integerField.text.Length, integerField.text.Length)));
            integerField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = (T)(object)integerField.value;
                OnValueChangedFromEdit?.Invoke(prev, Value);
                integerField.RemoveFromHierarchy();
                RemoveFromClassList("input-cell");
                Add(_body);
                _isEditing = false;
            });

            schedule.Execute(() => integerField.Focus()).ExecuteLater(0);
        }

        private void StartEditingAsFloat(float value = float.MinValue)
        {
            _isEditing = true;

            // く、苦しい～～～！ｗ（float.MinValue + 1
            var floatField = new FloatField { value = value > float.MinValue + 1 ? value : Convert.ToSingle(Value), };
            floatField.style.width = Width;
            AddToClassList("input-cell");

            _body.RemoveFromHierarchy();
            Add(floatField);

            floatField.RegisterCallback<FocusInEvent>(_ => this.ExecAfter1Frame(() => floatField.SelectRange(floatField.text.Length, floatField.text.Length)));
            floatField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var prev = Value;
                Value = (T)(object)floatField.value;
                OnValueChangedFromEdit?.Invoke(prev, Value);
                floatField.RemoveFromHierarchy();
                RemoveFromClassList("input-cell");
                Add(_body);
                _isEditing = false;
            });

            schedule.Execute(() => floatField.Focus()).ExecuteLater(0);
        }

        #endregion
    }
}
