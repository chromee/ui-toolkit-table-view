using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class ReorderShadow : VisualElement
    {
        private readonly VisualElement _rootVisualElement;

        public bool IsVisible
        {
            get => style.display == DisplayStyle.Flex;
            set => style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public ReorderShadow(VisualElement rootVisualElement)
        {
            _rootVisualElement = rootVisualElement;
            AddToClassList("reorder-shadow");
            pickingMode = PickingMode.Ignore;
            IsVisible = false;
            _rootVisualElement.Add(this);
        }
    }
}
