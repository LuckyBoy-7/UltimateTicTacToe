using AceAttorney.Scripts.Card.Data;
using AceAttorney.Scripts.Card.Logic;
using Lucky.Kits.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AceAttorney.Scripts.Card.UI
{
    public class TestimonyCardUI : CardUI
    {
        public TMP_Text nameText;
        public TMP_Text trueEffectText;
        public TMP_Text falseEffectText;
        public TMP_Text debugText;
        public Image leftBackground;
        public Image background;
        public Image upToken;
        public Image downToken;

        public override void DisplayWithCardData(CardData cardData)
        {
            TestimonyCardData data = (TestimonyCardData)cardData;
            nameText.text = "证言";
            trueEffectText.text = $"真+{data.truePoints}";
            falseEffectText.text = $"假+{data.falsePoints}";
            SetTextAndBackgroundColor(data);
            upToken.enabled = downToken.enabled = false;
        }

        private void SetTextAndBackgroundColor(TestimonyCardData data)
        {
            Color trueColor = GetColorByPointType(data.truePointFor);
            Color falseColor = GetColorByPointType(data.falsePointFor);
            background.color = trueColor;
            leftBackground.color = falseColor;
            trueEffectText.color = trueColor;
            falseEffectText.color = falseColor;
            if (data.falsePointFor == CharacterTypes.Judge)
            {
                leftBackground.enabled = false;
                return;
            }

            // 根据分数比例设置背景比例
            float ratio = (float)data.falsePoints / (data.truePoints + data.falsePoints);
            leftBackground.rectTransform.sizeDelta = leftBackground.rectTransform.sizeDelta.WithX(background.rectTransform.sizeDelta.x * ratio);
        }

        private Color GetColorByPointType(CharacterTypes type)
        {
            return type switch
            {
                CharacterTypes.Lawyer => CardManager.Instance.lawyerConfig.color,
                CharacterTypes.Procurator => CardManager.Instance.procuratorConfig.color,
                CharacterTypes.Judge => CardManager.Instance.judgeConfig.color,
                _ => Color.white
            };
        }

        public void UpdateToken(int trueTokenNumber, int falseTokenNumber, bool isFact, bool isFake)
        {
            upToken.enabled = downToken.enabled = false;
            if (isFact)
            {
                upToken.enabled = downToken.enabled = true;
                upToken.color = downToken.color = Color.white;
                return;
            }

            if (isFake)
            {
                upToken.enabled = downToken.enabled = true;
                upToken.color = downToken.color = Color.black;
                return;
            }

            // 1
            if (trueTokenNumber == 1 || falseTokenNumber == 1)
            {
                upToken.enabled = true;
                upToken.color = trueTokenNumber == 1 ? CardManager.Instance.trueColor : CardManager.Instance.falseColor;
            }
            else if (trueTokenNumber == 2 || falseTokenNumber == 2)
            {
                upToken.enabled = downToken.enabled = true;
                upToken.color = downToken.color = trueTokenNumber == 2 ? CardManager.Instance.trueColor : CardManager.Instance.falseColor;
            }
        }

        public void UpdateDebugInfo(int trueLogicNumber, int falseLogicNumber)
        {
            debugText.gameObject.SetActive(CardManager.Instance.debug);
            debugText.text = $"T: {trueLogicNumber}, F: {falseLogicNumber}";
        }
    }
}