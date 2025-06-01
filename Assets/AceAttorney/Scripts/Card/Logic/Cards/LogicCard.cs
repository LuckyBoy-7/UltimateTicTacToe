using System;
using System.Collections;
using AceAttorney.Scripts.Card.Data;
using AceAttorney.Scripts.Card.UI;
using DG.Tweening;
using Lucky.Kits.Interactive;
using UnityEngine;

namespace AceAttorney.Scripts.Card.Logic.Cards
{
    public class LogicCard : PlaceUsableCard
    {
        public LogicCardData Data => (LogicCardData)cardData;
        private LogicCardUI UI => (LogicCardUI)cardUI;

        private Scripts.Logic logic;

        protected override void Awake()
        {
            base.Awake();
            logic = LogicCardData.CreateLogicInstance(Data.logic);
        }

        protected TestimonyCard GetNonNeutralTestimonyCardAt(Vector3 worldPosition)
        {
            Card res = CardManager.Instance.GetUsedCardAt(worldPosition);
            if (res is TestimonyCard testimonyCard && !testimonyCard.Data.neutral)
                return testimonyCard;
            return null;
        }

        protected TestimonyCard GetNonNeutralTestimonyCardBy(Vector2 dir) => GetNonNeutralTestimonyCardAt((Vector2)transform.position + CardGap * dir);

        protected override bool UseValid() =>
            !OverlapAnyUsedCard() && TryGetTwoAdjacentNonNeutralTestimonyCards(out TestimonyCard fromTestimonyCard, out TestimonyCard toTestimonyCard);

        private bool TryGetTwoAdjacentNonNeutralTestimonyCards(out TestimonyCard fromTestimonyCard, out TestimonyCard toTestimonyCard)
        {
            fromTestimonyCard = GetNonNeutralTestimonyCardBy(Vector2.up);
            toTestimonyCard = GetNonNeutralTestimonyCardBy(Vector2.down);
            if (!fromTestimonyCard || !toTestimonyCard)
            {
                fromTestimonyCard = GetNonNeutralTestimonyCardBy(Vector2.left);
                toTestimonyCard = GetNonNeutralTestimonyCardBy(Vector2.right);
            }

            if (!fromTestimonyCard || !toTestimonyCard)
                return false;

            // 规定fromTestimonyCard离法官更近
            if (toTestimonyCard.depth < fromTestimonyCard.depth)
                (fromTestimonyCard, toTestimonyCard) = (toTestimonyCard, fromTestimonyCard);
            return true;
        }

        protected override void Use()
        {
            LogicManager.Instance.AddLogicCard(this);
            PlacedOnBoard();
            CardManager.Instance.UpdateCards();
        }

        public override void Removed()
        {
            base.Removed();
            LogicManager.Instance.RemoveLogicCard(this);
            CardManager.Instance.UpdateCards();
        }

        public void SwapTestimony()
        {
            TryGetTwoAdjacentNonNeutralTestimonyCards(out TestimonyCard fromTestimonyCard, out TestimonyCard toTestimonyCard);
            CardManager.Instance.SwapTwoTestimonyCards(fromTestimonyCard, toTestimonyCard);
        }

        public int GetTrueLogicNumber(TestimonyCard testimonyCard)
        {
            UpdateLogic();
            return logic.GetTrueLogicNumber(testimonyCard);
        }

        public int GetFalseLogicNumber(TestimonyCard testimonyCard)
        {
            UpdateLogic();
            return logic.GetFalseLogicNumber(testimonyCard);
        }

        private void UpdateLogic()
        {
            TryGetTwoAdjacentNonNeutralTestimonyCards(out TestimonyCard fromTestimonyCard, out TestimonyCard toTestimonyCard);
            logic.A = fromTestimonyCard;
            logic.B = toTestimonyCard;
        }
    }
}