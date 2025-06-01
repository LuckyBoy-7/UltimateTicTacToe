using System;
using System.Collections;
using System.Linq;
using AceAttorney.Scripts.Card.UI;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic
{
    public enum CharacterTypes
    {
        Lawyer,
        Procurator,
        Judge
    }

    public abstract class CharacterCard : Card
    {
        public CharacterCardData Data => (CharacterCardData)cardData;
        public CharacterCardUI UI => (CharacterCardUI)cardUI;

        public CharacterCardConfig config;

        [HideInInspector] public CardDeck cardDeck;

        public int score;
        public Action OnControlOver;
        public CharacterCard switchControlToCharacter;
        public CharacterCard returnControlToCharacter;
        protected abstract bool EndControlCondition { get; }
        public int playsLeft = 999;

        public virtual void InitWithConfig(CharacterCardConfig config)
        {
            this.config = config;
            CardManager.Instance.PlaceCardAt(this, config.position);
            cardDeck = config.cardDeck;
            cardDeck.belongToCharacterCard = this;

            UI.backgroundImage.color = Color();
            UI.SetSelectedHintColor(Color());
        }

        private Color Color() => config.color;

        public void TurnToLeft()
        {
            ((CharacterCardUI)cardUI).image.transform.eulerAngles = new Vector3(0, 180, 0);
        }

        public bool IsJudge => Data.characterType == CharacterTypes.Judge;
        public bool IsLawyer => Data.characterType == CharacterTypes.Lawyer;

        public IEnumerator GetControl()
        {
            CardManager.Instance.currentInControlCharacter = this;
            // 当前牌组显示在顶层
            ShowAtTop();
            UI.ShowSelectedHint();
            yield return new WaitUntil(() => EndControlCondition || switchControlToCharacter);
            ControlOver();
            UI.HideSelectedHint();

            if (switchControlToCharacter)
            {
                CharacterCard tmpSwitchControlToCharacter = switchControlToCharacter;
                switchControlToCharacter = null;

                tmpSwitchControlToCharacter.returnControlToCharacter = this;
                yield return tmpSwitchControlToCharacter.GetControl();
            }

            if (returnControlToCharacter)
            {
                yield return returnControlToCharacter.GetControl();
                returnControlToCharacter = null;
            }
        }

        protected virtual void ControlOver()
        {
            OnControlOver?.Invoke();
        }


        public void SwitchControl(CharacterCard characterCard)
        {
            switchControlToCharacter = characterCard;
        }

        public virtual IEnumerator DealCardsUntilCardDeckIsFull()
        {
            yield return cardDeck.DealCardsTo(config.cardDeckCapacity);
        }

        public virtual IEnumerator DealCards(int number)
        {
            yield return cardDeck.DealCards(number);
        }

        public virtual IEnumerator DiscardCards()
        {
            yield return cardDeck.DiscardCards();
        }

        public void UpdateScore()
        {
            score = CardManager.Instance.onBoardTestimonyCards.Sum(card => card.GetSocreForCharacter(Data.characterType));
            UI.UpdateScore(score);
        }
    }
}