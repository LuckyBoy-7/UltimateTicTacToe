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
    public float cellSize;
    private float LineWidth => BoardManager.Instance.lineWidth;

    public Color lineColor;
    public static int lineSortingOrder = 0;

    public bool canClick;

    [ShowInInspector] public Cell[,] cells = new Cell[3, 3];
    public Cell winner;
    public bool isOver;
    public Board[,] nineBoards => BoardManager.Instance.nineBoards;
    public Board parentBoard => BoardManager.Instance.bigBoard;
    public SpriteRenderer canClickHint;
    private bool isFilling;


    private void Start()
    {
        Init();
        DrawBoard();
    }

    private void Init()
    {
        canClickHint = this.NewSonWithComponent<SpriteRenderer>();
        canClickHint.sprite = Resources.Load<Sprite>("Default/Sprites/Square");
        canClickHint.transform.position = transform.position + (Vector3)Vector2.one * (cellSize * 1.5f);
        canClickHint.transform.localScale = Vector3.one * cellSize * 3;
        canClickHint.color = canClickHint.color.WithA(0.1f);
        canClickHint.sortingOrder = 100;
    }

    private void DrawBoard()
    {
        // 横
        for (int y = 0; y < 4; y++)
        {
            var line = GetLine();
            Vector2 pos0 = transform.position + Vector3.up * cellSize * y;
            Vector2 pos1 = transform.position + Vector3.up * cellSize * y + Vector3.right * cellSize * 3;
            line.SetPosition(0, pos0);
            line.SetPosition(1, pos1);
        }

        // 竖
        for (int x = 0; x < 4; x++)
        {
            var line = GetLine();
            Vector2 pos0 = transform.position + Vector3.right * cellSize * x;
            Vector2 pos1 = transform.position + Vector3.right * cellSize * x + Vector3.up * cellSize * 3;
            line.SetPosition(0, pos0);
            line.SetPosition(1, pos1);
        }
    }

    private LineRenderer GetLine()
    {
        var line = this.NewSonWithComponent<LineRenderer>();
        line.startWidth = line.endWidth = LineWidth;
        line.startColor = line.endColor = lineColor;
        line.numCapVertices = 90;
        // line.material = new Material("Default-Line");
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = lineSortingOrder++;
        line.useWorldSpace = false;
        return line;
    }

    protected override void ManagedUpdate()
    {
        base.ManagedUpdate();

        if (Input.GetMouseButtonDown(0) && canClick && !isFilling)
        {
            OnClick();
        }

        canClickHint.enabled = canClick;
    }

    private void OnClick()
    {
        Vector2 mousePos = GameCursor.MouseWorldPosition;
        Vector2 delta = mousePos - (Vector2)transform.position;
        (int x, int y) = ((int)Math.Floor(delta.x / cellSize), (int)Math.Floor(delta.y / cellSize));
        var cell = BoardManager.Instance.curPlayer == PlayerTypes.Circle ? typeof(Circle) : typeof(Cross);
        StartCoroutine(TryFillCoroutine(x, y, cell));
    }

    private IEnumerator TryFillCoroutine(int x, int y, Type cellType)
    {
        // 不是在当前board点的
        if (x < 0 || x >= 3 || y < 0 || y >= 3)
            yield break;

        // 填过了, 结束了(填满或胜利)
        if (cells[x, y] != null || isOver)
            yield break;

        // 填入圈或者叉
        isFilling = true;
        yield return FillCell(x, y, cellType);
        isFilling = false;

        if (CheckWinState())
        {
            isOver = true;
            if (parentBoard == this) // 如果当前是bigBoard
            {
                print("game over");
                GameOver();
            }
            else
            {
                StartCoroutine(Rollback());
                yield return StartCoroutine(FillParentCell());
            }
        }

        if (!isOver && CheckFull())
        {
            isOver = true;
        }

        if (parentBoard != this && !BoardManager.Instance.gameover)
        {
            NextTurn(x, y);
        }
    }

    private bool CheckFull()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (cells[x, y] == null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private IEnumerator Rollback()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (cells[x, y])
                    cells[x, y].Rollback();
            }
        }

        foreach (var line in GetComponentsInChildren<LineRenderer>())
        {
            Color startColor = line.startColor;
            Color endColor = startColor.WithA(0);
            line.DOColor(new Color2(startColor, startColor), new Color2(endColor, endColor), Cell.AnimationDuration);
        }

        yield return new WaitForSeconds(Cell.AnimationDuration);
    }

    private IEnumerator FillParentCell()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (nineBoards[x, y] == this)
                {
                    yield return StartCoroutine(parentBoard.TryFillCoroutine(x, y, winner.GetType()));
                }
            }
        }
    }

    public IEnumerator FillCell(int x, int y, Type cellType)
    {
        var cell = cellType == typeof(Circle) ? Cell.CreateCircle(this, cellSize) : Cell.CreateCross(this, cellSize);
        cells[x, y] = cell;

        Vector2 cellPos = transform.position + new Vector3(x, y) * cellSize;
        Vector2 cellCenterPos = cellPos + Vector2.one * (cellSize / 2f);
        cell.transform.position = cellCenterPos;
        yield return new WaitForSeconds(Cell.AnimationDuration);
    }

    private bool CheckWinState()
    {
        // 横
        for (int y = 0; y < 3; y++)
        {
            if (cells[0, y] != null && cells[0, y]?.GetType() == cells[1, y]?.GetType() && cells[1, y]?.GetType() == cells[2, y]?.GetType())
            {
                winner = cells[0, y];
                return true;
            }
        }

        // 竖
        for (int x = 0; x < 3; x++)
        {
            if (cells[x, 0] != null && cells[x, 0]?.GetType() == cells[x, 1]?.GetType() && cells[x, 1]?.GetType() == cells[x, 2]?.GetType())
            {
                winner = cells[x, 0];
                return true;
            }
        }

        // 主对角线
        if (cells[0, 0] != null && cells[0, 0]?.GetType() == cells[1, 1]?.GetType() && cells[1, 1]?.GetType() == cells[2, 2]?.GetType())
        {
            winner = cells[0, 0];
            return true;
        }

        // 辅对角线
        if (cells[0, 2] != null && cells[0, 2]?.GetType() == cells[1, 1]?.GetType() && cells[1, 1]?.GetType() == cells[2, 0]?.GetType())
        {
            winner = cells[0, 2];
            return true;
        }

        return false;
    }

    private void NextTurn(int nx, int ny)
    {
        BoardManager.Instance.curPlayer = BoardManager.Instance.curPlayer == PlayerTypes.Circle ? PlayerTypes.Cross : PlayerTypes.Circle;

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                nineBoards[x, y].canClick = false;
            }
        }

        if (nineBoards[nx, ny].isOver)
        {
            GiveArbitraryClickPermission();
        }
        else
        {
            nineBoards[nx, ny].canClick = true;
        }
    }

    private void GiveArbitraryClickPermission()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                nineBoards[x, y].canClick = !nineBoards[x, y].isOver;
            }
        }
    }

    private void GameOver()
    {
        BoardManager.Instance.gameover = true;
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                nineBoards[x, y].canClick = false;
                nineBoards[x, y].canClickHint.enabled = false;
            }
        }
    }
}