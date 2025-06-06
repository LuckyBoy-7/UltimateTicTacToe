using Unity.Collections;

namespace UltimateTicTacToe
{
    public class TicTacToe
    {
        public PlayerTypes[,] board = new PlayerTypes[3, 3];

        public bool isOver;
        public PlayerTypes winner;
        public int x, y;  // 相对于bigBoard的位置

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

        public float GetScore(PlayerTypes playerType)
        {
            float score = 0;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (board[x, y] == PlayerTypes.None)
                        continue;
                    // 从落子角度看, 下在角落好点, 从局面来看, 中间有子好点
                    float multiplier = 1f;
                    if (x == 1 && y == 1)
                        multiplier = 2f;
                    else if((x == 0 && y == 0) || (x == 2 && y == 2) || (x == 0 && y == 2) || (x == 2 && y == 0))
                        multiplier = 1.5f;
                    else
                        multiplier = 1f;
                    if (board[x, y] == playerType)
                        score += multiplier;
                    else
                        score -= multiplier;
                }
            }

            return score + 2;
        }
    }
}