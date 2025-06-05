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


namespace TicTacToe
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
        public float lineWidth;

        public Board curBoard;
        public PlayerTypes curPlayer = PlayerTypes.Cross;

        public Vector3 BoardBottomLeft => transform.position - new Vector3(1, 1) * (cellSize * 4.5f);

        public bool gameover;

        public bool useAI;

        public bool blockOperation;


        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            
            curBoard = this.NewSonWithComponent<Board>();
            curBoard.UI.cellSize = curBoard.cellSize = cellSize * 3;
            curBoard.transform.position = BoardBottomLeft;
            curBoard.UI.lineColor = Color.white;
        }



        protected override void Update()
        {
            base.Update();
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("TicTacToe");
            }
        }
    }
}