using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class OriginCell : VisualElement
    {
        public OriginCell()
        {
            AddToClassList("cell");
            AddToClassList("index-cell");
            AddToClassList("origin-cell");
        }
    }
}
