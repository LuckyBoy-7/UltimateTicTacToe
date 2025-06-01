using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AceAttorney.Scripts.Card.Logic.Cards;
using Lucky.Kits.Collections;
using Lucky.Kits.Managers;

namespace AceAttorney.Scripts
{
    public abstract class Logic
    {
        public TestimonyCard A;
        public TestimonyCard B;

        public abstract int GetTrueLogicNumber(TestimonyCard testimonyCard);

        public abstract int GetFalseLogicNumber(TestimonyCard testimonyCard);
    }

    class AToBLogic : Logic
    {
        public override int GetTrueLogicNumber(TestimonyCard testimonyCard)
        {
            if (testimonyCard == B && A.RegardAsTrue)
                return 1;

            return 0;
        }

        public override int GetFalseLogicNumber(TestimonyCard testimonyCard)
        {
            // 反推
            if (testimonyCard == A && B.IsFake)
                return 1;
            return 0;
        }
    }

    class BToALogic : Logic
    {
        public override int GetTrueLogicNumber(TestimonyCard testimonyCard)
        {
            if (testimonyCard == A && B.RegardAsTrue)
                return 1;
            return 0;
        }

        public override int GetFalseLogicNumber(TestimonyCard testimonyCard)
        {
            // 反推
            if (testimonyCard == B && A.IsFake)
                return 1;
            return 0;
        }
    }

    class AToNotBLogic : Logic
    {
        public override int GetTrueLogicNumber(TestimonyCard testimonyCard)
        {
            return 0;
        }

        public override int GetFalseLogicNumber(TestimonyCard testimonyCard)
        {
            if (testimonyCard == B && A.RegardAsTrue)
                return 1;
            // 反推
            if (testimonyCard == A && B.IsFact)
                return 1;
            return 0;
        }
    }

    class BToNotALogic : Logic
    {
        public override int GetTrueLogicNumber(TestimonyCard testimonyCard)
        {
            return 0;
        }

        public override int GetFalseLogicNumber(TestimonyCard testimonyCard)
        {
            if (testimonyCard == A && B.RegardAsTrue)
                return 1;
            // 反推
            if (testimonyCard == B && A.IsFact)
                return 1;
            return 0;
        }
    }

    public class LogicManager : Singleton<LogicManager>
    {
        private List<LogicCard> logicCards = new();

        public void AddLogicCard(LogicCard logicCard) => logicCards.Add(logicCard);
        public void RemoveLogicCard(LogicCard logicCard) => logicCards.Remove(logicCard);

        public int GetTrueLogicNumber(TestimonyCard testimonyCard) => logicCards.Sum(logicCard => logicCard.GetTrueLogicNumber(testimonyCard));
        public int GetFalseLogicNumber(TestimonyCard testimonyCard) => logicCards.Sum(logicCard => logicCard.GetFalseLogicNumber(testimonyCard));

        public IEnumerable<LogicCard> GetLogicCards() => logicCards;
    }
}