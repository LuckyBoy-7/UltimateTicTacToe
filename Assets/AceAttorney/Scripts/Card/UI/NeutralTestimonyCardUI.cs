using AceAttorney.Scripts.Card.Data;
using TMPro;
using UnityEngine.UI;

namespace AceAttorney.Scripts.Card.UI
{
    public class NeutralTestimonyCardUI : CardUI
    {
        public TMP_Text nameText;
        public TMP_Text descriptionText;
        public Image background;

        public override void DisplayWithCardData(CardData cardData)
        {
            TestimonyCardData data = (TestimonyCardData)cardData;
            nameText.text = "证言";
            descriptionText.text = $"{data.description}";
            background.color = CardManager.Instance.judgeConfig.color;
        }
    }
}