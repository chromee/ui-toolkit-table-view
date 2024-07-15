using System;
using Tables.Editor.VisualElements.Cells;
using Tables.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public abstract class Cell : VisualElement
    {
        protected readonly ColumnMetadata Metadata;
        protected readonly SerializedObject SerializedObject;
        protected readonly SerializedProperty DataProperty;

        public int Row;
        public int Col;
        public Vector2 Position => new(Col, Row);

        public abstract object Val { get; }
        public event Action<object, object> OnValueChangedFromEdit;

        public float Width
        {
            get => resolvedStyle.width;
            set => style.width = value;
        }

        public static Cell Create<T>(int row, int col, T value, ColumnMetadata metadata, SerializedObject serializedObject, SerializedProperty dataProperty)
        {
            return value switch
            {
                string sv => new StringCell(row, col, sv, metadata, serializedObject, dataProperty),
                int iv => new IntCell(row, col, iv, metadata, serializedObject, dataProperty),
                float fv => new FloatCell(row, col, fv, metadata, serializedObject, dataProperty),
                bool bv => new BoolCell(row, col, bv, metadata, serializedObject, dataProperty),
                Enum ev => new EnumCell(row, col, ev, metadata, serializedObject, dataProperty),
                _ => new StringCell(row, col, value.ToString(), metadata, serializedObject, dataProperty),
            };
        }

        protected Cell(int row, int col, ColumnMetadata metadata, SerializedObject serializedObject, SerializedProperty dataProperty)
        {
            Row = row;
            Col = col;
            Metadata = metadata;
            SerializedObject = serializedObject;
            DataProperty = dataProperty;
            Width = metadata.Width;

            AddToClassList("cell");
        }

        public abstract void StartEditing();
        public abstract void StartEditingByKeyDown(KeyDownEvent evt);
        public abstract bool TryPaste(Cell from);
        public abstract void ClearValue();
        public Cell<T> As<T>() => this as Cell<T>;
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

        protected Cell(int row, int col, T value, ColumnMetadata metadata, SerializedObject serializedObject, SerializedProperty dataProperty) : base(row, col, metadata, serializedObject, dataProperty)
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
