using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class FooterRow : VisualElement
    {
        public Button AddRowButton { get; }

        public FooterRow()
        {
            AddToClassList("row");

            AddRowButton = new Button { text = "+" };
            AddRowButton.AddToClassList("add-row-button");
            Add(AddRowButton);
        }
    }
}
