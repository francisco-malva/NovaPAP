using System;

namespace DragonGame.Engine.Utilities
{
    internal static class Mathematics
    {
        public static float Lerp(float a, float b, float t)
        {
            return a * (1 - t) + b * MathF.Min(t, 1);
        }
    }
}