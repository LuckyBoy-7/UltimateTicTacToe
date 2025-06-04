using System;
using Lucky.Kits.Managers;

namespace TicTacToe
{

    public class EventManager : Singleton<EventManager>
    {
        public Action<CellPos> AfterFillCell;
    }
}