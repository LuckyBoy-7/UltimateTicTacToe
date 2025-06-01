using System;
using System.Collections;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards
{
    public class PlayerCharacterCard : CharacterCard
    {
        protected CharacterCard Judge => CardManager.Instance.judge;
        protected override bool EndControlCondition => Input.GetKeyDown(KeyCode.Space) || playsLeft == 0;

        protected override void Awake()
        {
            base.Awake();
            EventManager.Instance.OnBeforeCardPlayed += BeforeCardPlayed;
        }

        private void BeforeCardPlayed(UsableCard card)
        {
            if (card.belongToCharacterCard == this)
                playsLeft -= 1;
        }


        public virtual IEnumerator StartRound()
        {
            playsLeft = 999;
            CardManager.Instance.round += 1;
            CardManager.Instance.currentRoundPlayer = this;
            yield return Judge.GetControl();

            yield return DealCardsUntilCardDeckIsFull();
            yield return GetControl();
            yield return DiscardCards();
            yield return DealCardsUntilCardDeckIsFull();
        }
    }
}