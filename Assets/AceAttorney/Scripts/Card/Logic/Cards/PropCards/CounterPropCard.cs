using System;
using System.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards.PropCards
{
    public abstract class CounterPropCard : ConsumeUsableCard
    {
        public override bool CanUse => !played && CardManager.Instance.counterCharacter == belongToCharacterCard && !CardManager.Instance.InReverseState && !CardManager.Instance.duringUnsafeAnim;
        protected UsableCard targetCard;
        private bool Cancel => GameCursor.MouseScreenPosition.y < (float)Screen.height / 2;

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Instance.OnBeforeCardPlayed += BeforeCardPlayed;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.Instance.OnBeforeCardPlayed -= BeforeCardPlayed;
        }

        protected virtual void BeforeCardPlayed(UsableCard card)
        {
            if (played || (card is TestimonyCard && CardManager.Instance.currentRoundPlayer == belongToCharacterCard))
                return;
            // 对手打出的牌
            if (card.belongToCharacterCard != belongToCharacterCard)
            {
                targetCard = card;
                if (CardManager.Instance.counterCharacter != belongToCharacterCard)
                {
                    CardManager.Instance.counterCharacter = belongToCharacterCard;
                    card.blockUse = true;
                    FadeHint.Instance.Show();
                }

                UI.ShowSelectedHint();
                UI.SetSelectedHintColor(Color.white);
            }
        }

        protected override void SelectStay()
        {
            base.SelectStay();
            UI.SetSelectedHintColor(Cancel ? FalseColor : TrueColor);
        }

        protected override void AfterPlayed()
        {
            base.AfterPlayed();
            CardManager.Instance.counterCharacter = null;
        }


        protected override bool CheckCancelUse()
        {
            return Cancel;
        }

        public override void Thrown()
        {
            base.Thrown();
            BlockTargetCardOver();
        }

        protected override void CancelUse()
        {
            base.CancelUse();
            CardManager.Instance.counterCharacter = null;
            BlockTargetCardOver();
        }

        private void BlockTargetCardOver()
        {
            if (targetCard)
            {
                targetCard.blockUse = false;
            }

            FadeHint.Instance.Hide();
        }
    }
}