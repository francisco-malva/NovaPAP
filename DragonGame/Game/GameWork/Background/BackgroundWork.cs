#region

using System;
using System.Drawing;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;

#endregion

namespace DuckDuckJump.Game.GameWork.Background;

public static class BackgroundWork
{
    private static short _target;

    private static float _progress;
    private static float _progressTarget;
    private static float _progressSpeed;

    public static void Reset()
    {
        SetTarget(-1);
        _progress = 0.0f;
        _progressSpeed = 0.0f;
        _progressTarget = 0.0f;
    }

    public static void Update()
    {
        _progress = Mathematics.SmoothDamp(_progress, _progressTarget, ref _progressSpeed, 0.25f,
            GameFlow.TimeStep);
    }

    public static void SetTarget(short newTarget)
    {
        if (newTarget != -1 && newTarget <= _target)
            return;

        _target = newTarget;
        _progressTarget = Math.Clamp((float)_target / Match.Info.PlatformCount, 0.0f, 1.0f);
    }

    public static void DrawMe()
    {
        Graphics.Draw(MatchAssets.Texture(MatchAssets.TextureIndex.Sky),
            null,
            Matrix3x2.CreateTranslation(0.0f, Mathematics.Lerp(-480.0f, 0.0f, _progress)), Color.White);
    }
}