using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using AceAttorney.Scripts;
using Lucky.Framework;
using Lucky.Kits.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using Lucky.Kits.Managers;
using Lucky.Kits.Utilities;
using Sirenix.OdinInspector;
using UltimateTicTacToe;
using UltimateTicTacToe.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using EventManager = UltimateTicTacToe.Scripts.EventManager;
using Random = System.Random;

namespace UltimateTicTacToe
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

    public class MCTS
    {
        public List<MCTS> children = new();

        // public Dictionary<CellPos, MCTS> cellPosToChildren = new();
        private Board[,] nineBoards;
        private Board bigBoard;
        public PlayerTypes playerType;

        public int totalValue;
        public float averageValue => (float)totalValue / visitedCount;
        public int visitedCount;

        public CellPos preCellPos;
        public MCTS parent;
        public bool terminated;
        public PlayerTypes terminatedWinner;

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
                Backpropagation(terminatedWinner);
                return;
            }

            if (children.Count == 0)
            {
                var poses = GetPosesCanChoose(currentBoardX, currentBoardY).GetEnumerator();
                if (!poses.MoveNext()) // 无法扩展子节点
                {
                    terminated = true;
                    terminatedWinner = bigBoard.ticTacToe.CheckWinState();

                    if (count++ < 1000)
                        Debug.Log(terminatedWinner);
                    // Simulate();
                    Backpropagation(terminatedWinner);
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

            // if (count++ % 1000 == 0)
            // Debug.Log(bestChild);

            FillAndRestoreCellPos(playerType, bestChild!.preCellPos, () => { bestChild!.Select(bestChild.preCellPos.innerX, bestChild.preCellPos.innerY, leftSelectCount); });
            // FillAndRestoreCellPos(playerType, bestChild!.preCellPos, () => { bestChild!.Select(bestChild.preCellPos.innerX, bestChild.preCellPos.innerY); });
            // depth = bestChild.depth + 1;
            // totalValue += lastChild.playerType == playerType ? lastChild.totalValue : -lastChild.totalValue;
        }

        private float GetUCB()
        {
            if (parent == null)
                return 0;
            // return (float)(averageValue + AI.Instance.c * Math.Sqrt(Math.Log(AI.Instance.mcts.visitedCount) / visitedCount));
            // return (float)(averageValue + AI.Instance.c * Math.Sqrt(Math.Log(AI.Instance.totalVisitedCount) / visitedCount));
            // float c = Ease.CubicEaseOut((float)AI.Instance.playedCharNumber / 81);
            float c = 1 - Ease.CubicEaseOutIn((float)AI.Instance.playedCharNumber / 81);
            return (float)(averageValue + c * AI.Instance.c * Math.Sqrt(Math.Log(parent.visitedCount) / visitedCount));
            // return (float)(averageValue + AI.Instance.c * Math.Sqrt(Math.Log(parent.visitedCount) / visitedCount));
            // return (float)(averageValue + AI.Instance.c * Math.Sqrt(Math.Log(parent.visitedCount) / visitedCount));
        }

        private void FillAndRestoreCellPos(PlayerTypes playerType, CellPos cellPos, Action callback)
        {
            Board board = cellPos.GetBoard();
            cellPos.SetSmallCellType(playerType);
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

        private PlayerTypes RandomFillUntilEnd(PlayerTypes curPlayer, int currentBoardX, int currentBoardY)
        {
            // 找到能用的空位置
            List<CellPos> posesCanChoose = GetPosesCanChoose(currentBoardX, currentBoardY).ToList();
            if (posesCanChoose.Count == 0)
                return PlayerTypes.None;

            // 抽一个位置
            int chosenIndex = UnityEngine.Random.Range(0, posesCanChoose.Count);
            CellPos chosenPos = posesCanChoose[chosenIndex];

            // foreach (CellPos cellPos in posesCanChoose)
            // {
            //     if (nineBoards[cellPos.innerX, cellPos.innerY].ticTacToe.isOver || cellPos.innerX == 1 && cellPos.innerY == 1)
            //     {
            //         if (cellPos.Equals(chosenPos))
            //             if (RandomUtils.NextFloat() < AI.Instance.probabilityToChooseDanger)
            //                 return PlayerTypes.None;
            //     }
            // }

            // List<CellPos> dangerPoses = new();
            // List<CellPos> normalPoses = new();
            // foreach (CellPos cellPos in posesCanChoose)
            // {
            //     if (nineBoards[cellPos.innerX, cellPos.innerY].ticTacToe.isOver || cellPos.innerX == 1 && cellPos.innerY == 1)
            //     {
            //         dangerPoses.Add(cellPos);
            //     }
            //     else
            //     {
            //         normalPoses.Add(cellPos);
            //     }
            // }
            //
            // CellPos chosenPos = new();
            // if (normalPoses.Count > 0)
            // {
            //     if (dangerPoses.Count > 0 && RandomUtils.NextFloat() < AI.Instance.probabilityToChooseDanger)
            //     {
            //         // chosenPos = dangerPoses[UnityEngine.Random.Range(0, dangerPoses.Count)];
            //         chosenPos = dangerPoses.Choice();
            //     }
            //     else
            //     {
            //         chosenPos = normalPoses.Choice();
            //         // chosenPos = normalPoses[UnityEngine.Random.Range(0, normalPoses.Count)];
            //     }
            // }
            // else
            // {
            //     chosenPos = dangerPoses.Choice();
            //     // chosenPos = dangerPoses[UnityEngine.Random.Range(0, dangerPoses.Count)];
            // }

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
                    return bigTicTacToeWinState;
                }
            }

            currentBoardX = chosenPos.innerX; // 这个不用恢复现场, 到时候外面会重新赋值的
            currentBoardY = chosenPos.innerY;
            var winner = RandomFillUntilEnd(GetOppositePlayerType(curPlayer), currentBoardX, currentBoardY);
            // 恢复现场
            board.ticTacToe.isOver = false;
            chosenPos.SetSmallCellType(PlayerTypes.None);
            bigBoard.ticTacToe[board.ticTacToe.x, board.ticTacToe.y] = PlayerTypes.None;

            return winner;
        }

        public void Simulate()
        {
            PlayerTypes ans = PlayerTypes.None;
            FillAndRestoreCellPos(GetOppositePlayerType(playerType), preCellPos, () => { ans = RandomFillUntilEnd(playerType, preCellPos.innerX, preCellPos.innerY); });
            Backpropagation(ans);
        }

        public void Backpropagation(PlayerTypes ans)
        {
            visitedCount++;
            totalValue += ans == PlayerTypes.None ? 0 : ans == playerType ? -1 : 1;
            // totalValue += ans == playerType ? 1 : 0;
            if (parent == null)
                return;
            parent.Backpropagation(ans);
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


        private IEnumerable<CellPos> GetPosesCanChoose(int currentBoardX, int currentBoardY)
        {
            if (!nineBoards[currentBoardX, currentBoardY].ticTacToe.isOver)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        if (nineBoards[currentBoardX, currentBoardY].ticTacToe[x, y] == PlayerTypes.None)
                            yield return new CellPos(currentBoardX, currentBoardY, x, y);
                    }
                }
            }
            else
            {
                for (int boardX0 = 0; boardX0 < 3; boardX0++)
                {
                    for (int boardY0 = 0; boardY0 < 3; boardY0++)
                    {
                        if (nineBoards[boardX0, boardY0].ticTacToe.isOver)
                            continue;
                        for (int innerX0 = 0; innerX0 < 3; innerX0++)
                        {
                            for (int innerY0 = 0; innerY0 < 3; innerY0++)
                            {
                                PlayerTypes curPlayerType = nineBoards[boardX0, boardY0].ticTacToe[innerX0, innerY0];
                                if (curPlayerType == PlayerTypes.None)
                                    yield return new(boardX0, boardY0, innerX0, innerY0);
                            }
                        }
                    }
                }
            }
        }
    }

    public class AI : Singleton<AI>
    {
        public PlayerTypes aiPlayerType = PlayerTypes.Circle;

        public Board[,] nineBoards => BoardManager.Instance.nineBoards;
        public Board bigBoard => BoardManager.Instance.bigBoard;

        public bool IsAITurn => BoardManager.Instance.useAI && BoardManager.Instance.curPlayer == aiPlayerType;
        public int searchTimes = 10000;

        private int currentBoardX;
        private int currentBoardY;

        private bool thinking;
        public float thinkingTime = 1f;

        public MCTS mcts;
        public float c = 1.1f;
        public int playedCharNumber;
        [Range(0, 1)] public float probabilityToChooseDanger = 0.1f;


        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (IsAITurn && !BoardManager.Instance.blockOperation && !BoardManager.Instance.gameover && !thinking)
            {
                StartCoroutine(Play());
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            EventManager.Instance.AfterFillCell += AfterFillCell;
            mcts = new MCTS(nineBoards, bigBoard, BoardManager.Instance.curPlayer, new CellPos());
        }

        private void OnDisable()
        {
            EventManager.Instance.AfterFillCell -= AfterFillCell;
        }

        private void AfterFillCell(CellPos cellPos)
        {
            if (BoardManager.Instance.gameover)
                return;
            mcts = mcts.GetNextMCTS(cellPos);
            playedCharNumber += 1;
        }

        private IEnumerator Play()
        {
            thinking = true;
            yield return new WaitForSeconds(thinkingTime);
            thinking = false;

            // MCTS mcts = new MCTS(nineBoards, bigBoard, aiPlayerType, new CellPos());
            print(mcts);
            mcts.Selects(searchTimes, BoardManager.Instance.currentBoardX, BoardManager.Instance.currentBoardY);
            print(mcts.totalValue);


            Debug.Log("====================================");
            foreach (MCTS child in mcts.children)
            {
                Debug.Log(child);
            }

            // foreach (var mctsChild in mcts.children)
            // {
            //     // print(mctsChild.averageValue);
            //     print(mctsChild.visitedCount);
            // }

            // 随机搜索若干次, 记录选某个位置胜利的次数
            // DefaultDict<CellPos, int[]> posToWinCount = new DefaultDict<CellPos, int[]>(() => new int[2] { 0, 0 });

            //
            // // if (posToWinCount.Count == 0)
            // // yield break;
            //
            // // 找到胜率最高的位置并输出
            // float maxRate = Single.NegativeInfinity;
            // CellPos cellPos = new();
            // foreach ((CellPos cellPos0, int[] counts) in posToWinCount)
            // {
            //     float rate = (float)counts[0] / counts[1];
            //     if (rate > maxRate)
            //     {
            //         maxRate = rate;
            //         cellPos = cellPos0;
            //     }
            // }


            bool block = BoardManager.Instance.blockOperation;
            BoardManager.Instance.blockOperation = true;

            yield return new WaitForSeconds(0.1f);
            CellPos cellPos = mcts.GetNextCellPos();
            yield return cellPos.GetBoard().TryFillCoroutine(cellPos.innerX, cellPos.innerY, aiPlayerType);

            BoardManager.Instance.blockOperation = block;
        }
    }
}