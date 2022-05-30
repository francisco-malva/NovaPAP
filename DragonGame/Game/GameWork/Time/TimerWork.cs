#region

using System.Drawing;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Game.Assets;

#endregion

namespace DuckDuckJump.Game.GameWork.Time;

internal static class TimerWork
{
    private const float DropShadowOffset = 3.0f;
    private static ushort _timeLeft;

    private static float _alpha;
    private static float _alphaVelocity;

    private static Size _stringSize;
    private static string _string;
    private static float TargetAlpha => Match.State == Match.MatchState.InGame ? 1.0f : 0.0f;

    public static bool Over => _timeLeft == 0;

    public static void Reset()
    {
        _timeLeft = Match.Info.TimeLeft;
        UpdateDisplay();
    }

    public static void UpdateMe()
    {
        _alpha = Mathematics.SmoothDamp(_alpha, TargetAlpha, ref _alphaVelocity, 0.15f, GameFlow.TimeStep);
        if (Match.State != Match.MatchState.InGame)
            return;

        if (_timeLeft > 0) TickDown();
    }

    private static void TickDown()
    {
        --_timeLeft;

        if (_timeLeft % 60 == 0) UpdateDisplay();
    }

    private static void UpdateDisplay()
    {
        _string = (_timeLeft / 60).ToString();
        _stringSize = MatchAssets.Font(MatchAssets.FontIndex.TimerFont).MeasureString(_string);
    }

    public static void DrawMe()
    {
        var intAlpha = (int) (_alpha * byte.MaxValue);

        var fgColor = Color.FromArgb(intAlpha, Color.Azure.R, Color.Azure.G, Color.Azure.B);
        var bgColor = Color.FromArgb(intAlpha, 0, 0, 0);

        var x = Graphics.Midpoint.X - _stringSize.Width / 2.0f;
        var y = 10.0f;


        MatchAssets.Font(MatchAssets.FontIndex.TimerFont).Draw(_string,
            Matrix3x2.CreateTranslation(x + DropShadowOffset, y + DropShadowOffset), bgColor);
        MatchAssets.Font(MatchAssets.FontIndex.TimerFont).Draw(_string,
            Matrix3x2.CreateTranslation(x, y), fgColor);
    }
}