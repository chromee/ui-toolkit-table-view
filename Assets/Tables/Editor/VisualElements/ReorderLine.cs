using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class ReorderLine : VisualElement
    {
        private readonly VisualElement _rootVisualElement;

        public bool IsVisible
        {
            get => style.display == DisplayStyle.Flex;
            set => style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public ReorderLine(VisualElement rootVisualElement)
        {
            _rootVisualElement = rootVisualElement;
            AddToClassList("reorder-line");
            pickingMode = PickingMode.Ignore;
            IsVisible = false;
            _rootVisualElement.Add(this);
        }
    }
}
