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

namespace UltimateTicTacToe
{
    public class BoardUI : ManagedBehaviour
    {
        public float cellSize;
        private float LineWidth => BoardManager.Instance.lineWidth;
        public Color lineColor;
        public static int lineSortingOrder = 0;

        [ShowInInspector] public Cell[,] cells = new Cell[3, 3];
        public SpriteRenderer canClickHint;

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
            canClickHint.enabled = false;
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


        public void UpdateUI(bool canFill)
        {
            if (canClickHint != null)
                canClickHint.enabled = canFill;
        }


        public IEnumerator FillCoroutine(int x, int y, PlayerTypes cellType)
        {
            // 填入圈或者叉
            yield return FillCell(x, y, cellType);
        }


        public IEnumerator Rollback()
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

        public IEnumerator FillCell(int x, int y, PlayerTypes cellType)
        {
            var cell = cellType == PlayerTypes.Circle ? Cell.CreateCircle(this, cellSize) : Cell.CreateCross(this, cellSize);
            cells[x, y] = cell;

            Vector2 cellPos = transform.position + new Vector3(x, y) * cellSize;
            Vector2 cellCenterPos = cellPos + Vector2.one * (cellSize / 2f);
            cell.transform.position = cellCenterPos;
            yield return new WaitForSeconds(Cell.AnimationDuration);
        }
    }
}