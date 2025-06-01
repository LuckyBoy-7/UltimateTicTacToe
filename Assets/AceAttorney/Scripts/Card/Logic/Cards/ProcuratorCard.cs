using System.Collections;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards
{
    public class ProcuratorCard : PlayerCharacterCard
    {
        private void Start()
        {
            cardDeck.DealOneCard(CardManager.Instance.procuratorRetainedCard);
        }
    }
}