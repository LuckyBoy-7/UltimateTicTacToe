using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        { "100", 500 },
        { "010", 800 },
        { "001", 500 },
        { "110", 2000 },
        { "101", 1500 },
        { "011", 2000 },
        { "111", 10000 },

        { "200", -5000 },
        { "020", -8000 },
        { "002", -5000 },
        { "220", -20000 },
        { "202", -15000 },
        { "022", -20000 },
        { "222", -100000 },

        { "120", 100 },
        { "102", 300 },
        { "012", 300 },
        { "210", 300 },
        { "201", 300 },
        { "021", 100 },

        { "112", 1000 },
        { "121", 1000 },
        { "211", 1000 },

        { "221", -1000 },
        { "212", -1000 },
        { "122", -1000 },
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

    public List<MCTS> children = new();
    private Board[,] nineBoards;
    private Board bigBoard;
    private PlayerTypes playerType;

    public int totalValue;
    public float averageValue => (float)totalValue / visitedCount;
    public int visitedCount;

    public CellPos lastCellPos;
    public MCTS root;
    public MCTS parent;

    public MCTS(Board[,] nineBoards, Board bigBoard, PlayerTypes playerType, CellPos lastCellPos)
    {
        this.nineBoards = nineBoards;
        this.bigBoard = bigBoard;
        this.playerType = playerType;
        this.lastCellPos = lastCellPos;
    }

    private PlayerTypes GetOppositePlayerType(PlayerTypes type)
    {
        return type == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle;
    }

    private void Expansion(int currentBoardX, int currentBoardY)
    {
        foreach (CellPos cellPos in GetPosesCanChoose(currentBoardX, currentBoardY))
        {
            FillAndRestoreCellPos(cellPos, () =>
            {
                MCTS mcts = new MCTS(nineBoards, bigBoard, GetOppositePlayerType(playerType), cellPos)
                {
                    root = root,
                    parent = this
                };
                children.Add(mcts);
            });
        }
    }


    public void Selects(int simulateTimes, int currentBoardX, int currentBoardY)
    {
        for (int _ = 0; _ < simulateTimes; _++)
        {
            Select(currentBoardX, currentBoardY);
        }
    }

    private static int count;

    private void Select(int currentBoardX, int currentBoardY)
    {
        var poses = GetPosesCanChoose(currentBoardX, currentBoardY).GetEnumerator();
        if (!poses.MoveNext()) // 平局/满了
        {
            PlayerTypes ans = bigBoard.ticTacToe.CheckWinState();
            // Simulate();
            Backpropagation(ans);
            return;
        }
        poses.Dispose();

        if (children.Count == 0)
        {
            Expansion(currentBoardX, currentBoardY);
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

            float UCB = (float)(child.averageValue + 2f * Math.Sqrt(Math.Log(root.visitedCount) / child.visitedCount));
  

            if (UCB > bestUCB)
            {
                bestUCB = UCB;
                bestChild = child;
            }
        }


        FillAndRestoreCellPos(bestChild!.lastCellPos, () => { bestChild!.Select(bestChild.lastCellPos.innerX, bestChild.lastCellPos.innerY); });
        // depth = bestChild.depth + 1;
        // totalValue += lastChild.playerType == playerType ? lastChild.totalValue : -lastChild.totalValue;
    }

    private void FillAndRestoreCellPos(CellPos cellPos, Action callback)
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
        // 填满并胜利的情况已经被 gameover 考虑, 所以这里一定是平局
        if (posesCanChoose.Count == 0)
            return PlayerTypes.None;

        // 抽一个位置
        int chosenIndex = UnityEngine.Random.Range(0, posesCanChoose.Count);
        (posesCanChoose[chosenIndex], posesCanChoose[^1]) = (posesCanChoose[^1], posesCanChoose[chosenIndex]);
        CellPos chosenPos = posesCanChoose.Pop();

        // 填入符号
        Board board = chosenPos.GetBoard();
        chosenPos.SetSmallCellType(curPlayer);
        PlayerTypes winState = board.ticTacToe.CheckWinState();
        // 当前盘分出胜负
        if (winState is PlayerTypes.Circle or PlayerTypes.Cross)
        {
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
                posesCanChoose.Add(chosenPos);
                (posesCanChoose[chosenIndex], posesCanChoose[^1]) = (posesCanChoose[^1], posesCanChoose[chosenIndex]);
                return curPlayer == winState ? curPlayer : GetOppositePlayerType(curPlayer);
            }
        }

        currentBoardX = chosenPos.innerX; // 这个不用恢复现场, 到时候外面会重新赋值的
        currentBoardY = chosenPos.innerY;
        var winner = RandomFillUntilEnd(GetOppositePlayerType(curPlayer), currentBoardX, currentBoardY);
        // 恢复现场
        board.ticTacToe.isOver = false;
        chosenPos.SetSmallCellType(PlayerTypes.None);
        bigBoard.ticTacToe[board.ticTacToe.x, board.ticTacToe.y] = PlayerTypes.None;
        posesCanChoose.Add(chosenPos);
        (posesCanChoose[chosenIndex], posesCanChoose[^1]) = (posesCanChoose[^1], posesCanChoose[chosenIndex]);

        return winner;
    }

    public void Simulate()
    {
        PlayerTypes ans = PlayerTypes.None;
        FillAndRestoreCellPos(lastCellPos, () => { ans = RandomFillUntilEnd(GetOppositePlayerType(playerType), lastCellPos.innerX, lastCellPos.innerY); });
        Backpropagation(ans);
    }

    public void Backpropagation(PlayerTypes ans)
    {
        visitedCount++;
        totalValue += ans == PlayerTypes.None ? 1 : ans == playerType ? 2 : -2;
        if (parent == null)
            return;
        parent.Backpropagation(ans);
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

    public int GetValueByCurrentBoards(PlayerTypes curPlayer)
    {
        var winner = bigBoard.ticTacToe.CheckWinState();
        if (winner == PlayerTypes.None)
            return 0;
        if (winner == curPlayer)
            return 1;
        return -1;

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
    public int searchTimes = 10000;

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
        mcts.root = mcts;
        mcts.Selects(searchTimes, BoardManager.Instance.currentBoardX, BoardManager.Instance.currentBoardY);

        print(mcts.totalValue);

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
        yield return cellPos.GetBoard().TryFillCoroutine(cellPos.innerX, cellPos.innerY, playerType);

        BoardManager.Instance.blockOperation = block;
    }
}