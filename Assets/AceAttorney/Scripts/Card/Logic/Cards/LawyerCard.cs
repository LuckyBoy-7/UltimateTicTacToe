using System;
using System.Collections;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards
{
    public class LawyerCard : PlayerCharacterCard
    {
        private void Start()
        {
            cardDeck.DealOneCard(CardManager.Instance.lawyerRetainedCard);
        }
    }
}