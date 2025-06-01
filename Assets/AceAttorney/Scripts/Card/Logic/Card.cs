using System.Collections;
using System.Collections.Generic;
using AceAttorney.Scripts.Card.Data;
using AceAttorney.Scripts.Card.UI;
using DG.Tweening;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic
{
    public class Card : InteractableWorldUI
    {
        public CardUI UI => cardUI;
        public CardData cardData;
        protected CardUI cardUI;

        /// <summary>
        /// 从法官开始, 离法官近的depth小, 以此来判断证言离中心的的距离
        /// </summary>
        public int depth;

        public float CardGap => CardManager.Instance.CardGap;

        protected Color ValidColor => CardManager.Instance.validColor;
        protected Color InvalidColor => CardManager.Instance.invalidColor;
        protected Color TrueColor => CardManager.Instance.trueColor;
        protected Color FalseColor => CardManager.Instance.falseColor;

        private SelectedHint selectedHint;


        public Vector2Int GridPos => CardManager.Instance.GetGridPos(transform.position);

        protected virtual void Awake()
        {
            InitCardUI();
        }

        public void InitCardUI()
        {
            cardUI = Instantiate(cardData.cardUIPrefab, transform);
            cardUI.transform.position = transform.position;
            cardUI.DisplayWithCardData(cardData);
            RectTransform.sizeDelta = Vector2.one * CardManager.Instance.CardSize;
        }

        protected Card GetUsedCardAt(Vector3 worldPosition) => CardManager.Instance.GetUsedCardAt(worldPosition);


        protected bool OverlapAnyUsedCard() => GetUsedCardAt(transform.position);

        public void PlacedToGridCenter() =>  CardManager.Instance.PlaceCardAt(this, GridPos);


        public IEnumerable<Card> GetUsedCardsAround(Vector2 worldPosition, int dist = 1)
        {
            foreach (var (dx, dy) in new List<(int, int)> { (1, 0), (-1, 0), (0, 1), (0, -1) })
            {
                var (nx, ny) = (worldPosition.x + dx * dist * CardGap, worldPosition.y + dy * dist * CardGap);
                Card card = GetUsedCardAt(new Vector3(nx, ny));
                if (card != null)
                    yield return card;
            }
        }


        protected void ShowAtTop()
        {
            // 当前手牌显示在顶层
            RectTransform.SetAsLastSibling();
        }
    }
}