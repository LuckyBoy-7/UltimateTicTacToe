using Lucky.Kits.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace AceAttorney.Scripts
{
    public class PlaceHint : Singleton<PlaceHint>
    {
        private Image image;
        private RectTransform rectTransform;

        protected override void Awake()
        {
            base.Awake();

            image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();

            Hide();
        }

        private void Start()
        {
            rectTransform.sizeDelta = Vector2.one * CardManager.Instance.CardSize;
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void SetColor(Color color)
        {
            image.color = color;
        }

        public void Show()
        {
            image.enabled = true;
        }


        public void Hide()
        {
            image.enabled = false;
        }
    }
}