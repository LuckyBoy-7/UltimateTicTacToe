using Lucky.Framework;
using Lucky.Kits.Extensions;
using Lucky.Kits.Managers;
using Lucky.Kits.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AceAttorney.Scripts
{
    public class SelectedHint : ManagedUIBehaviour
    {
        public int bgNumber = 2;
        public Color color = Color.green;
        public float gap = 5;
        public Image prefab;
        public float maxAlpha = 0.8f;
        public float minAlpha = 0;
        public float fadeDuration = 0.2f;

        private Image[] images;

        protected void Awake()
        {
            bgNumber = 3;
            color = Color.green;
            gap = 12;
            prefab = Resources.Load<Image>("SelectedBg");
            maxAlpha = 0.8f;
            minAlpha = 0;
            fadeDuration = 0.5f;
            images = new Image[bgNumber];
            
            for (int i = bgNumber - 1; i >= 0; i--)
            {
                Image bg = Instantiate(prefab, transform);
                images[i] = bg;

                bg.transform.position = transform.position;
                bg.color = color;
            }

            Hide();
        }

        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();
            AdjustFade(Timer.GetTime());
        }

        public void AdjustFade(float time)
        {
            // [0, 1]
            float increPercent = time % fadeDuration / fadeDuration;
            float decrePercent = 1 - increPercent;
            // i对应内圈, 拥有较大的alpha, 所以后面还要用j取反
            for (int i = 0; i < bgNumber; i++)
            {
                int j = bgNumber - 1 - i;
                float curMinAlpha = (maxAlpha - minAlpha) / bgNumber * j + minAlpha;
                float curMaxAlpha = (maxAlpha - minAlpha) / bgNumber * (j + 1) + minAlpha;
                images[i].color = images[i].color.WithA(MathUtils.Lerp(curMinAlpha, curMaxAlpha, decrePercent));
                
                float curMinGap = gap * i;
                float curMaxGap = gap * (i + 1);
                images[i].rectTransform.sizeDelta = (CardManager.Instance.CardSize + MathUtils.Lerp(curMinGap, curMaxGap, increPercent)) * Vector2.one;
            }
        }


        public void Show()
        {
            gameObject.SetActive(true);
        }


        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetColor(Color c)
        {
            foreach (var image in images)
            {
                image.color = c;
            }
        }
    }
}