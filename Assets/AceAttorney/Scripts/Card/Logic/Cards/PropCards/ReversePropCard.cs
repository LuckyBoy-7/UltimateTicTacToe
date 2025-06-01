using System.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards.PropCards
{
    /// <summary>
    /// 逆转: 调换2张相邻证言的位置
    /// </summary>
    public class ReverseThatPropCard : ConsumeUsableCard
    {
        private TestimonyCard card0;
        private TestimonyCard card1;

        protected override void SelectStay()
        {
            base.SelectStay();
            UI.SetSelectedHintColor(UseValid() ? ValidColor : InvalidColor);
        }

        protected override bool UseValid()
        {
            return CardManager.Instance.onBoardTestimonyCards.Count >= 2;
        }

        protected override void Use()
        {
            base.Use();
            ReverseManager.Instance.Reverse();
            Destroy(gameObject);
        }
    }
}