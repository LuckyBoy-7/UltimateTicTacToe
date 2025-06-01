using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AceAttorney.Scripts.Card.Data;
using DG.Tweening;
using Lucky.Framework;
using Lucky.Kits.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic
{
    public class CardDeck : ManagedBehaviour
    {
        public enum Alignments
        {
            Left,
            Right,
            Mid
        }

        public Alignments alignment;

        private List<UsableCard> cardPrefabs = new();
        [ShowInInspector] private List<UsableCard> cards = new();

        public CharacterCard belongToCharacterCard;

        public Vector2Int popupDir;
        public float popupDistance = 200;
        public const float DealInterval = 0.05f;
        public const float DiscardInterval = 0.05f;

        public void FillCardData(List<UsableCard> fillCards) => cardPrefabs.AddRange(fillCards);

        public IEnumerator DealCardsTo(int number)
        {
            int rest = number - cards.Count(card => !(card.cardData as UsableCardData)!.notOccupyCardDeckCapacity);
            if (rest <= 0)
                yield break;
            yield return DealCards(rest);
        }

        public IEnumerator DealCards(int capacity)
        {
            if (cardPrefabs.Count == 0)
            {
                Debug.LogWarning("There's no card to deal!");
                yield break;
            }

            for (int i = 0; i < capacity; i++)
            {
                DealOneCard(cardPrefabs.Choice());
                yield return new WaitForSeconds(DealInterval);
            }
        }

        public void DealOneCard(UsableCard cardPrefab)
        {
            UsableCard card = Instantiate(cardPrefab, transform);
            cards.Add(card);
            card.transform.position = transform.position;
            card.belongToCharacterCard = belongToCharacterCard;
            card.popupDistance = popupDistance;
            card.popupDir = popupDir;
            card.OnRemovedFromCardDeck += OnCardRemovedFromCardDeck;
            card.stateMachine.State = UsableCard.StDealing;
        }

        private void OnCardRemovedFromCardDeck(UsableCard card)
        {
            cards.Remove(card);
        }

        public IEnumerator DiscardCards()
        {
            foreach (var card in cards.ToList())
            {
                if (card.TryDiscard())
                    yield return new WaitForSeconds(DiscardInterval);
            }
        }


        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();

            Vector3 start = GetStartByPivotAndAlignment();
            int gap = CardManager.Instance.CardGap;
            for (int i = 0; i < cards.Count; i++)
            {
                UsableCard card = cards[i];
                card.startPosition = card.targetPosition = start + Vector3.right * (gap * i);
            }
        }

        private Vector3 GetStartByPivotAndAlignment()
        {
            int gap = CardManager.Instance.CardGap;
            float width = (cards.Count - 1) * gap;
            return transform.position + alignment switch
            {
                Alignments.Left => Vector3.zero,
                Alignments.Mid => Vector3.left * width / 2,
                Alignments.Right => Vector3.left * width,
                _ => Vector3.zero
            };
        }
    }
}