using Tables.Runtime;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tables.Editor
{
    public class SpreadsheetEditorWindow : EditorWindow
    {
        private Database _database;
        private TableManager _tableManager;

        private const string UxmlGuid = "f553fca5bf120624a864195eeeb2d85a";
        private const string StyleSheetGuid = "a6b8a1926b674ae6985aea2e420c4d0a";
        private static string UxmlPath => AssetDatabase.GUIDToAssetPath(UxmlGuid);
        private static string StyleSheetPath => AssetDatabase.GUIDToAssetPath(StyleSheetGuid);

        public static void Open(Database database)
        {
            if (HasOpenInstances<SpreadsheetEditorWindow>())
            {
                var existingWindow = GetWindow<SpreadsheetEditorWindow>();
                if (existingWindow._database == database)
                {
                    SetupWindow(existingWindow);
                    existingWindow.Focus();
                    return;
                }

                if (existingWindow._database == null)
                {
                    existingWindow.Close();
                }
            }

            var window = CreateInstance<SpreadsheetEditorWindow>();
            window._database = database;
            SetupWindow(window);
            window.Show();
        }

        private static void SetupWindow(SpreadsheetEditorWindow window)
        {
            var database = window._database;
            if (database == null) return;

            window.titleContent = new GUIContent(database.name);

            var doc = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);

            var rootVisualElement = window.rootVisualElement;
            var uxml = doc.CloneTree();

            rootVisualElement.Clear();

            rootVisualElement.styleSheets.Add(styleSheet);
            rootVisualElement.Add(uxml);

            window._tableManager = new TableManager(database, rootVisualElement);
            window._tableManager.ShortcutKeySystem.SetupRootVisualElementForKeyboardInput();
            window._tableManager.ShortcutKeySystem.RegisterShortcuts();
        }

        private void OnEnable()
        {
            if (_database != null) SetupWindow(this);
            _tableManager?.ShortcutKeySystem.RegisterShortcuts();
        }

        private void OnDisable()
        {
            _tableManager?.ShortcutKeySystem.UnregisterShortcuts();
        }
    }

    public static class OnOpenDatabase
    {
        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID);

            if (asset is not Database database) return false;

            SpreadsheetEditorWindow.Open(database);

            return true;
        }
    }
}
