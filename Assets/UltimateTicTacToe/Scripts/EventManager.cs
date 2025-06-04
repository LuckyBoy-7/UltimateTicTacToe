using System;
using Lucky.Kits.Managers;

namespace UltimateTicTacToe.Scripts
{
    public class EventManager : Singleton<EventManager>
    {
        public Action<CellPos> AfterFillCell;
    }
}