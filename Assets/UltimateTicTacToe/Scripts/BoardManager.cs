using System;
using System.Collections;
using System.Collections.Generic;
using Lucky.Framework;
using Lucky.Kits.Extensions;
using Lucky.Kits.Interactive;
using UltimateTicTacToe.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum PlayerTypes
{
    None,
    Cross,
    Circle
}

public class BoardManager : Engine
{
    public static BoardManager Instance;

    // 单元格大小
    public float cellSize;

    // lineWidth
    public float lineWidth;

    public Board[,] nineBoards = new Board[3, 3];
    public Board bigBoard;
    public PlayerTypes curPlayer = PlayerTypes.Cross;

    public Vector3 BoardBottomLeft => transform.position - new Vector3(1, 1) * (cellSize * 4.5f);

    public bool gameover;

    public bool useAI;

    public bool blockOperation;


    protected override void Awake()
    {
        base.Awake();
        Instance = this;
    }

    private void Start()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                Board board = this.NewSonWithComponent<Board>();
                nineBoards[x, y] = board;
                board.UI.cellSize = board.cellSize = cellSize;
                board.transform.position = new Vector3(x * 3 * cellSize, y * 3 * cellSize) + BoardBottomLeft;
                board.UI.lineColor = Color.gray;
                board.canClick = x == 1 && y == 1;
            }
        }

        bigBoard = this.NewSonWithComponent<Board>();
        bigBoard.UI.cellSize = bigBoard.cellSize = cellSize * 3;
        bigBoard.transform.position = BoardBottomLeft;
        bigBoard.UI.lineColor = Color.white;
        bigBoard.canClick = false;
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("UltimateTicTacToe");
        }
    }
}