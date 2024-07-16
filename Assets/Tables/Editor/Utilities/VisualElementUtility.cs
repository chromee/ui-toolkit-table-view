using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tables.Editor.Utilities
{
    public static class VisualElementUtility
    {
        public static void ExecAfterFrame(this VisualElement ve, Action action, int delayFrame = 0)
        {
            ve.schedule.Execute(action).ExecuteLater(delayFrame);
        }

        public static void ForceUpdate(this ScrollView view)
        {
            view.schedule.Execute(() =>
            {
                var fakeOldRect = Rect.zero;
                var fakeNewRect = view.layout;

                using var evt = GeometryChangedEvent.GetPooled(fakeOldRect, fakeNewRect);
                evt.target = view.contentContainer;
                view.contentContainer.SendEvent(evt);
            });
        }
    }
}
