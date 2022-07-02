#region

using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;
using DuckDuckJump.Game.GameWork.Camera;
using DuckDuckJump.Game.GameWork.Rng;

#endregion

namespace DuckDuckJump.Game.GameWork.Platforming;

[StructLayout(LayoutKind.Sequential)]
internal struct Platform
{
    public enum BehaviorType
    {
        Static,
        SideToSide,
        Breaking,
        Max
    }

    public static readonly SizeF Extents = new(68 * 2, 14);
    public Vector2 Position;
    public BehaviorType Type;

    public RectangleF Body => new((PointF)Position, Extents);
    private float _time;
    private float _randomAngle;

    public void ResetMe()
    {
        OnScreen = true;
        _randomAngle = RandomWork.Next(0.0f, MathF.PI * 2.0f);
        _time = 0.0f;

        if (Type == BehaviorType.SideToSide) Position.X = SideToSideX;
    }

    public bool OnScreen { get; private set; }

    public void UpdateMe()
    {
        OnScreen = CameraWork.Camera.Bounds.IntersectsWith(Body);
        if (!OnScreen)
            return;

        switch (Type)
        {
            case BehaviorType.Static:
                break;
            case BehaviorType.SideToSide:
                Position.X = SideToSideX;
                break;
            case BehaviorType.Breaking:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _time += GameFlow.TimeStep;
    }

    private float SideToSideX => Mathematics.SmoothStep(0.0f, Graphics.LogicalSize.Width - Extents.Width,
        (MathF.Cos(_time * 0.65f + _randomAngle) + 1.0f) * 0.5f);

    private static readonly Color[] PlatformColors =
    {
        Color.LightGoldenrodYellow,
        Color.SlateBlue,
        Color.Firebrick
    };

    public void DrawMe()
    {
        if (!OnScreen)
            return;
        Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.Platform), null,
            Matrix3x2.CreateTranslation(Position),
            PlatformColors[(int)Type]);
    }
}