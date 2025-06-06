using System;

namespace UltimateTicTacToe.Scripts
{
    public struct CellPos : IEquatable<CellPos>
    {
        public int boardX;
        public int boardY;
        public int innerX;
        public int innerY;


        public CellPos(int boardX = -1, int boardY = -1, int innerX = -1, int innerY = -1)
        {
            this.boardX = boardX;
            this.boardY = boardY;
            this.innerX = innerX;
            this.innerY = innerY;
        }

        public override string ToString()
        {
            return $"({boardX}, {boardY}), ({innerX}, {innerY})";
        }

        public PlayerTypes GetCellType()
        {
            return BoardManager.Instance.nineBoards[boardX, boardY].ticTacToe[innerX, innerY];
        }

        public void SetSmallCellType(PlayerTypes playerType)
        {
            BoardManager.Instance.nineBoards[boardX, boardY].ticTacToe[innerX, innerY] = playerType;
        }

        public void SetBigCellType(PlayerTypes playerType)
        {
            // TicTacToe ticTacToe = GetBoard().ticTacToe;
            // BoardManager.Instance.bigBoard.ticTacToe[ticTacToe.x, ticTacToe.y] = playerType;
            BoardManager.Instance.bigBoard.ticTacToe[boardX, boardY] = playerType;
        }

        public Board GetBoard()
        {
            return BoardManager.Instance.nineBoards[boardX, boardY];
        }

        public bool Equals(CellPos other)
        {
            return boardX == other.boardX && boardY == other.boardY && innerX == other.innerX && innerY == other.innerY;
        }

        public override bool Equals(object obj)
        {
            return obj is CellPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(boardX, boardY, innerX, innerY);
        }
    }

}