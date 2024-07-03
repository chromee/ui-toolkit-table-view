using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Editor.VisualElements
{
    public class Table : VisualElement
    {
        public readonly HeaderRow HeaderRow;
        private readonly List<DataRow> _dataRows = new();
        public IReadOnlyList<DataRow> DataRows => _dataRows;
        public readonly EmptyRow EmptyRow;

        public Table(ColInfo[] colInfos, object[][] rowValues = null)
        {
            AddToClassList("table");

            HeaderRow = new HeaderRow(colInfos);
            Add(HeaderRow);

            // Create data rows
            if (rowValues != null)
            {
                for (var index = 0; index < rowValues.Length; index++)
                {
                    var dataRow = new DataRow(index, colInfos, rowValues[index]);
                    _dataRows.Add(dataRow);
                    Add(dataRow);
                }
            }

            // Create empty row
            EmptyRow = new EmptyRow(rowValues?.Length ?? 0, colInfos);
            Add(EmptyRow);
        }

        public void AddDataRow(ColInfo[] colInfos, object[] rowValues)
        {
            var dataRow = new DataRow(_dataRows.Count, colInfos, rowValues);
            _dataRows.Add(dataRow);
            Insert(Children().Count() - 1, dataRow);
        }
    }

    public class ColInfo
    {
        public readonly Type Type;
        public readonly string Name;
        public readonly float Width;

        public ColInfo(Type type, string name, float width)
        {
            Type = type;
            Name = name;
            Width = width;
        }
    }
}
