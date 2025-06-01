using System.Collections;
using AceAttorney.Scripts.Card.Data;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards
{
    public class WitnessCard : ArrowUsableCard
    {
        public WitnessCardData Data => (WitnessCardData)cardData;

        private TestimonyCard targetTestimonyCard;

        private bool Support => lastMouseWorldPositionBeforePlayed.y > targetTestimonyCard.transform.position.y;


        protected override bool UseValid()
        {
            targetTestimonyCard = GetUsedCardAt(lastMouseWorldPositionBeforePlayed) as TestimonyCard;
            return targetTestimonyCard != null &&
                   targetTestimonyCard.CheckGetSupportOrObjectionTokenValid(Support, belongToCharacterCard);
        }

        protected override void Use()
        {
            base.Use();
            targetTestimonyCard.GetSupportOrObjectionToken(Support, belongToCharacterCard);
            PlacedOnBoard();
        }

        protected override void SelectStay()
        {
            base.SelectStay();
            targetTestimonyCard = GetUsedCardAt(lastMouseWorldPositionBeforePlayed) as TestimonyCard;
            BezierArrow.Instance.SetColor(Color.white);
            if (targetTestimonyCard != null && targetTestimonyCard.CheckGetSupportOrObjectionTokenValid(Support, belongToCharacterCard))
                BezierArrow.Instance.SetColor(Support ? CardManager.Instance.trueColor : CardManager.Instance.falseColor);
        }
    }
}