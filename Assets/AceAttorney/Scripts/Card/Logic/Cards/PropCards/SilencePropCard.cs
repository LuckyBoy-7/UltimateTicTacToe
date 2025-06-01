using System.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards.PropCards
{
    /// <summary>
    /// 肃静: 移除1张没有证人支持或反对的末端证言
    /// </summary>
    public class SilencePropCard : ArrowUsableCard
    {
        private TestimonyCard testimonyCard;

        protected override void SelectStay()
        {
            base.SelectStay();
            BezierArrow.Instance.SetColor(UseValid() ? ValidColor : InvalidColor);
        }

        protected override bool UseValid()
        {
            Card card = GetUsedCardAt(GameCursor.MouseWorldPosition);
            if (card is TestimonyCard { isLeaf: true, TrueNumber: 0, FalseNumber: 0 } target)
            {
                testimonyCard = target;
                return true;
            }

            return false;
        }

        protected override void Use()
        {
            base.Use();
            testimonyCard.Removed();
            Destroy(gameObject);
        }
    }
}