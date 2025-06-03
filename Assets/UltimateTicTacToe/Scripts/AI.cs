using System;
using System.Collections;
using System.Collections.Generic;
using Lucky.Framework;
using Lucky.Kits.Collections;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using Lucky.Kits.Managers;
using UltimateTicTacToe.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public struct CellPos
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
        TicTacToe ticTacToe = GetBoard().ticTacToe;
        BoardManager.Instance.bigBoard.ticTacToe[ticTacToe.x, ticTacToe.y] = playerType;
    }

    public Board GetBoard()
    {
        return BoardManager.Instance.nineBoards[boardX, boardY];
    }
}

public class MCTS
{
    /// 己方是 1, 空白是 0, 对方是 2
    public static Dictionary<string, int> pattern0ToScore = new()
    {
        { "000", 0 },
        { "100", 50 },
        { "010", 80 },
        { "001", 50 },
        { "110", 200 },
        { "101", 150 },
        { "011", 200 },
        { "111", 1000 },

        { "200", -50 },
        { "020", -80 },
        { "002", -50 },
        { "220", -200 },
        { "202", -150 },
        { "022", -200 },
        { "222", -1000 },

        { "120", 10 },
        { "102", 30 },
        { "012", 30 },
        { "210", 30 },
        { "201", 30 },
        { "021", 10 },

        { "112", 100 },
        { "121", 100 },
        { "211", 100 },

        { "221", -100 },
        { "212", -100 },
        { "122", -100 },
    };

    public static Dictionary<string, int> pattern1ToScore = new()
    {
        { "000", 0 },
        { "100", 50 },
        { "010", 80 },
        { "001", 50 },
        { "110", 200 },
        { "101", 150 },
        { "011", 200 },
        { "111", 100000 },

        { "200", -50 },
        { "020", -80 },
        { "002", -50 },
        { "220", -200 },
        { "202", -150 },
        { "022", -200 },
        { "222", -100000 },

        { "120", 10 },
        { "102", 30 },
        { "012", 30 },
        { "210", 30 },
        { "201", 30 },
        { "021", 10 },

        { "112", 100 },
        { "121", 100 },
        { "211", 100 },

        { "221", -100 },
        { "212", -100 },
        { "122", -100 },
        // { "000", 0 },
        // { "100", 500 },
        // { "010", 800 },
        // { "001", 500 },
        // { "110", 2000 },
        // { "101", 1500 },
        // { "011", 2000 },
        // { "111", 1000000 },
        //
        // { "200", -500 },
        // { "020", -800 },
        // { "002", -500 },
        // { "220", -2000 },
        // { "202", -1500 },
        // { "022", -2000 },
        // { "222", -1000000 },
        //
        // { "120", 100 },
        // { "102", 300 },
        // { "012", 300 },
        // { "210", 300 },
        // { "201", 300 },
        // { "021", 100 },
        //
        // { "112", 1000 },
        // { "121", 1000 },
        // { "211", 1000 },
        //
        // { "221", -1000 },
        // { "212", -1000 },
        // { "122", -1000 },
    };

    private List<MCTS> children = new();
    private Board[,] nineBoards;
    private Board bigBoard;
    private PlayerTypes playerType;

    public int value;
    public int totalValue;
    public int depth = 1;
    public int averageValue;
    public int visitedCount;

    public CellPos lastCellPos;

    public MCTS(Board[,] nineBoards, Board bigBoard, PlayerTypes playerType, CellPos lastCellPos)
    {
        this.nineBoards = nineBoards;
        this.bigBoard = bigBoard;
        this.playerType = playerType;
        this.lastCellPos = lastCellPos;
    }

    private void InitChildren(int currentBoardX, int currentBoardY)
    {
        var posesCanChoose = GetPosesCanChoose(currentBoardX, currentBoardY);
        foreach (CellPos cellPos in posesCanChoose)
        {
            FillAndRestoreCellPos(cellPos, () =>
            {
                MCTS mcts = new MCTS(nineBoards, bigBoard, playerType == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle, cellPos)
                {
                    value = GetValueByCurrentBoards(playerType),
                    totalValue = value
                };
                children.Add(mcts);
            });
        }
    }


    public void Simulate(int simulateTimes, int currentBoardX, int currentBoardY)
    {
        for (int _ = 0; _ < simulateTimes; _++)
        {
            Search(currentBoardX, currentBoardY);
        }
    }

    public void Search(int currentBoardX, int currentBoardY)
    {
        if (children.Count == 0)
        {
            InitChildren(currentBoardX, currentBoardY);
            return;
        }


        // GetPUCT
        MCTS bestChild = null;
        int bestPUCT = int.MinValue;
        foreach (MCTS child in children)
        {
            int PUCT = (int)(child.averageValue + 1.5f * child.value * Math.Sqrt(visitedCount) / (1 + child.visitedCount));
            if (PUCT > bestPUCT)
            {
                bestPUCT = PUCT;
                bestChild = child;
            }
        }

        FillAndRestoreCellPos(bestChild!.lastCellPos, () => { bestChild!.Search(bestChild.lastCellPos.boardX, bestChild.lastCellPos.boardY); });
        visitedCount += 1;
        // depth = bestChild.depth + 1;
        totalValue -= bestChild.averageValue;
        averageValue = totalValue / visitedCount;
    }

    private void FillAndRestoreCellPos(CellPos cellPos, Action callback)
    {
        Board board = cellPos.GetBoard();
        cellPos.SetSmallCellType(playerType);
        PlayerTypes winState = board.ticTacToe.CheckWinState();
        // 当前盘分出胜负
        if (winState is PlayerTypes.Circle or PlayerTypes.Cross)
        {
            cellPos.SetBigCellType(winState);
            callback?.Invoke();
            cellPos.SetBigCellType(PlayerTypes.None);
            cellPos.SetSmallCellType(PlayerTypes.None);
            return;
        }

        callback?.Invoke();
        cellPos.SetSmallCellType(PlayerTypes.None);
    }


    public CellPos GetNextCellPos()
    {
        // GetPUCT
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


    private List<CellPos> GetPosesCanChoose(int currentBoardX, int currentBoardY)
    {
        List<CellPos> posesCanChoose = new();
        if (!nineBoards[currentBoardX, currentBoardY].ticTacToe.isOver)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (nineBoards[currentBoardX, currentBoardY].ticTacToe[x, y] == PlayerTypes.None)
                        posesCanChoose.Add(new CellPos(currentBoardX, currentBoardY, x, y));
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
                                posesCanChoose.Add(new(boardX0, boardY0, innerX0, innerY0));
                        }
                    }
                }
            }
        }

        return posesCanChoose;
    }

    public int GetValueByCurrentBoards(PlayerTypes curPlayer)
    {
        int ans = 0;
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                ans += GetValueByCurrentBoard(nineBoards[x, y], curPlayer, false);
            }
        }

        ans += GetValueByCurrentBoard(bigBoard, curPlayer, true);
        return ans;
    }

    private static int count = 0;

    public int GetValueByCurrentBoard(Board board, PlayerTypes curPlayer, bool bigBoard)
    {
        TicTacToe ticTacToe = board.ticTacToe;

        int value = 0;

        string str = "";
        // 横    
        for (int y = 0; y < 3; y++)
        {
            str = "";
            for (int x = 0; x < 3; x++)
            {
                str += PlayerTypeToString(ticTacToe[x, y], curPlayer);
            }

            value += GetValueByPattern(str, bigBoard);
        }

        // 竖
        for (int x = 0; x < 3; x++)
        {
            str = "";
            for (int y = 0; y < 3; y++)
            {
                str += PlayerTypeToString(ticTacToe[x, y], curPlayer);
            }

            value += GetValueByPattern(str, bigBoard);
        }

        // 主对角线
        str = "";
        for (int i = 0; i < 3; i++)
        {
            str += PlayerTypeToString(ticTacToe[i, i], curPlayer);
        }

        value += GetValueByPattern(str, bigBoard);

        // 辅对角线
        str = "";
        for (int i = 0; i < 3; i++)
        {
            str += PlayerTypeToString(ticTacToe[i, 2 - i], curPlayer);
        }

        value += GetValueByPattern(str, bigBoard);
        if (count++ < 100)
            Debug.Log(this.value);
        return value;
    }

    private int GetValueByPattern(string str, bool bigBoard)
    {
        if (bigBoard)
            return pattern1ToScore[str];
        return pattern0ToScore[str];
    }


    private static string PlayerTypeToString(PlayerTypes playerType, PlayerTypes curPlayingPlayer)
    {
        if (curPlayingPlayer == PlayerTypes.Cross)
        {
            return playerType switch
            {
                PlayerTypes.Cross => "1",
                PlayerTypes.Circle => "2",
                _ => "0"
            };
        }

        return playerType switch
        {
            PlayerTypes.Cross => "2",
            PlayerTypes.Circle => "1",
            _ => "0"
        };
    }
}

public class AI : Singleton<AI>
{
    public PlayerTypes playerType = PlayerTypes.Circle;

    public Board[,] nineBoards => BoardManager.Instance.nineBoards;
    public Board bigBoard => BoardManager.Instance.bigBoard;

    public bool IsAITurn => BoardManager.Instance.useAI && BoardManager.Instance.curPlayer == playerType;
    public int searchTimes = 10;

    private int currentBoardX;
    private int currentBoardY;

    private bool thinking;
    public float thinkingTime = 1f;

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

        MCTS mcts = new MCTS(nineBoards, bigBoard, playerType, new CellPos());
        mcts.Simulate(searchTimes, BoardManager.Instance.currentBoardX, BoardManager.Instance.currentBoardY);
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
        yield return cellPos.GetBoard().TryFillCoroutine(cellPos.innerX, cellPos.innerY, playerType);

        BoardManager.Instance.blockOperation = block;
    }
}