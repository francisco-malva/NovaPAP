using System;
using Engine.Wrappers.SDL2;

namespace DragonGame.Engine.Utilities
{
    internal static class Mathematics
    {
        public static float Lerp(float a, float b, float t)
        {
            var clampedT = Math.Clamp(t, 0.0f, 1.0f);
            return a * (1.0f - clampedT) + b * clampedT;
        }

        public static Color Lerp(Color from, Color to, float t)
        {
            var r = (byte)Lerp(from.R, to.R, t);
            var g = (byte)Lerp(from.G, to.G, t);
            var b = (byte)Lerp(from.B, to.B, t);
            return new Color(r, g, b, 255);
        }
    }
}