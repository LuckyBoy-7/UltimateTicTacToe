using DG.Tweening;
using Lucky.Framework;
using Lucky.Kits.Extensions;
using UnityEngine;

namespace UltimateTicTacToe.Scripts
{
    public abstract class Cell : ManagedBehaviour
    {
        public const float AnimationDuration = 0.15f;
        public float cellSize;
        public bool big;

        public static Cell CreateCross(ManagedBehaviour parent, float cellSize)
        {
            Cell cell = parent.NewSonWithComponent<Cross>();
            cell.cellSize = cellSize;

            return cell;
        }
        
        public static Cell CreateCircle(ManagedBehaviour parent, float cellSize)
        {
            Cell cell = parent.NewSonWithComponent<Circle>();
            cell.cellSize = cellSize;

            return cell;
        }

        public LineRenderer GetLine()
        {
            var line = this.NewSonWithComponent<LineRenderer>();
            float lineWidth = 0.1f;
            line.startWidth = line.endWidth = lineWidth;
            line.startColor = line.endColor = Color.white;
            line.numCapVertices = 90;
            // line.material = new Material("Default-Line");
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = false;
            line.sortingOrder = 1000;
            return line;
        }

        public abstract void Rollback();
    }
}