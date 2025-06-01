using Lucky.Kits.Extensions;
using Lucky.Kits.Managers;
using Lucky.Kits.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace AceAttorney.Scripts
{
    public class FadeHint : Singleton<FadeHint>
    {
        private Image image;
        public float showAlpha = 0.6f;
        public float hideAlpha = 0;
        public float fadeSpeed = 1f;
        private bool show;

        protected override void Awake()
        {
            base.Awake();

            image = GetComponent<Image>();
        }

        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();
            
            float alpha = image.color.a;
            alpha = MathUtils.Approach(alpha, show ? showAlpha : hideAlpha, fadeSpeed * Time.deltaTime);
            image.color = image.color.WithA(alpha);
        }

        public void Show() => show = true;


        public void Hide() => show = false;
    }
}