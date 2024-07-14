using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Utilities
{
    public static class VisualElementUtility
    {
        public static void ExecAfter1Frame(this VisualElement ve, Action action)
        {
            ve.schedule.Execute(action).ExecuteLater(0);
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
