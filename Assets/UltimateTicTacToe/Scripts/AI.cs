using System.Collections;
using Lucky.Framework;
using UltimateTicTacToe.Scripts;
using UnityEngine;

namespace UltimateTicTacToe
{
    public class AI : ManagedBehaviour
    {
        public PlayerTypes aiPlayerType = PlayerTypes.Circle;

        public Board[,] nineBoards => BoardManager.Instance.nineBoards;
        public Board bigBoard => BoardManager.Instance.bigBoard;

        public bool IsAITurn => on && BoardManager.Instance.curPlayer == aiPlayerType;
        public bool on;
        public int searchTimes = 10000;

        private int currentBoardX;
        private int currentBoardY;

        private bool thinking;
        public float thinkingTime = 1f;

        public MCTS MCTS => BoardManager.Instance.mcts;

        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (IsAITurn && !BoardManager.Instance.blockOperation && !BoardManager.Instance.gameover && !thinking)
            {
                StartCoroutine(Play());
            }
        }


        private IEnumerator Play()
        {
            thinking = true;
            yield return new WaitForSeconds(thinkingTime);
            thinking = false;

            MCTS.Selects(searchTimes, BoardManager.Instance.currentBoardX, BoardManager.Instance.currentBoardY);

            bool block = BoardManager.Instance.blockOperation;
            BoardManager.Instance.blockOperation = true;

            yield return new WaitForSeconds(0.1f);
            CellPos cellPos = MCTS.GetNextCellPos();
            yield return cellPos.GetBoard().TryFillCoroutine(cellPos.innerX, cellPos.innerY, aiPlayerType);

            BoardManager.Instance.blockOperation = block;
        }
    }
}