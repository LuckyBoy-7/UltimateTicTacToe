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
        BoardManager.Instance.bigBoard.ticTacToe[innerX, innerY] = playerType;
    }

    public Board GetBoard()
    {
        return BoardManager.Instance.nineBoards[boardX, boardY];
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
        // // 找到空位置
        // List<CellPos> posesCanChoose = new();
        // for (int boardX = 0; boardX < 3; boardX++)
        // {
        //     for (int boardY = 0; boardY < 3; boardY++)
        //     {
        //         for (int innerX = 0; innerX < 3; innerX++)
        //         {
        //             for (int innerY = 0; innerY < 3; innerY++)
        //             {
        //                 PlayerTypes curPlayerType = nineBoards[boardX, boardY].ticTacToe[innerX, innerY];
        //                 if (curPlayerType == PlayerTypes.None)
        //                     posesCanChoose.Add(new(boardX, boardY, innerX, innerY));
        //             }
        //         }
        //     }
        // }

        thinking = true;
        yield return new WaitForSeconds(thinkingTime);
        thinking = false;
        // 随机搜索若干次, 记录选某个位置胜利的次数
        DefaultDict<CellPos, int[]> posToWinCount = new DefaultDict<CellPos, int[]>(() => new int[2] { 0, 0 });
        for (int _ = 0; _ < searchTimes; _++)
        {
            currentBoardX = BoardManager.Instance.currentBoardX;
            currentBoardY = BoardManager.Instance.currentBoardY;
            (CellPos cellPos0, int win) = DFSNineBoards(playerType);
            // win
            posToWinCount[cellPos0][0] += win;
            posToWinCount[cellPos0][1] += 1;
        }

        // if (posToWinCount.Count == 0)
        // yield break;

        // 找到胜率最高的位置并输出
        float maxRate = Single.NegativeInfinity;
        CellPos cellPos = new();
        foreach ((CellPos cellPos0, int[] counts) in posToWinCount)
        {
            float rate = (float)counts[0] / counts[1];
            if (rate > maxRate)
            {
                maxRate = rate;
                cellPos = cellPos0;
            }
        }


        bool block = BoardManager.Instance.blockOperation;
        BoardManager.Instance.blockOperation = true;

        yield return new WaitForSeconds(0.1f);
        yield return cellPos.GetBoard().TryFillCoroutine(cellPos.innerX, cellPos.innerY, playerType);

        BoardManager.Instance.blockOperation = block;
    }

    private (CellPos, int ) DFSNineBoards(PlayerTypes curPlayer)
    {
        // int GetChosenIndex()
        // {
        //     if (!nineBoards[currentBoardX, currentBoardY].ticTacToe.isOver)
        //     {
        //         return UnityEngine.Random.Range(0, posesCanChoose.Count);
        //     }
        //     
        // }
        // 找到能用的空位置
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

        // 填满并胜利的情况已经被 gameover 考虑, 所以这里一定是平局
        if (posesCanChoose.Count == 0)
            return (new CellPos(), 0);

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
                return (chosenPos, curPlayer == winState ? 1 : -1);
            }
        }

        currentBoardX = chosenPos.innerX; // 这个不用恢复现场, 到时候外面会重新赋值的
        currentBoardY = chosenPos.innerY;
        (CellPos _, int win) = DFSNineBoards(playerType == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle);
        // 恢复现场
        chosenPos.SetSmallCellType(PlayerTypes.None);
        bigBoard.ticTacToe[board.ticTacToe.x, board.ticTacToe.y] = PlayerTypes.None;
        posesCanChoose.Add(chosenPos);
        (posesCanChoose[chosenIndex], posesCanChoose[^1]) = (posesCanChoose[^1], posesCanChoose[chosenIndex]);

        return (chosenPos, win == 0 ? 0 : -win);
    }
}