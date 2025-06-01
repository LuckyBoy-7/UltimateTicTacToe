using Lucky.Kits.Utilities;
using UnityEngine;

namespace Lucky.Kits.Generator.Noise
{
    public class EasyNoiseGenerator : NoiseGenerator
    {
        protected override string FileName => "EasyNoise.png";

        protected override Color GetPixelColor(int x, int y) => Color.white * RandomUtils.NextFloat(1);
    }
}