using System.Collections;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards
{
    public class JudgeCard : CharacterCard
    {
        public bool roundOverTrigger;

        protected override bool EndControlCondition => roundOverTrigger;

        public override void InitWithConfig(CharacterCardConfig config)
        {
            base.InitWithConfig(config);
            UI.scoreText.enabled = false;
            EventManager.Instance.OnCardPlayed += OnCardPlayed;
        }

        private void OnCardPlayed(UsableCard usableCard)
        {
            if (usableCard is not TestimonyCard card)
                return;
            StartCoroutine(DealCards(1));
            if (card.Data.neutral)
                return;
            roundOverTrigger = true;
        }

        protected override void ControlOver()
        {
            base.ControlOver();
            roundOverTrigger = false;
        }

        public void Reroll()
        {
            StartCoroutine(RerollCoroutine());
        }

        public IEnumerator RerollCoroutine()
        {
            yield return CardManager.Instance.judge.DiscardCards();
            yield return CardManager.Instance.judge.DealCardsUntilCardDeckIsFull();
        }
    }
}