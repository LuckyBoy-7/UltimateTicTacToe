using System;
using AceAttorney.Scripts.Card.Data;
using DG.Tweening;
using Lucky.Framework;
using Lucky.Kits.Extensions;
using UnityEngine;

namespace AceAttorney.Scripts.Card.UI
{
    public class CardUI : ManagedBehaviour
    {
        public CanvasGroup canvasGroup;
        private Vector3 origScale;

        private SelectedHint selectedHint;

        private void Awake()
        {
            origScale = transform.localScale;
        }

        public virtual void DisplayWithCardData(CardData cardData)
        {
        }

        public void ScaleFromTo(float from, float to, float duration)
        {
            transform.localScale = origScale * from;
            transform.DOScale(origScale * to, duration);
        }


        public void Pulse()
        {
            ScaleFromTo(1.2f, 1, 0.1f);
        }

        public void ShowSelectedHint()
        {
            MakeSureSelectedHint();
            selectedHint.Show();
        }

        private void MakeSureSelectedHint()
        {
            if (selectedHint == null)
            {
                selectedHint = this.NewUISonWithComponent<SelectedHint>();
                selectedHint.transform.SetParent(transform);
                selectedHint.transform.localPosition = Vector3.zero;
                selectedHint.RectTransform.SetAsFirstSibling();
            }
        }

        public void HideSelectedHint()
        {
            MakeSureSelectedHint();
            selectedHint.Hide();
        }

        public void SetSelectedHintColor(Color color)
        {
            MakeSureSelectedHint();
            selectedHint.SetColor(color);
        }
    }
}