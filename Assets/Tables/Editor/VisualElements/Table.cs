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

        public readonly FooterRow FooterRow;

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

            FooterRow = new FooterRow();
            Add(FooterRow);
        }

        public DataRow AddDataRow()
        {
            var index = _dataRows.Count;

            _dataListProperty.InsertArrayElementAtIndex(index);
            var rowProperty = _dataListProperty.GetArrayElementAtIndex(index);
            _dataListProperty.serializedObject.ApplyModifiedProperties();

            var data = _database.GetData();
            var dataRow = new DataRow(index, _database.Columns, data[index], null, rowProperty);
            Insert(Children().Count() - 1, dataRow);

            _dataRows.Add(dataRow);
            OnRowAdded?.Invoke(dataRow);

            return dataRow;
        }

        public void RemoveDataRow(DataRow dataRow)
        {
            var index = dataRow.Index;
            _dataListProperty.DeleteArrayElementAtIndex(index);
            _dataListProperty.serializedObject.ApplyModifiedProperties();

            _dataRows.Remove(dataRow);
            dataRow.RemoveFromHierarchy();
        }

        public void MoveDataRow(DataRow[] moveRows, int toIndex)
        {
            if (moveRows == null || !moveRows.Any()) return;

            var stIndex = moveRows.Min(row => row.Index);
            var edIndex = moveRows.Max(row => row.Index);
            var isMoveToUp = toIndex < stIndex;
            var changeSize = moveRows.Length;
            if (stIndex <= toIndex && toIndex <= edIndex) return;

            for (var i = 0; i < changeSize; i++) _dataListProperty.MoveArrayElement(isMoveToUp ? edIndex : stIndex, toIndex);
            _dataListProperty.serializedObject.ApplyModifiedProperties();

            // NOTE: MoveArrayElement & ApplyModifiedProperties の結果、元データの参照はそのままに中身の移動した値が書き換わる。罠すぎワロタ（笑えない）

            var (updateSt, updateEd) = isMoveToUp ? (toIndex, edIndex) : (stIndex, toIndex);
            var (upSt, upEd, downSt, downEd) = isMoveToUp ?
                (stIndex, edIndex, toIndex, stIndex - 1) :
                (edIndex + 1, toIndex, stIndex, edIndex);
            var upCount = isMoveToUp ? upSt - downSt : changeSize;
            var downCount = isMoveToUp ? changeSize : upSt - downSt;

            var moves = new List<(int from, int to)>();
            for (var i = updateSt; i <= updateEd; i++)
            {
                if (upSt <= i && i <= upEd) moves.Add((i, i - upCount));
                else if (downSt <= i && i <= downEd) moves.Add((i, i + downCount));
            }

            foreach (var (from, to) in moves)
            {
                var dataRow = _dataRows[from];
                dataRow.UpdateIndex(to);
                Remove(dataRow);
                Insert(to + 1, dataRow);
            }

            for (var i = 0; i < changeSize; i++)
            {
                var index = isMoveToUp ? edIndex : stIndex;
                var dataRow = _dataRows[index];
                _dataRows.RemoveAt(index);
                _dataRows.Insert(toIndex, dataRow);
            }
        }
    }
}
