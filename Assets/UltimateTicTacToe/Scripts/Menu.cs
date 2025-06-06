using Lucky.Framework;
using UnityEngine;

namespace UltimateTicTacToe.Scripts
{
    public class Menu : ManagedBehaviour
    {
        public void Quit()
        {
            Application.Quit();
        }

        public void Restart()
        {
            Settings.Instance.startPlayerType = Settings.Instance.startPlayerType == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle;
            BoardManager.Instance.Restart();
        }
    }
}