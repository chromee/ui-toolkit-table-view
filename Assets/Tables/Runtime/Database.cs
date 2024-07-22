using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tables.Runtime
{
    public abstract class Database : ScriptableObject
    {
        [SerializeField] private ColumnMetadata[] _columns;
        public ColumnMetadata[] Columns => _columns;

        public abstract object[] GetData();
        public abstract object[][] GetDataAsArray();

        public abstract ValidationResult Validate(ColumnMetadata column, object data, object value);

        protected abstract Type GetDataType();

        [ContextMenu("Set up Columns")]
        public void SetUpColumns()
        {
            var type = GetDataType();
            var fields = type.GetFields();
            var columns = new List<ColumnMetadata>();
            foreach (var field in fields)
            {
                var column = new ColumnMetadata
                {
                    Name = field.Name,
                    TypeName = ColumnMetadata.TypeToString(field.FieldType),
                    Width = 100,
                };
                columns.Add(column);
            }

            _columns = columns.ToArray();
        }
    }

    public abstract class Database<T> : Database
    {
        [SerializeField] private T[] _data;
        public T[] Data => _data;

        public override object[] GetData() => Data as object[];

        public override ValidationResult Validate(ColumnMetadata column, object data, object value)
        {
            if (this is IValidator<T> validator)
            {
                return validator.Validate(column, (T)data, value);
            }
            else
            {
                return ValidationResult.Success();
            }
        }

        protected override Type GetDataType() => typeof(T);
    }

    [Serializable]
    public class ColumnMetadata
    {
        public string Name;
        public string TypeName;
        public float Width;

        private Type _type;

        public Type Type => _type ??= TypeName switch
        {
            "string" => typeof(string),
            "int" => typeof(int),
            "float" => typeof(float),
            "bool" => typeof(bool),
            _ => Type.GetType(TypeName),
        };

        public static string TypeToString(Type type) =>
            type.IsEnum ?
                type.AssemblyQualifiedName :
                Type.GetTypeCode(type) switch
                {
                    TypeCode.String => "string",
                    TypeCode.Int32 => "int",
                    TypeCode.Single => "float",
                    TypeCode.Boolean => "bool",
                    _ => type.AssemblyQualifiedName,
                };

        public object GetDefaultValue()
        {
            return
                Type.IsEnum ?
                    Enum.ToObject(Type, 0) :
                    Type.GetTypeCode(Type) switch
                    {
                        TypeCode.String => string.Empty,
                        TypeCode.Int32 => 0,
                        TypeCode.Single => 0f,
                        TypeCode.Boolean => false,
                        _ => Activator.CreateInstance(Type),
                    };
        }
    }
}
