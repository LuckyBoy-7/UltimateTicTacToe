using Lucky.Kits.Managers;

namespace UltimateTicTacToe.Scripts
{
    [System.Serializable]
    public struct PlayerSettings
    {
        public bool isAI;
        public int searchTimes;
        public PlayerTypes playerType;
    }

    public class Settings : Singleton<Settings>
    {
        public const int AIMaxSeartchTimes = 100000;
        public PlayerSettings playerSettings0;
        public PlayerSettings playerSettings1;
        public PlayerTypes startPlayerType;
    }
}