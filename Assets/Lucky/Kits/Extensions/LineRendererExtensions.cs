using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Lucky.Kits.Utilities;
using UnityEngine;

namespace Lucky.Kits.Extensions
{
    public static class LineRendererExtensions
    {
        public static TweenerCore<Vector2, Vector2, VectorOptions> DrawLineFromTo(this LineRenderer orig, Vector2 from, Vector2 to, Vector2 start, float duration)
        {
            orig.SetPosition(0, from);
            orig.SetPosition(1, start);

            return DOTween.To(() => start, (value) => start = value, to, duration).OnUpdate(() => { orig.SetPosition(1, start); });
        }

        /// <summary>
        /// 逆时针画圈
        /// </summary>
        public static TweenerCore<float, float, FloatOptions> DrawCircleFromTo(this LineRenderer orig, float length, float fromAngle, float toAngle, float startAngle,
            float duration, int resolution = 60)
        {
            fromAngle = MathUtils.Mod(fromAngle, 360);
            toAngle = MathUtils.Mod(toAngle, 360);
            startAngle = MathUtils.Mod(startAngle, 360);
            if (toAngle < fromAngle)
                toAngle += 360;


            orig.positionCount = resolution + 1;
            return DOTween.To(() => startAngle, (value) => startAngle = value, toAngle, duration).OnUpdate(() =>
            {
                float f = fromAngle;
                float t = startAngle;
                if (fromAngle > startAngle)
                    (f, t) = (t, f);
                for (int i = 0; i <= resolution; i++)
                {
                    float angle = Mathf.Lerp(f, t, i / (float)resolution);
                    orig.SetPosition(i, MathUtils.AngleToVector(angle, length));
                }
            });
        }
    }
}