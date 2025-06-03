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
        // List<Tuple<int, int, int, int>> posesCanChoose = new();
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
        DefaultDict<Tuple<int, int, int, int>, int[]> posToWinCount = new DefaultDict<Tuple<int, int, int, int>, int[]>(() => new int[2] { 0, 0 });
        for (int _ = 0; _ < searchTimes; _++)
        {
            currentBoardX = BoardManager.Instance.currentBoardX;
            currentBoardY = BoardManager.Instance.currentBoardY;
            (int boardX, int boardY, int innerX, int innerY, int win) = DFSNineBoards(playerType);
            if (boardX == -1)
                continue;
            var t = new Tuple<int, int, int, int>(boardX, boardY, innerX, innerY);
            // win
            posToWinCount[t][0] += win;
            posToWinCount[t][1] += 1;
        }

        // if (posToWinCount.Count == 0)
        // yield break;

        // 找到胜率最高的位置并输出
        float maxRate = Single.NegativeInfinity;
        int ansBoardX = -1;
        int ansboardY = -1;
        int ansinnerX = -1;
        int ansinnerY = -1;
        foreach (((int boardX, int boardY, int innerX, int innerY), int[] counts) in posToWinCount)
        {
            float rate = (float)counts[0] / counts[1];
            if (rate > maxRate)
            {
                maxRate = rate;
                ansBoardX = boardX;
                ansboardY = boardY;
                ansinnerX = innerX;
                ansinnerY = innerY;
            }
        }


        bool block = BoardManager.Instance.blockOperation;
        BoardManager.Instance.blockOperation = true;

        yield return new WaitForSeconds(0.1f);
        yield return nineBoards[ansBoardX, ansboardY].TryFillCoroutine(ansinnerX, ansinnerY, playerType);

        BoardManager.Instance.blockOperation = block;
    }

    private (int, int, int, int, int win) DFSNineBoards(PlayerTypes curPlayer)
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
        List<Tuple<int, int, int, int>> posesCanChoose = new();
        if (!nineBoards[currentBoardX, currentBoardY].ticTacToe.isOver)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (nineBoards[currentBoardX, currentBoardY].ticTacToe[x, y] == PlayerTypes.None)
                        posesCanChoose.Add(new Tuple<int, int, int, int>(currentBoardX, currentBoardY, x, y));
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
            return (-1, -1, -1, -1, 0);

        // 抽一个位置
        int chosenIndex = UnityEngine.Random.Range(0, posesCanChoose.Count);
        (posesCanChoose[chosenIndex], posesCanChoose[^1]) = (posesCanChoose[^1], posesCanChoose[chosenIndex]);
        Tuple<int, int, int, int> chosenPos = posesCanChoose.Pop();
        (int boardX, int boardY, int innerX, int innerY) = chosenPos;

        // 填入符号
        Board board = nineBoards[boardX, boardY];
        board.ticTacToe[innerX, innerY] = curPlayer;
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
                board.ticTacToe[innerX, innerY] = PlayerTypes.None;
                board.ticTacToe.isOver = false;
                bigBoard.ticTacToe[board.ticTacToe.x, board.ticTacToe.y] = PlayerTypes.None;
                bigBoard.ticTacToe.isOver = false;
                posesCanChoose.Add(chosenPos);
                (posesCanChoose[chosenIndex], posesCanChoose[^1]) = (posesCanChoose[^1], posesCanChoose[chosenIndex]);
                return (boardX, boardY, innerX, innerY, curPlayer == winState ? 1 : -1);
            }
        }

        currentBoardX = innerX; // 这个不用恢复现场, 到时候外面会重新赋值的
        currentBoardY = innerY;
        (int _, int _, int _, int _, int win) = DFSNineBoards(playerType == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle);
        // 恢复现场
        board.ticTacToe[innerX, innerY] = PlayerTypes.None;
        bigBoard.ticTacToe[board.ticTacToe.x, board.ticTacToe.y] = PlayerTypes.None;
        posesCanChoose.Add(chosenPos);
        (posesCanChoose[chosenIndex], posesCanChoose[^1]) = (posesCanChoose[^1], posesCanChoose[chosenIndex]);

        return (boardX, boardY, innerX, innerY, win == 0 ? 0 : -win);
    }
}