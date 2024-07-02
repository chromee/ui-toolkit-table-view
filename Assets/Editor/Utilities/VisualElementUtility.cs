using System;
using UnityEngine.UIElements;

namespace Editor.Utilities
{
    public static class VisualElementUtility
    {
        public static void ExecAfter1Frame(this VisualElement ve, Action action)
        {
            ve.schedule.Execute(action).ExecuteLater(0);
        }
    }
}
