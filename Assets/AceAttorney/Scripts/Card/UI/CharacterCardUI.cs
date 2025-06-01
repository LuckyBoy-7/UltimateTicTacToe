using AceAttorney.Scripts.Card.Data;
using TMPro;
using UnityEngine.UI;

namespace AceAttorney.Scripts.Card.UI
{
    public class CharacterCardUI : CardUI
    {
        public TMP_Text nameText;
        public TMP_Text scoreText;
        public Image image;
        public Image backgroundImage;

        public override void DisplayWithCardData(CardData cardData)
        {
            CharacterCardData data = (CharacterCardData)cardData;
            nameText.text = data.name;
            image.sprite = data.characterSprite;
        }

        public void UpdateScore(int score) => scoreText.text = score.ToString();

    }
}