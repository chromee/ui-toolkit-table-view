using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Data;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.VisualElements
{
    public class Table : VisualElement
    {
        private readonly Database _database;
        private readonly SerializedObject _serializedObject;
        private readonly SerializedProperty _dataListProperty;

        public readonly HeaderRow HeaderRow;
        private readonly List<DataRow> _dataRows = new();
        public IReadOnlyList<DataRow> DataRows => _dataRows;
        public readonly EmptyRow EmptyRow;

        public Table(Database database)
        {
            _database = database;

            _serializedObject = new SerializedObject(_database);
            _dataListProperty = _serializedObject.FindProperty("_data");

            AddToClassList("table");

            HeaderRow = new HeaderRow(_database.Columns);
            Add(HeaderRow);

            var data = _database.GetData();
            if (data != null)
            {
                for (var i = 0; i < data.Length; i++)
                {
                    var dataProperty = _dataListProperty.GetArrayElementAtIndex(i);
                    var dataRow = new DataRow(i, _database.Columns, data[i], dataProperty);
                    _dataRows.Add(dataRow);
                    Add(dataRow);
                }
            }

            // Create empty row
            EmptyRow = new EmptyRow(data?.Length ?? 0, _database.Columns);
            Add(EmptyRow);
        }

        public DataRow AddDataRow(object[] rowValues)
        {
            var index = _dataRows.Count;
            _dataListProperty.InsertArrayElementAtIndex(index);
            var dataProperty = _dataListProperty.GetArrayElementAtIndex(index);
            var dataRow = new DataRow(index, _database.Columns, rowValues, dataProperty);
            _dataRows.Add(dataRow);
            Insert(Children().Count() - 1, dataRow);
            _serializedObject.ApplyModifiedProperties();
            return dataRow;
        }
    }
}
