using System;
using System.Collections;
using System.Collections.Generic;
using Lucky.Framework;
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

    public bool IsAITurn => BoardManager.Instance.useAI && BoardManager.Instance.curPlayer == playerType;
    public bool thinking;
    public int searchTimes = 10;

    protected override void ManagedUpdate()
    {
        base.ManagedUpdate();

        // if (IsAITurn && !thinking && !BoardManager.Instance.gameover)
        // {
        //     StartCoroutine(ThinkAndPlay());
        // }
    }

    private IEnumerator ThinkAndPlay()
    {
        thinking = true;
        yield return new WaitForSeconds(1f);

        Play();

        thinking = false;
    }

    private void Play()
    {
        // 找到空位置
        // List<Tuple<int, int>> posesCanChoose = new();
        // PlayerTypes[,] boards = new PlayerTypes[9, 9];
        // for (int x = 0; x < 9; x++)
        // {
        //     for (int y = 0; y < 9; y++)
        //     {
        //         Cell cell = nineBoards[x / 3, y / 3].cells[x % 3, y % 3];
        //         if (cell == null)
        //             posesCanChoose.Add(new(x, y));
        //         else if (cell is Circle)
        //             boards[x, y] = PlayerTypes.Circle;
        //         else
        //             boards[x, y] = PlayerTypes.Cross;
        //     }
        // }


        // 随机搜索若干次
        // (int x, int y, bool win) tuple = DFSNineBoards(boards, posesCanChoose, playerType);


        // 找到胜率最高的位置并输出
    }

    // private (int x, int y, bool win) DFSNineBoards(PlayerTypes[,] boards, List<Tuple<int, int>> posesCanChoose, PlayerTypes curPlayer)
    // {
    //     // 抽一个位置
    //     int chosenIndex = UnityEngine.Random.Range(0, posesCanChoose.Count);
    //     (posesCanChoose[chosenIndex], posesCanChoose[^1]) = (posesCanChoose[^1], posesCanChoose[chosenIndex]);
    //     (int x, int y) = posesCanChoose.Pop(chosenIndex);
    //
    //     // 填入符号
    //     boards[x, y] = curPlayer;
    //     PlayerTypes winState = CheckWinState(boards, x / 3 * 3, y / 3 * 3);
    //     if (winState == PlayerTypes.Circle || winState == PlayerTypes.Cross)
    //         return (x, y, curPlayer == winState);
    //     (int _, int _, bool win) = DFSNineBoards(boards, posesCanChoose, playerType == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle);
    // }

    private PlayerTypes CheckWinState(PlayerTypes[,] boards, int boardX, int boardY)
    {
        // 横
        for (int y = 0; y < 3; y++)
        {
            if (boards[boardX, boardY + y] != PlayerTypes.None && boards[boardX, boardY + y] == boards[boardX + 1, boardY + y] &&
                boards[boardX + 1, boardY + y] == boards[boardX + 2, boardY + y])
            {
                return boards[boardX, boardY];
            }
        }

        // 竖
        for (int x = 0; x < 3; x++)
        {
            if (boards[boardX + x, boardY] != PlayerTypes.None && boards[boardX + x, boardY] == boards[boardX + x, boardY + 1] &&
                boards[boardX + x, boardY + 1] == boards[boardX + x, boardY + 2])
            {
                return boards[boardX, boardY];
            }
        }

        // 主对角线
        if (boards[boardX, boardY] != PlayerTypes.None && boards[boardX, boardY] == boards[boardX + 1, boardY + 1] &&
            boards[boardX + 1, boardY + 1] == boards[boardX + 2, boardY + 2])
        {
            return boards[boardX, boardY];
        }

        // 副对角线
        if (boards[boardX, boardY + 2] != PlayerTypes.None && boards[boardX, boardY + 2] == boards[boardX + 1, boardY + 1] &&
            boards[boardX + 1, boardY + 1] == boards[boardX + 2, boardY])
        {
            return boards[boardX, boardY + 2];
        }

        return PlayerTypes.None;
    }
}