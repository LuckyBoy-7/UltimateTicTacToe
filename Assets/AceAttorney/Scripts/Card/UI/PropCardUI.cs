using AceAttorney.Scripts.Card.Data;
using TMPro;
using UnityEngine.UI;

namespace AceAttorney.Scripts.Card.UI
{
    public class PropCardUI : CardUI
    {
        public Image propImage;
        public TMP_Text descriptionText;

        public override void DisplayWithCardData(CardData cardData)
        {
            PropCardData data = (PropCardData)cardData;
            propImage.sprite = data.propSprite;
            descriptionText.text = data.description;
        }
    }
}