using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class IndexCell : VisualElement
    {
        public IndexCell(int index)
        {
            AddToClassList("cell");
            AddToClassList("index-cell");
            Add(new Label(index.ToString()));
        }
    }
}
