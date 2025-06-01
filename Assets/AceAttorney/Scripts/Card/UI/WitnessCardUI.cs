using AceAttorney.Scripts.Card.Data;
using UnityEngine.UI;

namespace AceAttorney.Scripts.Card.UI
{
    public class WitnessCardUI : CardUI
    {
        public Image witnessImage;

        public override void DisplayWithCardData(CardData cardData)
        {
            WitnessCardData data = (WitnessCardData)cardData;
            witnessImage.sprite = data.witnessSprite;
            witnessImage.SetNativeSize();
        }
    }
}