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
        public readonly ColumnMetadata Metadata;
        protected readonly SerializedProperty RowProperty;
        protected readonly SerializedProperty CellProperty;

        public int Row;
        public int Col;
        public Vector2 Position => new(Col, Row);

        public abstract object GetValue();

        public event Action<object, object> OnValueChanged;
        public event Action<object, object> OnValueChangedFromEdit;

        public float Width
        {
            get => resolvedStyle.width;
            set => style.width = value;
        }

        public static Cell Create<T>(int row, int col, T value, ColumnMetadata metadata, SerializedProperty rowProperty)
        {
            return value switch
            {
                string sv => new StringCell(row, col, sv, metadata, rowProperty),
                int iv => new IntCell(row, col, iv, metadata, rowProperty),
                float fv => new FloatCell(row, col, fv, metadata, rowProperty),
                bool bv => new BoolCell(row, col, bv, metadata, rowProperty),
                Enum ev => new EnumCell(row, col, ev, metadata, rowProperty),
                _ => new StringCell(row, col, value.ToString(), metadata, rowProperty),
            };
        }

        protected Cell(int row, int col, ColumnMetadata metadata, SerializedProperty rowProperty)
        {
            Row = row;
            Col = col;
            Metadata = metadata;
            RowProperty = rowProperty;
            CellProperty = rowProperty?.FindPropertyRelative(metadata.Name);
            Width = metadata.Width;

            AddToClassList("cell");
        }

        public abstract void StartEditing();
        public abstract void StartEditingByKeyDown(KeyDownEvent evt);
        public abstract bool TryPaste(Cell from);
        public abstract void ClearValue();
        public Cell<T> As<T>() => this as Cell<T>;

        protected void ValueChange(object prev, object current) => OnValueChanged?.Invoke(prev, current);
        protected void ValueChangeFromEdit(object prev, object current) => OnValueChangedFromEdit?.Invoke(prev, current);

        public void ChangeStatus(ValidationResult result)
        {
            switch (result.ResultType)
            {
                case ValidationResult.Type.Success:
                    RemoveFromClassList("error-cell");
                    RemoveFromClassList("warning-cell");
                    tooltip = null;
                    break;
                case ValidationResult.Type.Warning:
                    AddToClassList("warning-cell");
                    tooltip = result.Message;
                    break;
                case ValidationResult.Type.Error:
                    AddToClassList("error-cell");
                    tooltip = result.Message;
                    break;
            }
        }
    }

    public abstract class Cell<T> : Cell
    {
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                var prev = _value;
                _value = value;
                ValueChange(prev, _value);
                RefreshView();
            }
        }

        public override object GetValue() => Value;

        protected Cell(int row, int col, T value, ColumnMetadata metadata, SerializedProperty rowProperty) : base(row, col, metadata, rowProperty)
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
