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
using Settings = UltimateTicTacToe.Scripts.Settings;

/// <summary>
/// 参考视频: https://www.bilibili.com/video/BV1nT421Q7Do
/// </summary>
namespace UltimateTicTacToe
{
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

        public bool blockOperation;

        public int currentBoardX = 1;
        public int currentBoardY = 1;

        public float c = 1.1f;
        public int playedCharNumber;

        public PlayerSettingsController playerSettings0;
        public PlayerSettingsController playerSettings1;
        public AI AIPlayer0 => playerSettings0.ai;
        public AI AIPlayer1 => playerSettings1.ai;
        public MCTS mcts;


        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            blockOperation = true;
        }

        private void Start()
        {
            StartCoroutine(StartGame());
        }

        private IEnumerator StartGame()
        {
            yield return null;
            curPlayer = Settings.Instance.startPlayerType;
            Debug.Log(curPlayer);

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Board board = this.NewSonWithComponent<Board>();
                    nineBoards[x, y] = board;
                    board.UI.cellSize = board.cellSize = cellSize;
                    board.transform.position = new Vector3(x * 3 * cellSize, y * 3 * cellSize) + BoardBottomLeft;
                    board.UI.lineColor = Color.gray;
                    board.ticTacToe.x = x;
                    board.ticTacToe.y = y;
                }
            }

            bigBoard = this.NewSonWithComponent<Board>();
            bigBoard.UI.cellSize = bigBoard.cellSize = cellSize * 3;
            bigBoard.transform.position = BoardBottomLeft;
            bigBoard.UI.lineColor = Color.white;

            EventManager.Instance.AfterFillCell += AfterFillCell;
            mcts = new MCTS(nineBoards, bigBoard, curPlayer, new CellPos());
            blockOperation = false;
        }

        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
        }

        private void OnDisable()
        {
            EventManager.Instance.AfterFillCell -= AfterFillCell;
        }

        private void AfterFillCell(CellPos cellPos)
        {
            if (Instance.gameover)
                return;
            mcts = mcts.GetNextMCTS(cellPos);
            playedCharNumber += 1;
        }

        public IEnumerable<CellPos> GetPosesCanChoose(int boardX, int boardY)
        {
            if (!nineBoards[boardX, boardY].ticTacToe.isOver)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        if (nineBoards[boardX, boardY].ticTacToe[x, y] == PlayerTypes.None)
                            yield return new CellPos(boardX, boardY, x, y);
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

        public void Restart()
        {
            SceneManager.LoadScene("UltimateTicTacToe");
        }
    }
}