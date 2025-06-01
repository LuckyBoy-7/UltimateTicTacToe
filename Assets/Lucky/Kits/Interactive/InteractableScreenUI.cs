namespace Lucky.Kits.Interactive
{
    public class InteractableScreenUI : InteractableUIBase
    {
        public override bool CursorInBounds()
        {
            // 此时相对屏幕坐标系, canvas的宽高正好是屏幕的宽高, 所以正常情况canvas大小能和MouseScreenPositionu范围对上
            return GetWorldRect(RectTransform).Contains(GameCursor.MouseScreenPosition);
        }

        protected void OnEnable()
        {
            GameCursor.Instance?.RegisterInteractable<InteractableScreenUI>(this);
        }

        protected void OnDisable()
        {
            GameCursor.Instance?.UnregisterInteractable<InteractableScreenUI>(this);
        }
    }
}