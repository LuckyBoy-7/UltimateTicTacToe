using Lucky.Kits.Managers;
using UnityEngine;

namespace AceAttorney.Scripts
{
    public class InsertHint : Singleton<InsertHint>
    {
        private LineRenderer lineRenderer;

        protected override void Awake()
        {
            base.Awake();

            lineRenderer = GetComponent<LineRenderer>();

            Hide();
        }

        public void SetLine(Vector2 from, Vector2 to)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, from);
            lineRenderer.SetPosition(1, to);
        }

        public void SetColor(Color color)
        {
            lineRenderer.startColor = lineRenderer.endColor = color;
        }

        public void Show()
        {
            lineRenderer.enabled = true;
        }


        public void Hide()
        {
            lineRenderer.enabled = false;
        }
    }
}