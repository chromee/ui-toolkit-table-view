using System;
using Editor.VisualElements.Cells;
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
}
