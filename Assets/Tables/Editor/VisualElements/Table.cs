using System;
using System.Collections.Generic;
using System.Linq;
using Tables.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class Table : VisualElement
    {
        private readonly Database _database;
        private readonly SerializedProperty _dataListProperty;

        public readonly HeaderRow HeaderRow;
        private readonly List<DataRow> _dataRows = new();
        public IReadOnlyList<DataRow> DataRows => _dataRows;
        public readonly EmptyRow EmptyRow;

        public event Action<DataRow> OnRowAdded;

        public Table(Database database)
        {
            _database = database;

            var serializedObject = new SerializedObject(_database);
            _dataListProperty = serializedObject.FindProperty("_data");

            AddToClassList("table");

            HeaderRow = new HeaderRow(_database.Columns);
            Add(HeaderRow);

            var data = _database.GetData();
            var dataMatrix = _database.GetDataAsArray();
            if (dataMatrix != null)
            {
                for (var i = 0; i < dataMatrix.Length; i++)
                {
                    var dataProperty = _dataListProperty.GetArrayElementAtIndex(i);
                    var dataRow = new DataRow(i, _database.Columns, data[i], dataMatrix[i], dataProperty);
                    _dataRows.Add(dataRow);
                    OnRowAdded?.Invoke(dataRow);
                    Add(dataRow);
                }
            }

            EmptyRow = new EmptyRow(dataMatrix?.Length ?? 0, _database.Columns);
            Add(EmptyRow);
        }

        public DataRow AddDataRow(object[] rowValues)
        {
            var index = _dataRows.Count;
            _dataListProperty.InsertArrayElementAtIndex(index);
            var rowProperty = _dataListProperty.GetArrayElementAtIndex(index);

            var dataRow = new DataRow(index, _database.Columns, rowProperty.boxedValue, rowValues, rowProperty);
            Insert(Children().Count() - 1, dataRow);

            _dataListProperty.serializedObject.ApplyModifiedProperties();

            rowProperty = _dataListProperty.GetArrayElementAtIndex(index);
            dataRow.SetData(rowProperty.boxedValue);

            _dataRows.Add(dataRow);
            OnRowAdded?.Invoke(dataRow);

            return dataRow;
        }
    }
}
