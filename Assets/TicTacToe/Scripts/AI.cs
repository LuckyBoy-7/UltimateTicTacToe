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
using UltimateTicTacToe.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using EventManager = UltimateTicTacToe.Scripts.EventManager;

namespace TicTacToe
{
    public struct CellPos : IEquatable<CellPos>
    {
        public int innerX;
        public int innerY;


        public CellPos(int innerX = -1, int innerY = -1)
        {
            this.innerX = innerX;
            this.innerY = innerY;
        }

        public override string ToString()
        {
            return $"(({innerX}, {innerY})";
        }


        public void SetCellType(PlayerTypes playerType)
        {
            BoardManager.Instance.curBoard.ticTacToe[innerX, innerY] = playerType;
        }


        public bool Equals(CellPos other)
        {
            return innerX == other.innerX && innerY == other.innerY;
        }

        public override bool Equals(object obj)
        {
            return obj is CellPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(innerX, innerY);
        }
    }

    public class MCTS
    {
        public List<MCTS> children = new();

        // public Dictionary<CellPos, MCTS> cellPosToChildren = new();
        private Board curBoard;
        private PlayerTypes playerType;

        public int totalValue;
        public float averageValue => (float)totalValue / visitedCount;
        public int visitedCount;

        public CellPos lastCellPos;
        public MCTS parent;

        public MCTS(Board curBoard, PlayerTypes playerType, CellPos lastCellPos)
        {
            this.curBoard = curBoard;
            this.playerType = playerType;
            this.lastCellPos = lastCellPos;
        }

        public override string ToString()
        {
            return $"UCB: {GetUCB()}, totalValue: {totalValue}, visitedCount: {visitedCount},  playerType: {playerType}, cellPos: {lastCellPos}";
        }

        private PlayerTypes GetOppositePlayerType(PlayerTypes type)
        {
            return type == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle;
        }

        private void Expansion()
        {
            foreach (CellPos cellPos in GetPosesCanChoose())
            {
                FillAndRestoreCellPos(cellPos, () =>
                {
                    MCTS mcts = new MCTS(curBoard, GetOppositePlayerType(playerType), cellPos)
                    {
                        parent = this
                    };
                    children.Add(mcts);
                });
            }
        }


        public void Selects(int simulateTimes)
        {
            for (int _ = 0; _ < simulateTimes; _++)
            {
                Select();
            }
        }

        public MCTS GetNextMCTS(CellPos cellPos)
        {
            MCTS child = null;
            if (children.Count == 0)
            {
                child = new MCTS(curBoard, GetOppositePlayerType(playerType), cellPos)
                {
                    parent = this
                };
            }
            else
            {
                child = children.Find(c => c.lastCellPos.Equals(cellPos));
            }

            return child;
        }

        private static int count = 0;

        private void Select()
        {
            var poses = GetPosesCanChoose().GetEnumerator();
            if (!poses.MoveNext()) // 平局/满了
            {
                PlayerTypes ans = curBoard.ticTacToe.CheckWinState();
                // Simulate();
                Backpropagation(ans);
                return;
            }

            poses.Dispose();

            if (children.Count == 0)
            {
                Expansion();
                MCTS child = children.Choice();
                child.Simulate();
                return;
            }


            // 找到目前相对最优的子节点
            MCTS bestChild = null;
            float bestUCB = int.MinValue;
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
            }


            // if (count++ % 1000 == 0)
            // Debug.Log(bestChild);

            FillAndRestoreCellPos(bestChild!.lastCellPos, () => { bestChild!.Select(); });
            // depth = bestChild.depth + 1;
            // totalValue += lastChild.playerType == playerType ? lastChild.totalValue : -lastChild.totalValue;
        }

        private float GetUCB()
        {
            if (parent == null)
                return 0;
            return (float)(averageValue + AI.Instance.c * Math.Sqrt(Math.Log(AI.Instance.mcts.visitedCount) / visitedCount));
            // return (float)(averageValue + AI.Instance.c * Math.Sqrt(Math.Log(parent.visitedCount) / visitedCount));
        }

        public void FillAndRestoreCellPos(CellPos cellPos, Action callback)
        {
            Board board = curBoard;
            cellPos.SetCellType(playerType);
            PlayerTypes winState = board.ticTacToe.CheckWinState();
            // 当前盘分出胜负
            if (winState is PlayerTypes.Circle or PlayerTypes.Cross)
            {
                board.ticTacToe.isOver = true;
                callback?.Invoke();
                cellPos.SetCellType(PlayerTypes.None);
                board.ticTacToe.isOver = false;
                return;
            }

            callback?.Invoke();
            board.ticTacToe.isOver = false;
            cellPos.SetCellType(PlayerTypes.None);
        }

        public PlayerTypes RandomFillUntilEnd(PlayerTypes curPlayer)
        {
            // 找到能用的空位置
            List<CellPos> posesCanChoose = GetPosesCanChoose().ToList();
            if (posesCanChoose.Count == 0)
                return PlayerTypes.None;

            // 抽一个位置
            int chosenIndex = UnityEngine.Random.Range(0, posesCanChoose.Count);
            CellPos chosenPos = posesCanChoose[chosenIndex];

            // 填入符号
            chosenPos.SetCellType(curPlayer);
            PlayerTypes winState = curBoard.ticTacToe.CheckWinState();
            // 当前盘分出胜负
            if (winState is PlayerTypes.Circle or PlayerTypes.Cross)
            {
                // 恢复现场
                chosenPos.SetCellType(PlayerTypes.None);
                return winState;
            }

            var winner = RandomFillUntilEnd(GetOppositePlayerType(curPlayer));
            // 恢复现场
            chosenPos.SetCellType(PlayerTypes.None);

            return winner;
        }

        public void Simulate()
        {
            PlayerTypes ans = PlayerTypes.None;
            FillAndRestoreCellPos(lastCellPos, () => { ans = RandomFillUntilEnd(GetOppositePlayerType(playerType)); });
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

            return bestChild!.lastCellPos;
        }


        private IEnumerable<CellPos> GetPosesCanChoose()
        {
            if (curBoard.ticTacToe.isOver)
                yield break;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (curBoard.ticTacToe[x, y] == PlayerTypes.None)
                        yield return new CellPos(x, y);
                }
            }
        }
    }

    public class AI : Singleton<AI>
    {
        public PlayerTypes aiPlayerType = PlayerTypes.Circle;

        public bool IsAITurn => BoardManager.Instance.useAI && BoardManager.Instance.curPlayer == aiPlayerType;
        public int searchTimes = 10000;

        private int currentBoardX;
        private int currentBoardY;

        private bool thinking;
        public float thinkingTime = 1f;

        public MCTS mcts;
        public float c = 1.1f;

        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();

            if (IsAITurn && !BoardManager.Instance.blockOperation && !BoardManager.Instance.gameover && !thinking)
            {
                StartCoroutine(Play());
            }
        }

        private void Start()
        {
            mcts = new MCTS(BoardManager.Instance.curBoard, BoardManager.Instance.curPlayer, new CellPos());
            EventManager.Instance.AfterFillCell += AfterFillCell;
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
            mcts.parent = null;
        }
        

        private IEnumerator Play()
        {
            thinking = true;
            yield return new WaitForSeconds(thinkingTime);
            thinking = false;

            // MCTS mcts = new MCTS(nineBoards, bigBoard, aiPlayerType, new CellPos());
           


            Debug.Log("====================================");
            print(mcts);
            mcts.Selects(searchTimes);
            print(mcts.totalValue);
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
            yield return BoardManager.Instance.curBoard.TryFillCoroutine(cellPos.innerX, cellPos.innerY, aiPlayerType);

            BoardManager.Instance.blockOperation = block;
        }
    }
}