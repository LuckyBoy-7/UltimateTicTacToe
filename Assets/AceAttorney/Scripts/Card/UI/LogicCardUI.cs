using AceAttorney.Scripts.Card.Data;
using TMPro;

namespace AceAttorney.Scripts.Card.UI
{
    public class LogicCardUI : CardUI
    {
        public TMP_Text contentText;
        public TMP_Text descriptionText;

        public override void DisplayWithCardData(CardData cardData)
        {
            LogicCardData data = (LogicCardData)cardData;
            contentText.text = $"{data.content}";
            descriptionText.text = $"{data.description}";
        }
    }
}