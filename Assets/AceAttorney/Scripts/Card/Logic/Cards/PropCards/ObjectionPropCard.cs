using System;
using System.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards.PropCards
{
    /// <summary>
    /// 异议: 对手打出一张手牌时使其无效
    /// </summary>
    public class ObjectionPropCard : CounterPropCard
    {
        protected override void BeforeCardPlayed(UsableCard card)
        {
            if (card is TestimonyCard && CardManager.Instance.currentRoundPlayer == belongToCharacterCard)
                return;
            base.BeforeCardPlayed(card);
        }

        protected override void Use()
        {
            base.Use();
            if (targetCard != null)
            {
                targetCard.Thrown();
                Removed();
            }

            FadeHint.Instance.Hide();
        }
    }
}