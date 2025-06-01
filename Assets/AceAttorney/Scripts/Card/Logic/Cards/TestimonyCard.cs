using System.Collections;
using AceAttorney.Scripts.Card.Data;
using AceAttorney.Scripts.Card.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards
{
    public class TestimonyCard : PlaceUsableCard
    {
        public TestimonyCardData Data => (TestimonyCardData)cardData;
        public TestimonyCardUI UI => (TestimonyCardUI)cardUI;

        public enum TestimonyColorTypes
        {
            Gray,
            Blue,
            Red,
            Mix
        }

        [ShowInInspector]
        public TestimonyColorTypes TestimonyColorType
        {
            get
            {
                bool anyForLawyer = Data.truePointFor == CharacterTypes.Lawyer || Data.falsePointFor == CharacterTypes.Lawyer;
                bool anyForProcurator = Data.truePointFor == CharacterTypes.Procurator || Data.falsePointFor == CharacterTypes.Procurator;
                if (anyForLawyer && anyForProcurator)
                    return TestimonyColorTypes.Mix;
                if (anyForLawyer)
                    return TestimonyColorTypes.Blue;
                if (anyForProcurator)
                    return TestimonyColorTypes.Red;
                return TestimonyColorTypes.Gray;
            }
        }

        // [ShowInInspector] public bool IsFact => trueTokenNumber == 2 && FalseLogicNumber == 0; // 只要这个证言存伪就不能当作事实
        // [ShowInInspector] public bool IsFake => falseTokenNumber == 2 && TrueLogicNumber == 0; // 只要这个证言存真就不能当作谎言
        [ShowInInspector] public bool IsFact { get; set; }
        [ShowInInspector] public bool IsFake { get; set; }
        [ShowInInspector] public int TrueNumber => trueTokenNumber + TrueLogicNumber;
        [ShowInInspector] public int FalseNumber => falseTokenNumber + FalseLogicNumber;
        [ShowInInspector] public bool RegardAsTrue => IsFact || TrueNumber > FalseNumber;
        [ShowInInspector] public bool RegardAsFalse => IsFake || TrueNumber < FalseNumber;

        private int trueTokenNumber;
        private int falseTokenNumber;

        [ShowInInspector] private int TrueLogicNumber => LogicManager.Instance.GetTrueLogicNumber(this);
        [ShowInInspector] private int FalseLogicNumber => LogicManager.Instance.GetFalseLogicNumber(this);

        private int lastGetTokenRound = -1;
        private CharacterCard lastGetTokenCharacter;

        public bool isLeaf;

        private Card adjacentValidCard;

        public void UpdateCard()
        {
            if (Data.neutral)
                return;
            IsFact |= trueTokenNumber == 2 && FalseLogicNumber == 0; // 只要这个证言存伪就不能当作事实
            IsFake |= falseTokenNumber == 2 && TrueLogicNumber == 0; // 只要这个证言存真就不能当作谎言
            UI.UpdateToken(trueTokenNumber, falseTokenNumber, IsFact, IsFake);
            CardManager.Instance.UpdateCharacterCardScore();
        }

        protected override bool UseValid() => HasOneJudgeOrTestimonyAround(out adjacentValidCard) && CardFitsPrerequisite(adjacentValidCard) && !OverlapAnyUsedCard();

        protected override void Use()
        {
            depth = adjacentValidCard.depth + 1;
            if (adjacentValidCard is TestimonyCard testimonyCard)
                testimonyCard.isLeaf = false;
            isLeaf = true;
            PlacedOnBoard();
        }

        protected override void PlacedOnBoard()
        {
            base.PlacedOnBoard();
            CardManager.Instance.onBoardTestimonyCards.Add(this);
        }

        private bool CardFitsPrerequisite(Card target)
        {
            if (Data.prerequisiteType == TestimonyPrerequisiteTypes.None)
                return true;
            if (target is JudgeCard)
                return true;
            TestimonyCard card = (TestimonyCard)target;
            if (Data.prerequisiteType == TestimonyPrerequisiteTypes.ConnectedWithBlue)
                return card.TestimonyColorType == TestimonyColorTypes.Blue;
            if (Data.prerequisiteType == TestimonyPrerequisiteTypes.ConnectedWithRed)
                return card.TestimonyColorType == TestimonyColorTypes.Red;
            if (Data.prerequisiteType == TestimonyPrerequisiteTypes.ConnectedWithGray)
                return card.TestimonyColorType == TestimonyColorTypes.Gray;
            return false;
        }

        private bool HasOneJudgeOrTestimonyAround(out Card target)
        {
            target = null;
            int count = 0;
            foreach (Card card in GetUsedCardsAround(transform.position, 2))
            {
                if (card is CharacterCard { IsJudge: true } || card is TestimonyCard { played: true })
                {
                    target = card;
                    if (++count > 1)
                        return false;
                }
            }

            return count == 1;
        }

        protected override void ManagedFixedUpdate()
        {
            base.ManagedFixedUpdate();


            // debug
            if (cardUI is TestimonyCardUI testimonyCardUI)
            {
                testimonyCardUI.UpdateDebugInfo(TrueLogicNumber, FalseLogicNumber);
            }
        }

        public void GetSupportOrObjectionToken(bool support, CharacterCard fromCharacter)
        {
            if (!CheckGetSupportOrObjectionTokenValid(support, fromCharacter))
                return;

            if (support)
            {
                if (falseTokenNumber > 0)
                {
                    falseTokenNumber -= 1;
                }
                else if (trueTokenNumber < 2)
                {
                    trueTokenNumber += 1;
                }
            }
            else
            {
                if (trueTokenNumber > 0)
                {
                    trueTokenNumber -= 1;
                }
                else if (falseTokenNumber < 2)
                {
                    falseTokenNumber += 1;
                }
            }

            lastGetTokenRound = CardManager.Instance.round;
            lastGetTokenCharacter = fromCharacter;
            CardManager.Instance.UpdateCards();
        }

        public bool CheckGetSupportOrObjectionTokenValid(bool support, CharacterCard fromCharacter)
        {
            // 中立证言不接受任何证据
            if (Data.neutral)
                return false;
            // 被证实或证伪也不接受任何证据
            if (IsFact || IsFake)
                return false;
            // 同一回合一个证言牌只接受一个证人
            if (lastGetTokenRound == CardManager.Instance.round && lastGetTokenCharacter == fromCharacter)
                return false;

            bool valid = true;
            if (support)
            {
                if (falseTokenNumber <= 0 && trueTokenNumber >= 2)
                    valid = false;
            }
            else
            {
                if (trueTokenNumber <= 0 && falseTokenNumber >= 2)
                    valid = false;
            }

            return valid;
        }

        public int GetSocreForCharacter(CharacterTypes characterType)
        {
            if (TrueNumber > FalseNumber && Data.truePointFor == characterType)
                return Data.truePoints;
            if (TrueNumber < FalseNumber && Data.falsePointFor == characterType)
                return Data.falsePoints;
            return 0;
        }

        public override void Removed()
        {
            base.Removed();
            CardManager.Instance.onBoardTestimonyCards.Remove(this);
            Destroy(gameObject);
        }
    }
}