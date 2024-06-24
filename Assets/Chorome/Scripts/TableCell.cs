using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Chorome
{
    public class TableCell : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<TableCell, UxmlTraits> { }

        // ファクトリによるColorAndTextの初期化時に使うクラス
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            // UXMLの属性を定義
            UxmlStringAttributeDescription _text = new() { name = "text", defaultValue = "cell" };

            // 子を持たない場合はこのように書く
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var tableCell = ve as TableCell;
                tableCell!.Text = _text.GetValueFromBag(bag, cc);
            }
        }

        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        private readonly Label _label;

        public TableCell()
        {
            _label = new Label();
            SetMargin(_label, 0);
            SetPadding(_label, 0);
            Add(_label);

            AddToClassList("table-cell");
        }

        private static void SetMargin(VisualElement element, float px)
        {
            element.style.marginLeft = px;
            element.style.marginTop = px;
            element.style.marginRight = px;
            element.style.marginBottom = px;
        }

        private static void SetPadding(VisualElement element, float px)
        {
            element.style.paddingLeft = px;
            element.style.paddingTop = px;
            element.style.paddingRight = px;
            element.style.paddingBottom = px;
        }
    }
}
