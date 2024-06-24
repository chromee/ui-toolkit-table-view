using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TableView : EditorWindow
{
    private const int InitialRowCount = 10;
    private const int InitialColCount = 10;
     
    [MenuItem("Window/TableView")]
    public static void ShowExample()
    {
        TableView wnd = GetWindow<TableView>();
        wnd.titleContent = new GUIContent("TableView");
    }

    public void CreateGUI()
    {
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TableView.uxml");
        VisualElement root = visualTree.CloneTree();
        root.style.flexGrow = 1.0f;
        rootVisualElement.Add(root);
        
        var table = root.Q<VisualElement>("table");
        var headerRow = table.Q<VisualElement>("header-row");

        // ヘッダーセルを作成
        for (int col = 0; col < InitialColCount; col++)
        {
            var headerCell = new Label($"Header {col + 1}");
            headerCell.AddToClassList("cell");
            headerCell.AddToClassList("editable-cell");
            headerRow.Add(headerCell);

            // 列幅変更イベントを追加
            AddColumnResizer(headerCell, col);
        }

        // データ行を作成
        for (int row = 0; row < InitialRowCount; row++)
        {
            var dataRow = new VisualElement();
            dataRow.AddToClassList("row");
            for (int col = 0; col < InitialColCount; col++)
            {
                var dataCell = new Label($"{row},{col}");
                dataCell.AddToClassList("cell");
                dataCell.AddToClassList("editable-cell");
                dataRow.Add(dataCell);

                // セル編集イベントを追加
                AddCellEditor(dataCell);
            }
            table.Add(dataRow);
        }
    }

    private void AddColumnResizer(VisualElement headerCell, int colIndex)
    {
        var resizer = new VisualElement();
        resizer.AddToClassList("resizer");

        bool isResizing = false;
        float initialWidth = 0;
        float initialMousePos = 0;

        resizer.RegisterCallback<MouseDownEvent>(evt =>
        {
            isResizing = true;
            initialWidth = headerCell.resolvedStyle.width;
            initialMousePos = evt.localMousePosition.x;
            evt.StopPropagation();
        });

        resizer.RegisterCallback<MouseMoveEvent>(evt =>
        {
            if (isResizing)
            {
                float delta = evt.localMousePosition.x - initialMousePos;
                headerCell.style.width = initialWidth + delta;

                foreach (var row in headerCell.parent.parent.Children())
                {
                    var cell = row.ElementAt(colIndex);
                    cell.style.width = initialWidth + delta;
                }

                evt.StopPropagation();
            }
        });

        resizer.RegisterCallback<MouseUpEvent>(evt =>
        {
            isResizing = false;
            evt.StopPropagation();
        });

        headerCell.Add(resizer);
    }

    private void AddCellEditor(Label cell)
    {
        cell.RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.clickCount == 2)
            {
                var textField = new TextField { value = cell.text };
                textField.style.width = cell.resolvedStyle.width;
                textField.RegisterCallback<FocusOutEvent>(e =>
                {
                    cell.text = textField.value;
                    cell.parent.Add(cell);
                    textField.RemoveFromHierarchy();
                });
                cell.parent.Add(textField);
                textField.Focus();
                textField.SelectAll();
                cell.RemoveFromHierarchy();
            }
        });
    }
}
