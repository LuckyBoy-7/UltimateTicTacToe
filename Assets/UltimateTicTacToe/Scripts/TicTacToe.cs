using Unity.Collections;

namespace UltimateTicTacToe.Scripts
{
    public class TicTacToe
    {
        public PlayerTypes[,] board = new PlayerTypes[3, 3];

        public bool isOver;
        public PlayerTypes winner;

        public PlayerTypes this[int x, int y]
        {
            get => board[x, y];
            set => Fill(x, y, value);
        }

        public void Fill(int x, int y, PlayerTypes player)
        {
            board[x, y] = player;
        }

        public bool CheckFull()
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (this[x, y] == PlayerTypes.None)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public PlayerTypes CheckWinState()
        {
            // 横
            for (int y = 0; y < 3; y++)
            {
                if (board[0, y] != PlayerTypes.None && board[0, y] == board[1, y] && board[1, y] == board[2, y])
                {
                    return board[0, y];
                }
            }

            // 竖
            for (int x = 0; x < 3; x++)
            {
                if (board[x, 0] != PlayerTypes.None && board[x, 0] == board[x, 1] && board[x, 1] == board[x, 2])
                {
                    return board[x, 0];
                }
            }

            // 主对角线
            if (board[0, 0] != PlayerTypes.None && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
            {
                return board[0, 0];
            }

            // 副对角线
            if (board[0, 2] != PlayerTypes.None && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
            {
                return board[0, 2];
            }

            return PlayerTypes.None;
        }
    }
}