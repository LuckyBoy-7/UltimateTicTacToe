using System.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards.PropCards
{
    /// <summary>
    /// 等等: 移除场上一张逻辑牌
    /// </summary>
    public class TakeThatPropCard : ArrowUsableCard
    {
        private LogicCard logicCard;

        protected override void SelectStay()
        {
            base.SelectStay();
            BezierArrow.Instance.SetColor(UseValid() ? ValidColor : InvalidColor);
        }

        protected override bool UseValid()
        {
            Card card = GetUsedCardAt(GameCursor.MouseWorldPosition);
            logicCard = card as LogicCard;
            return logicCard != null;
        }

        protected override void Use()
        {
            base.Use();
            logicCard.Removed();
            Destroy(gameObject);
        }
    }
}