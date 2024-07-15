using UnityEngine;
using UnityEngine.UIElements;

namespace Tables.Editor.VisualElements
{
    public class Marker : VisualElement
    {
        private readonly VisualElement _rootVisualElement;

        public bool IsVisible
        {
            get => style.display == DisplayStyle.Flex;
            set => style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public Marker(VisualElement rootVisualElement, string className)
        {
            _rootVisualElement = rootVisualElement;
            AddToClassList(className);
            pickingMode = PickingMode.Ignore;
            IsVisible = false;
            _rootVisualElement.Add(this);
        }

        public void Fit(Cell cell)
        {
            if (cell == null) return;
            FitTopLeftToBotRight(cell, cell);
        }

        public void Fit(Cell startCell, Cell endCell)
        {
            if (startCell == null) return;

            if (endCell == null) Fit(startCell);
            else if (startCell.Position == endCell.Position) Fit(startCell);
            else if (startCell.Row <= endCell.Row) // startが上
            {
                if (startCell.Col <= endCell.Col) FitTopLeftToBotRight(startCell, endCell);
                else FitTopRightToBotLeft(startCell, endCell);
            }
            else // endが上
            {
                if (endCell.Col <= startCell.Col) FitTopLeftToBotRight(endCell, startCell);
                else FitTopRightToBotLeft(endCell, startCell);
            }
        }

        private void FitTopLeftToBotRight(Cell leftTop, Cell rightBot)
        {
            if (leftTop == null || rightBot == null) return;

            var startPos = leftTop.worldBound.position;
            var endPos = rightBot.worldBound.position + rightBot.worldBound.size;
            var rootBound = _rootVisualElement.worldBound;

            style.left = startPos.x - rootBound.x;
            style.top = startPos.y - rootBound.y;
            style.width = endPos.x - startPos.x - 1;
            style.height = endPos.y - startPos.y - 1;
        }

        private void FitTopRightToBotLeft(Cell rightTop, Cell leftBot)
        {
            if (rightTop == null || leftBot == null) return;

            var leftBotPos = leftBot.worldBound.position;
            var rightTopPos = rightTop.worldBound.position;
            var startPos = new Vector2(leftBotPos.x, rightTopPos.y);
            var endPos = new Vector2(rightTopPos.x + rightTop.worldBound.size.x, leftBotPos.y + leftBot.worldBound.size.y);
            var rootBound = _rootVisualElement.worldBound;

            style.left = startPos.x - rootBound.x;
            style.top = startPos.y - rootBound.y;
            style.width = endPos.x - startPos.x - 1;
            style.height = endPos.y - startPos.y - 1;
        }
    }
}
