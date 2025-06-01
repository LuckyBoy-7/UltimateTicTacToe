using System;
using System.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards.PropCards
{
    /// <summary>
    /// 徽章: 更新法官所有的证言
    /// </summary>
    public class RerollPropCard : ConsumeUsableCard
    {
        public override bool CanUse => CardManager.Instance.currentRoundPlayer == belongToCharacterCard && CardManager.Instance.counterCharacter == null;

        protected override void Use()
        {
            base.Use();
            CardManager.Instance.judge.Reroll();
        }
    }
}