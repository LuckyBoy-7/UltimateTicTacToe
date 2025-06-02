using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lucky.Framework;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using Lucky.Kits.Utilities;
using Sirenix.OdinInspector;
using UltimateTicTacToe.Scripts;
using UnityEngine;

public class Board : ManagedBehaviour
{
    public BoardUI UI;
    public float cellSize;

    [ShowInInspector] public TicTacToe ticTacToe = new();
    public Board[,] nineBoards => BoardManager.Instance.nineBoards;
    public Board parentBoard => BoardManager.Instance.bigBoard;
    public bool IsPlayerTurn => !BoardManager.Instance.useAI || BoardManager.Instance.curPlayer != AI.Instance.playerType;


    private void Awake()
    {
        UI = this.NewSonWithComponent<BoardUI>();
    }


    protected override void ManagedUpdate()
    {
        base.ManagedUpdate();

        bool canFill = CheckCanFill();
        if (Input.GetMouseButtonDown(0) && canFill && IsPlayerTurn && !BoardManager.Instance.blockOperation)
        {
            OnClick();
        }

        if (!BoardManager.Instance.blockOperation)
            UI.UpdateUI(canFill);
    }


    private bool CheckCanFill()
    {
        if (ticTacToe.isOver || parentBoard.ticTacToe.isOver || parentBoard == this)
            return false;
        int x = BoardManager.Instance.currentBoardX;
        int y = BoardManager.Instance.currentBoardY;
        if (x == ticTacToe.x && y == ticTacToe.y)
            return true;
        if (nineBoards[x, y].ticTacToe.isOver)
            return true;
        return false;
    }


    private void OnClick()
    {
        Vector2 mousePos = GameCursor.MouseWorldPosition;
        Vector2 delta = mousePos - (Vector2)transform.position;
        (int x, int y) = ((int)Math.Floor(delta.x / cellSize), (int)Math.Floor(delta.y / cellSize));
        StartCoroutine(TryFillCoroutine(x, y, BoardManager.Instance.curPlayer));
    }

    public IEnumerator TryFillCoroutine(int x, int y, PlayerTypes playerType)
    {
        // 不是在当前board点的
        if (x < 0 || x >= 3 || y < 0 || y >= 3)
            yield break;


        // 填过了, 结束了(填满或胜利)
        if (ticTacToe[x, y] != PlayerTypes.None || ticTacToe.isOver)
            yield break;

        // 填入圈或者叉
        bool block = BoardManager.Instance.blockOperation;
        BoardManager.Instance.blockOperation = true;
        yield return UI.FillCoroutine(x, y, playerType);
        BoardManager.Instance.blockOperation = block;

        ticTacToe[x, y] = playerType;

        ticTacToe.winner = ticTacToe.CheckWinState();
        if (ticTacToe.winner != PlayerTypes.None)
        {
            ticTacToe.isOver = true;
            if (parentBoard == this) // 如果当前是bigBoard
            {
                print("game over");
                GameOver();
            }
            else
            {
                UI.StartCoroutine(UI.Rollback());
                yield return StartCoroutine(FillParentCell());
            }
        }

        if (!ticTacToe.isOver && ticTacToe.CheckFull())
        {
            ticTacToe.isOver = true;
        }

        if (parentBoard != this && !BoardManager.Instance.gameover)
        {
            NextTurn(x, y);
        }
    }


    private IEnumerator FillParentCell()
    {
        yield return StartCoroutine(parentBoard.TryFillCoroutine(ticTacToe.x, ticTacToe.y, ticTacToe.winner));
    }


    private void NextTurn(int nx, int ny)
    {
        BoardManager.Instance.curPlayer = BoardManager.Instance.curPlayer == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle;

        BoardManager.Instance.currentBoardX = nx;
        BoardManager.Instance.currentBoardY = ny;
    }


    private void GameOver()
    {
        BoardManager.Instance.gameover = true;
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                nineBoards[x, y].UI.canClickHint.enabled = false;
            }
        }
    }
}