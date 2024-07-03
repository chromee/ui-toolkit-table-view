using Editor.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class SpreadsheetEditorWindow : EditorWindow
    {
        [MenuItem("Window/Spreadsheet Editor")]
        public static void ShowExample()
        {
            var wnd = GetWindow<SpreadsheetEditorWindow>();
            wnd.titleContent = new GUIContent("Spreadsheet Editor");
        }

        private TableManager _tableManager;

        public void CreateGUI()
        {
            // Load USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/SpreadsheetEditor.uss");
            rootVisualElement.styleSheets.Add(styleSheet);

            var tableInfo = new[]
            {
                new ColInfo(typeof(string), "String Value", 100f),
                new ColInfo(typeof(int), "Int Value", 100f),
                new ColInfo(typeof(float), "Float Value", 100f),
                new ColInfo(typeof(bool), "✓", 30f),
                new ColInfo(typeof(string), "String Value 2", 200f),
            };

            var values = new object[][]
            {
                new object[] { "A", 1, 1.1f, true, "Z" },
                new object[] { "B", 1, 1.1f, false, "Y" },
                new object[] { "C", 1, 1.1f, true, "X" },
                new object[] { "D", 1, 1.1f, false, "W" },
                new object[] { "E", 1, 1.1f, true, "V" },
            };

            _tableManager = new TableManager(rootVisualElement, tableInfo, values);
        }

        private void OnEnable()
        {
            _tableManager.ShortcutKeySystem.RegisterShortcuts();
            _tableManager.ShortcutKeySystem.SetupRootVisualElementForKeyboardInput();
        }

        private void OnDisable()
        {
            _tableManager.ShortcutKeySystem.UnregisterShortcuts();
        }

        // デバッグ用
        // private void OnGUI()
        // {
        //     Debug.Log(rootVisualElement.panel.focusController.focusedElement);
        // }
    }
}
