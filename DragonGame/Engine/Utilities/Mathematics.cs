using System;
using System.Drawing;
using DuckDuckJump.Game.Gameplay.Players.AI;

namespace DuckDuckJump.Engine.Utilities;

internal static class Mathematics
{
    /// <summary>
    ///     Linearly interpolate a set of values.
    /// </summary>
    /// <param name="a">The beginning value.</param>
    /// <param name="b">The ending value.</param>
    /// <param name="t">The progress of the interpolation.</param>
    /// <returns>The interpolated value.</returns>
    public static float Lerp(float a, float b, float t)
    {
        var clampedT = Math.Clamp(t, 0.0f, 1.0f);
        return a * (1.0f - clampedT) + b * clampedT;
    }

    /// <summary>
    ///     Interpolate a color.
    /// </summary>
    /// <param name="from">The beginning color.</param>
    /// <param name="to">The ending color.</param>
    /// <param name="t">The interpolated value.</param>
    /// <returns>The interpolated color.</returns>
    public static Color Lerp(Color from, Color to, float t)
    {
        var r = (byte) Lerp(from.R, to.R, t);
        var g = (byte) Lerp(from.G, to.G, t);
        var b = (byte) Lerp(from.B, to.B, t);
        var a = (byte) Lerp(from.A, to.A, t);
        return Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    ///     Prevent a value from being smaller than min and bigger than max.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <typeparam name="T">The type of the value to operate on.</typeparam>
    /// <returns>The clamped value.</returns>
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            value = min;
        else if (value.CompareTo(max) > 0) value = max;
        return value;
    }
}