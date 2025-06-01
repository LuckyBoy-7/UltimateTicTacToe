using Lucky.Kits.Interactive;
using UnityEngine;

namespace Test
{
    public class Test : InteractableScreenUI
    {
        public bool debug;

        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();
            if (debug)
                transform.position = GameCursor.MouseWorldPosition;
        }

        protected override void OnCursorDrag(bool hasStart, Vector2 delta)
        {
            base.OnCursorDrag(hasStart, delta);
            if (hasStart)
                // transform.position = GameCursor.MouseWorldPosition;
                RectTransform.anchoredPosition = GameCursor.MouseScreenPosition;
        }
    }
}