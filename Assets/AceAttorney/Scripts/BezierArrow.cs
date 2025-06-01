using System.Collections.Generic;
using Lucky.Kits.Managers;
using Lucky.Kits.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace AceAttorney.Scripts
{
    public class BezierArrow : Singleton<BezierArrow>
    {
        public Transform arrowBodyPrefab;
        public int bodySegments = 8;
        public int controlOffset = 80;
        [Range(0, 1)] public float minBodySize = 0.2f;
        [Range(0, 1)] public float maxBodySize = 0.8f;
        private List<Transform> arrowBodies = new();
        private List<Image> arrowBodyImages = new();
        private RectTransform rectTransform;


        protected override void Awake()
        {
            base.Awake();

            rectTransform = GetComponent<RectTransform>();

            for (int i = 0; i < bodySegments; i++)
            {
                Transform body = Instantiate(arrowBodyPrefab, transform);
                arrowBodies.Add(body);
                arrowBodyImages.AddRange(body.GetComponentsInChildren<Image>());
            }

            Hide();
        }

        public void SetCurve(Vector2 from, Vector2 to)
        {
            BezierCurve bezier = new BezierCurve(from, to, from + Vector2.up * controlOffset);
            for (int i = 0; i < bodySegments; i++)
            {
                Transform body = arrowBodies[i];
                body.transform.localPosition = bezier.GetPoint((float)(i + 1) / bodySegments);
                body.transform.localScale = Vector3.one * MathUtils.ClampedMap(1f - Ease.SineEaseIn((float)(i + 1) / bodySegments), 0, 1, minBodySize, maxBodySize);
                body.transform.rotation = GetRotationFronBezierPoints(i, bezier);
            }
        }

        private Quaternion GetRotationFronBezierPoints(int i, BezierCurve bezier)
        {
            Vector3 prePos = bezier.GetPoint((float)i / bodySegments);
            Vector3 curPos = bezier.GetPoint((float)(i + 1) / bodySegments);
            // Vector3 dir = curPos - prePos;
            return MathUtils.LookAt(curPos - prePos);
        }


        public void Show()
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }


        public void Hide()
        {
            transform.localPosition = Vector2.one * 100000000;
        }

        public void SetColor(Color color)
        {
            foreach (var image in arrowBodyImages)
            {
                image.color = color;
            }
        }
    }
}