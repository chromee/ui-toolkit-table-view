using System.Collections.Generic;
using Editor.Sample;
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

            var db = new PersonDatabase();
            var conInfos = new List<ColInfo>();
            foreach (var c in db.Columns) conInfos.Add(c.ToColInfo());

            var values = new List<object[]>();
            foreach (var p in db.Persons)
            {
                values.Add(new object[]
                {
                    p.Id,
                    p.Name,
                    p.Height,
                    p.Gender,
                });
            }

            _tableManager = new TableManager(rootVisualElement, conInfos.ToArray(), values.ToArray());
            _tableManager.ShortcutKeySystem.SetupRootVisualElementForKeyboardInput();
            _tableManager.ShortcutKeySystem.RegisterShortcuts();
        }

        private void OnEnable()
        {
            _tableManager?.ShortcutKeySystem.RegisterShortcuts();
        }

        private void OnDisable()
        {
            _tableManager?.ShortcutKeySystem.UnregisterShortcuts();
        }

        // デバッグ用
        // private void OnGUI()
        // {
        //     Debug.Log(rootVisualElement.panel.focusController.focusedElement);
        // }
    }
}
