using System;
using AceAttorney.Scripts.Card.Logic;
using Lucky.Kits.Managers;

namespace AceAttorney.Scripts
{
    public class EventManager : Singleton<EventManager>
    {
        public Action<UsableCard> OnBeforeCardPlayed;
        public Action<UsableCard> OnCardPlayed;
    }
}