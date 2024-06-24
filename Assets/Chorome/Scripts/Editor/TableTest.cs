using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chorome.Scripts.Editor
{
    public class TableTest : EditorWindow
    {
        [MenuItem("Zzzzzzzzzzzz/TableTest")]
        private static void ShowWindow()
        {
            var window = GetWindow<TableTest>();
            window.titleContent = new GUIContent("TITLE");
            window.Show();
        }

        public List<Row> Cells = new();
        
        public int ColumnSize = 5;
        public int RowSize = 30;

        private static readonly float DefaultRowHeight = 14;
        private static readonly float DefaultColumnWidth = 100;

        private void CreateGUI()
        {
            var tableRoot = new ScrollView();
            tableRoot.AddToClassList("table-root");
            rootVisualElement.Add(tableRoot);

            for (var i = 0; i < RowSize; i++)
            {
                var row = new Row();
                Cells.Add(row);

                var rowElement = new VisualElement();
                rowElement.AddToClassList("table-row");
                rowElement.style.height = DefaultRowHeight;
                rowElement.style.flexDirection = FlexDirection.Row;
                tableRoot.Add(rowElement);

                for (var j = 0; j < ColumnSize; j++)
                {
                    row.Add(new Cell { X = i, Y = j });
                    var cell = new VisualElement();
                    cell.AddToClassList("table-cell");
                    cell.style.width = DefaultColumnWidth;
                    cell.style.height = DefaultRowHeight;

                    cell.Add(new Label("cell"));
                    rowElement.Add(cell);
                }
            }
        }

        public class Row : List<Cell> { }

        public class Cell
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
