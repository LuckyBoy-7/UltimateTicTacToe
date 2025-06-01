namespace Lucky.Kits.Interactive
{
    public class InteractableWorldUI : InteractableUIBase
    {
        public override bool CursorInBounds()
        {
            // 此时相对世界坐标系
            return GetWorldRect(RectTransform).Contains(GameCursor.MouseWorldPosition);
        }

        protected virtual void OnEnable()
        {
            GameCursor.Instance?.RegisterInteractable<InteractableWorldUI>(this);
        }

        protected virtual void OnDisable()
        {
            GameCursor.Instance?.UnregisterInteractable<InteractableWorldUI>(this);
        }
    }
}