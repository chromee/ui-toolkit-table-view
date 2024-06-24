using UnityEngine.UIElements;

namespace Chorome
{
    /// <summary>
    /// Text field with blinking caret.
    /// <list type="bullet">
    ///     <listheader>Exposed UXML properties</listheader>
    ///     <item>blink-interval (in ms)</item>
    ///     <item>blink-enable (if true, caret blinks)</item>
    ///     <item>blink-style (uss style which applies to caret on blink)</item>
    /// </list>
    /// </summary>
    public class BlinkingTextField : TextField
    {
        private readonly IVisualElementScheduledItem _blink;

        private long _blinkInterval = 500;
        private bool _isBlinkEnabled = true;
        private string _blinkStyle = "cursor-transparent";

        /// <summary>
        /// Caret blink interval in ms.
        /// </summary>
        public long BlinkInterval
        {
            get => _blinkInterval;
            set
            {
                _blinkInterval = value;
                _blink?.Every(_blinkInterval);
            }
        }

        /// <summary>
        /// Caret uss style applied on blink.
        /// </summary>
        public string BlinkStyle
        {
            get => _blinkStyle;
            set => _blinkStyle = value;
        }

        /// <summary>
        /// If true, caret blinks.
        /// </summary>
        public bool BlinkEnable
        {
            get => _isBlinkEnabled;
            set
            {
                if (_isBlinkEnabled == value) return;

                _isBlinkEnabled = value;

                if (!_isBlinkEnabled)
                {
                    if (IsFocused) _blink?.Pause();
                    if (ClassListContains(_blinkStyle)) RemoveFromClassList(_blinkStyle);
                }
                else if (IsFocused)
                {
                    _blink?.Resume();
                }
            }
        }

        /// <summary>
        /// Returns true if active input.
        /// </summary>
        public bool IsFocused => focusController?.focusedElement == this;

        public BlinkingTextField()
        {
            RegisterCallback<FocusEvent>(OnFocus);
            RegisterCallback<BlurEvent>(OnInputEnded);

            _blink = schedule.Execute(() =>
                {
                    if (ClassListContains(_blinkStyle)) RemoveFromClassList(_blinkStyle);
                    else AddToClassList(_blinkStyle);
                }).
                Every(_blinkInterval);

            _blink.Pause();
        }

        private void OnFocus(FocusEvent evt)
        {
            if (!_isBlinkEnabled) return;
            _blink.Resume();
        }

        private void OnInputEnded(BlurEvent evt)
        {
            _blink.Pause();
        }

        public new class UxmlFactory : UxmlFactory<BlinkingTextField, BlinkingUxmlTraits> { }

        [UnityEngine.Scripting.Preserve]
        public class BlinkingUxmlTraits : UxmlTraits
        {
            private readonly UxmlLongAttributeDescription _blinkInterval = new() { name = "blink-interval", use = UxmlAttributeDescription.Use.Optional, defaultValue = 500 };
            private readonly UxmlBoolAttributeDescription _blinkEnable = new() { name = "blink-enable", use = UxmlAttributeDescription.Use.Optional, defaultValue = true };
            private readonly UxmlStringAttributeDescription _blinkStyle = new() { name = "blink-style", use = UxmlAttributeDescription.Use.Optional, defaultValue = "cursor-transparent" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((BlinkingTextField)ve).BlinkInterval = _blinkInterval.GetValueFromBag(bag, cc);
                ((BlinkingTextField)ve).BlinkEnable = _blinkEnable.GetValueFromBag(bag, cc);
                ((BlinkingTextField)ve).BlinkStyle = _blinkStyle.GetValueFromBag(bag, cc);
            }
        }
    }
}
