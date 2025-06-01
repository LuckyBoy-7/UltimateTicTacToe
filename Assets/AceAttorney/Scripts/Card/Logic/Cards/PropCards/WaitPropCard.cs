using System;
using System.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards.PropCards
{
    /// <summary>
    /// 等等: 对手回合打出一张证言或手牌时, 你可立即出一张牌
    /// </summary>
    public class WaitPropCard : CounterPropCard
    {
        private int playsLeftBackup;
        // private CharacterCard preInControlCharacter;
        protected override void Use()
        {
            base.Use();
            // preInControlCharacter = CardManager.Instance.currentInControlCharacter;
            CardManager.Instance.currentInControlCharacter.SwitchControl(belongToCharacterCard);
            playsLeftBackup = belongToCharacterCard.playsLeft;
            belongToCharacterCard.OnControlOver += OnPlayerControlOver;
            belongToCharacterCard.playsLeft = 1;
            FadeHint.Instance.Hide();
            // StartCoroutine(belongToCharacterCard.GetControl());
            Removed();
        }

        private void OnPlayerControlOver()
        {
            // StartCoroutine(preInControlCharacter.GetControl());
            if (targetCard != null)
            {
                // targetCard.Thrown();
                targetCard.blockUse = false;
            }

            belongToCharacterCard.playsLeft = playsLeftBackup;
            belongToCharacterCard.OnControlOver -= OnPlayerControlOver;
        }
    }
}