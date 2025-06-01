using System;
using DG.Tweening;
using Lucky.Framework;
using Lucky.Kits.Extensions;
using UnityEngine;

namespace UltimateTicTacToe.Scripts
{
    public class Circle : Cell
    {
        private LineRenderer line;

        private void Start()
        {
            line = GetLine();
            line.transform.localPosition = Vector3.zero;
            float halfSize = cellSize / 2f * 0.6f;
            float startAngle = 30;
            line.DrawCircleFromTo(halfSize, startAngle, startAngle - 0.01f, startAngle, AnimationDuration, 30);
        }

        public override void Rollback()
        {
            float halfSize = cellSize / 2f * 0.6f;
            float startAngle = 30;
            line.DrawCircleFromTo(halfSize, startAngle, startAngle - 0.01f, startAngle + 360, AnimationDuration, 30).onComplete += () => line.enabled = false;
        }
    }
}