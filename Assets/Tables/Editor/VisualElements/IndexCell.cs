using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class IndexCell : VisualElement
    {
        private readonly Label _label;

        public IndexCell(int index)
        {
            AddToClassList("cell");
            AddToClassList("index-cell");
            _label = new Label(index.ToString());
            Add(_label);
        }

        public void UpdateIndex(int index) => _label.text = index.ToString();
    }
}
