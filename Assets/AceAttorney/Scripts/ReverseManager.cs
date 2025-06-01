using System.Collections.Generic;
using System.Linq;
using AceAttorney.Scripts.Card.Logic;
using AceAttorney.Scripts.Card.Logic.Cards;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using Lucky.Kits.Managers;
using Lucky.Kits.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace AceAttorney.Scripts
{
    public class ReverseManager : Singleton<ReverseManager>
    {
        public void Reverse()
        {
            CardManager.Instance.InReverseState = true;
            foreach (var logicCard in LogicManager.Instance.GetLogicCards())
            {
                logicCard.UI.ShowSelectedHint();
                logicCard.UI.SetSelectedHintColor(Color.green);
            }
        }

        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();
            if (!CardManager.Instance.InReverseState)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                LogicCard card = CardManager.Instance.GetUsedCardAt(GameCursor.MouseWorldPosition) as LogicCard;
                if (card == null)
                    return;
                card.SwapTestimony();
                foreach (var logicCard in LogicManager.Instance.GetLogicCards())
                    logicCard.UI.HideSelectedHint();
                CardManager.Instance.InReverseState = false;
            }
        }
    }
}