using System;
using DG.Tweening;
using Lucky.Framework;
using Lucky.Kits.Extensions;
using UnityEngine;

namespace TicTacToe
{

    public class Cross : Cell
    {
        private LineRenderer line0;
        private LineRenderer line1;

        private void Start()
        {
            line0 = GetLine();
            line0.transform.localPosition = Vector3.zero;
            float halfSize = cellSize / 2f * 0.6f;
            Vector2 start = new Vector2(-halfSize, halfSize);
            Vector2 end = new Vector2(halfSize, -halfSize);
            line0.DrawLineFromTo(start, end, start, AnimationDuration);

            line1 = GetLine();
            line1.transform.localPosition = Vector3.zero;
            start = new Vector2(halfSize, halfSize);
            end = new Vector2(-halfSize, -halfSize);
            line1.DrawLineFromTo(start, end, start, AnimationDuration);
        }

        public override void Rollback()
        {
            float halfSize = cellSize / 2f * 0.6f;
            Vector2 start = new Vector2(-halfSize, halfSize);
            Vector2 end = new Vector2(halfSize, -halfSize);
            line0.DrawLineFromTo(start, start, end, AnimationDuration).onComplete += () => line0.enabled = false;;

            start = new Vector2(halfSize, halfSize);
            end = new Vector2(-halfSize, -halfSize);
            line1.DrawLineFromTo(start, start, end, AnimationDuration).onComplete += () => line1.enabled = false;;
        }
    }
}