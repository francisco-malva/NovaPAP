#region

using System.Drawing;
using System.Numerics;

#endregion

namespace Common.Utilities;

public static class Mathematics
{
    public static float Next(this Random random, float minimum, float maximum)
    {
        return Lerp(minimum, maximum, random.NextSingle());
    }

    // Gradually changes a value towards a desired goal over time.
    public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime,
        float deltaTime, float maxSpeed = float.PositiveInfinity)
    {
        // Based on Game Programming Gems 4 Chapter 1.10
        smoothTime = Math.Max(0.0001F, smoothTime);
        var omega = 2F / smoothTime;

        var x = omega * deltaTime;
        var exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
        var change = current - target;
        var originalTo = target;

        // Clamp maximum speed
        var maxChange = maxSpeed * smoothTime;
        change = Math.Clamp(change, -maxChange, maxChange);
        target = current - change;

        var temp = (currentVelocity + omega * change) * deltaTime;
        currentVelocity = (currentVelocity - omega * temp) * exp;
        var output = target + (change + temp) * exp;

        // Prevent overshooting
        if (originalTo - current > 0.0F != output > originalTo) return output;

        output = originalTo;
        currentVelocity = (output - originalTo) / deltaTime;

        return output;
    }

    public static float Map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public static float SmoothStep(float a, float b, float t)
    {
        return Lerp(a, b, SmoothStep(t));
    }

    private static float SmoothStep(float x)
    {
        x = Math.Clamp(x, 0.0f, 1.0f);
        return x * x * (3.0f - 2.0f * x);
    }

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

    public static float Eerp(float a, float b, float t)
    {
        return MathF.Pow(a, 1 - t) * MathF.Pow(b, t);
    }

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
    }

    public static Vector2 QuadraticBezier(Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        var mid1 = Vector2.Lerp(p1, p2, t);
        var mid2 = Vector2.Lerp(p2, p3, t);

        return Vector2.Lerp(mid1, mid2, t);
    }

    public static Vector2 CubicBezier(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float t)
    {
        var mid1 = Vector2.Lerp(p1, p2, t);
        var mid2 = Vector2.Lerp(p2, p3, t);
        var mid3 = Vector2.Lerp(p3, p4, t);

        var end1 = Vector2.Lerp(mid1, mid2, t);
        var end2 = Vector2.Lerp(mid2, mid3, t);

        return Vector2.Lerp(end1, end2, t);
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
        var r = (byte)Lerp(from.R, to.R, t);
        var g = (byte)Lerp(from.G, to.G, t);
        var b = (byte)Lerp(from.B, to.B, t);
        var a = (byte)Lerp(from.A, to.A, t);
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