using System;
using System.Collections.Generic;
using System.Linq;
using Lucky.Kits.Extensions;
using Lucky.Kits.Utilities;
using UnityEngine;

namespace UltimateTicTacToe.Scripts
{
    public class MCTS
    {
        public List<MCTS> children = new();

        // public Dictionary<CellPos, MCTS> cellPosToChildren = new();
        private Board[,] nineBoards;
        private Board bigBoard;
        public PlayerTypes playerType;

        public float totalValue;
        public float averageValue => totalValue / visitedCount;
        public int visitedCount;

        public CellPos preCellPos;
        public MCTS parent;
        public bool terminated;
        public PlayerTypes terminatedWinner;
        public float terminatedScore;

        public MCTS(Board[,] nineBoards, Board bigBoard, PlayerTypes playerType, CellPos lastCellPos)
        {
            this.nineBoards = nineBoards;
            this.bigBoard = bigBoard;
            this.playerType = playerType;
            this.preCellPos = lastCellPos;
        }

        public override string ToString()
        {
            return $"UCB: {GetUCB()}, totalValue: {totalValue}, visitedCount: {visitedCount},  playerType: {playerType}, cellPos: {preCellPos}";
        }

        private PlayerTypes GetOppositePlayerType(PlayerTypes type)
        {
            return type == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle;
        }

        private void Expansion(int currentBoardX, int currentBoardY)
        {
            foreach (CellPos cellPos in GetPosesCanChoose(currentBoardX, currentBoardY))
            {
                FillAndRestoreCellPos(playerType, cellPos, () =>
                {
                    MCTS mcts = new MCTS(nineBoards, bigBoard, GetOppositePlayerType(playerType), cellPos)
                    {
                        parent = this
                    };
                    children.Add(mcts);
                });
            }
        }


        public void Selects(int simulateTimes, int currentBoardX, int currentBoardY)
        {
            for (int c = 1; c <= simulateTimes; c++)
            {
                Select(currentBoardX, currentBoardY, simulateTimes - c);
            }
        }

        public MCTS GetNextMCTS(CellPos cellPos)
        {
            MCTS child = null;
            if (children.Count == 0)
            {
                child = new MCTS(nineBoards, bigBoard, GetOppositePlayerType(playerType), cellPos)
                {
                    parent = this
                };
            }
            else
            {
                child = children.Find(c => c.preCellPos.Equals(cellPos));
            }

            child.parent = null;
            return child;
        }

        private static int count = 0;

        private void Select(int currentBoardX, int currentBoardY, int leftSelectCount)
        {
            if (terminated)
            {
                Backpropagation(terminatedWinner, terminatedScore);
                return;
            }

            if (children.Count == 0)
            {
                var poses = GetPosesCanChoose(currentBoardX, currentBoardY).GetEnumerator();
                if (!poses.MoveNext()) // 无法扩展子节点
                {
                    terminated = true;
                    terminatedWinner = bigBoard.ticTacToe.CheckWinState();
                    if (terminatedWinner != PlayerTypes.None)
                        terminatedScore = bigBoard.ticTacToe.GetScore(terminatedWinner);

                    if (count++ < 1000)
                        Debug.Log(terminatedWinner);
                    // Simulate();
                    Backpropagation(terminatedWinner, terminatedScore);
                    poses.Dispose();
                    return;
                }

                poses.Dispose();

                Expansion(currentBoardX, currentBoardY);
                MCTS child = children.Choice();
                child.Simulate();
                return;
            }


            // 找到目前相对最优的子节点
            MCTS bestChild = null;
            float bestUCB = int.MinValue;

            MCTS visitBestChild = null;
            int maxVisitedCount = int.MinValue;
            int sumVisitedCount = 0;
            foreach (MCTS child in children)
            {
                if (child.visitedCount == 0)
                {
                    bestChild = child;
                    break;
                }

                float UCB = child.GetUCB();

                if (UCB > bestUCB)
                {
                    bestUCB = UCB;
                    bestChild = child;
                }

                sumVisitedCount += child.visitedCount;
                if (child.visitedCount > maxVisitedCount)
                {
                    visitBestChild = child;
                    maxVisitedCount = child.visitedCount;
                }
            }

            if (sumVisitedCount - maxVisitedCount + leftSelectCount < maxVisitedCount)
            {
                bestChild = visitBestChild;
            }


            FillAndRestoreCellPos(playerType, bestChild!.preCellPos, () => { bestChild!.Select(bestChild.preCellPos.innerX, bestChild.preCellPos.innerY, leftSelectCount); });
        }

        private float GetUCB()
        {
            if (parent == null)
                return 0;

            float c = 1 - Ease.SineEaseIn(BoardManager.Instance.playedCharNumber / 200f);
            return (float)(averageValue + c * BoardManager.Instance.c * Math.Sqrt(Math.Log(parent.visitedCount) / visitedCount));
        }

        private void FillAndRestoreCellPos(PlayerTypes player, CellPos cellPos, Action callback)
        {
            Board board = cellPos.GetBoard();
            cellPos.SetSmallCellType(player);
            PlayerTypes winState = board.ticTacToe.CheckWinState();
            // 当前盘分出胜负
            if (winState is PlayerTypes.Circle or PlayerTypes.Cross)
            {
                board.ticTacToe.isOver = true;
                cellPos.SetBigCellType(winState);
                callback?.Invoke();
                cellPos.SetBigCellType(PlayerTypes.None);
                cellPos.SetSmallCellType(PlayerTypes.None);
                board.ticTacToe.isOver = false;
                return;
            }

            callback?.Invoke();
            board.ticTacToe.isOver = false;
            cellPos.SetSmallCellType(PlayerTypes.None);
        }

        private (PlayerTypes, float) RandomFillUntilEnd(PlayerTypes curPlayer, int currentBoardX, int currentBoardY)
        {
            // 找到能用的空位置
            List<CellPos> posesCanChoose = GetPosesCanChoose(currentBoardX, currentBoardY).ToList();
            if (posesCanChoose.Count == 0)
            {
                // int score = 0;
                // for (int x = 0; x < 3; x++)
                // {
                //     for (int y = 0; y < 3; y++)
                //     {
                //         var w = bigBoard.ticTacToe[x, y];
                //         if (w == PlayerTypes.None)
                //             continue;
                //         if (w == curPlayer)
                //             score += 1;
                //         else
                //             score -= 1;
                //     }
                // }

                // 哪怕是平局可能也是比较差的局面的平局, 但是贡献不能像胜利或失败整那么大
                return (PlayerTypes.None, bigBoard.ticTacToe.GetScore(curPlayer) / 2);
                // return (PlayerTypes.None, (score * 0.5f));
            }

            // 抽一个位置
            int chosenIndex = UnityEngine.Random.Range(0, posesCanChoose.Count);
            CellPos chosenPos = posesCanChoose[chosenIndex];

            // 填入符号
            Board board = chosenPos.GetBoard();
            chosenPos.SetSmallCellType(curPlayer);
            PlayerTypes winState = board.ticTacToe.CheckWinState();
            // 当前盘分出胜负
            if (winState is PlayerTypes.Circle or PlayerTypes.Cross)
            {
                board.ticTacToe.isOver = true;
                bigBoard.ticTacToe[board.ticTacToe.x, board.ticTacToe.y] = winState;
                PlayerTypes bigTicTacToeWinState = board.ticTacToe.CheckWinState();
                // 整个盘分出胜负
                if (bigTicTacToeWinState is PlayerTypes.Circle or PlayerTypes.Cross)
                {
                    // 恢复现场
                    chosenPos.SetSmallCellType(PlayerTypes.None);
                    board.ticTacToe.isOver = false;
                    bigBoard.ticTacToe[board.ticTacToe.x, board.ticTacToe.y] = PlayerTypes.None;
                    bigBoard.ticTacToe.isOver = false;
                    // int score = 0;
                    // for (int x = 0; x < 3; x++)
                    // {
                    //     for (int y = 0; y < 3; y++)
                    //     {
                    //         var w = bigBoard.ticTacToe[x, y];
                    //         if (w == PlayerTypes.None)
                    //             continue;
                    //         if (w == curPlayer)
                    //             score += 1;
                    //         else
                    //             score -= 1;
                    //     }
                    // }

                    return (bigTicTacToeWinState, bigBoard.ticTacToe.GetScore(bigTicTacToeWinState));
                }
            }

            currentBoardX = chosenPos.innerX; // 这个不用恢复现场, 到时候外面会重新赋值的
            currentBoardY = chosenPos.innerY;
            var (winner, s) = RandomFillUntilEnd(GetOppositePlayerType(curPlayer), currentBoardX, currentBoardY);
            // 恢复现场
            board.ticTacToe.isOver = false;
            chosenPos.SetSmallCellType(PlayerTypes.None);
            bigBoard.ticTacToe[board.ticTacToe.x, board.ticTacToe.y] = PlayerTypes.None;

            return (winner, s);
        }

        public void Simulate()
        {
            PlayerTypes ans = PlayerTypes.None;
            float score = 0;
            FillAndRestoreCellPos(GetOppositePlayerType(playerType), preCellPos, () => { (ans, score) = RandomFillUntilEnd(playerType, preCellPos.innerX, preCellPos.innerY); });
            Backpropagation(ans, score);
        }

        public void Backpropagation(PlayerTypes ans, float score)
        {
            visitedCount++;
            totalValue += ans == PlayerTypes.None ? 0 : ans == playerType ? -score : score;
            // totalValue += ans == playerType ? 1 : 0;
            if (parent == null)
                return;
            parent.Backpropagation(ans, score);
        }


        public CellPos GetNextCellPos()
        {
            MCTS bestChild = null;
            int bestVisCount = int.MinValue;
            foreach (MCTS child in children)
            {
                if (child.visitedCount > bestVisCount)
                {
                    bestVisCount = child.visitedCount;
                    bestChild = child;
                }
            }
            
            return bestChild!.preCellPos;
        }

        public IEnumerable<CellPos> GetPosesCanChoose(int currentBoardX, int currentBoardY) => BoardManager.Instance.GetPosesCanChoose(currentBoardX, currentBoardY);
    }
}