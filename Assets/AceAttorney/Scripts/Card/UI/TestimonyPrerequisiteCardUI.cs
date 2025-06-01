using System;
using AceAttorney.Scripts.Card.Data;
using TMPro;
using UnityEngine;

namespace AceAttorney.Scripts.Card.UI
{
    public class TestimonyPrerequisiteCardUI : TestimonyCardUI
    {
        public TMP_Text descriptionText;

        public override void DisplayWithCardData(CardData cardData)
        {
            base.DisplayWithCardData(cardData);
            TestimonyCardData data = (TestimonyCardData)cardData;
            descriptionText.text = $"{data.description}";

            Color descriptionColor = data.prerequisiteType switch
            {
                TestimonyPrerequisiteTypes.ConnectedWithBlue => CardManager.Instance.lawyerConfig.color,
                TestimonyPrerequisiteTypes.ConnectedWithRed => CardManager.Instance.procuratorConfig.color,
                TestimonyPrerequisiteTypes.ConnectedWithGray => CardManager.Instance.judgeConfig.color,
                _ => throw new ArgumentOutOfRangeException()
            };
            descriptionText.color = descriptionColor;
        }
    }
}