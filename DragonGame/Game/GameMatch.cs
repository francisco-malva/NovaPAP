#region

using System;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    public const int PlayerCount = 2;
    private static MatchState _state;
    private static GameInfo _info;

    private static float _fade;
    private static Winner _winner;

    private static byte _currentRound;

    private static readonly string[] WinMessages =
    {
        "P1 Wins",
        "P2 Wins",
        "Draw"
    };

    private static readonly Color[] WinColors =
    {
        Color.Goldenrod,
        Color.Goldenrod,
        Color.DodgerBlue
    };

    private static int _matchWinner;

    public static bool IsOver { get; private set; }
    public static int MatchWinner => _matchWinner;


    public static void Initialize(GameInfo info)
    {
        IsOver = false;
        _info = info;
        _currentRound = 0;
        _matchWinner = 0;

        _fade = 1.0f;

        ScoreWork.Reset();
        RandomWork.Reset();

        SetState(MatchState.GetReady);
    }

    private static void SetState(MatchState state)
    {
        _state = state;

        switch (_state)
        {
            case MatchState.GetReady:
                _winner = Winner.None;
                _currentRound++;
                BannerWork.Reset();
                CameraWork.Reset();
                PlayerWork.Reset();
                PlatformWork.Reset();
                TimerWork.Reset();

                if (_info.ScoreCount > 1)
                    BannerWork.SetMessage($"Round {_currentRound}", Color.White, 1.0f);
                break;
            case MatchState.InGame:
                break;
            case MatchState.Winner:
                if (_winner != Winner.Draw && _winner != Winner.None)
                {
                    var winner = (int) _winner;
                    ScoreWork.IncreaseScore(winner);

                    ref var player = ref PlayerWork.Get(winner);
                    CameraWork.Target =
                        new Vector2(player.Position.X + player.PushBox.Width / 2.0f - Graphics.Midpoint.X,
                            player.Position.Y + player.PushBox.Height / 2.0f - Graphics.Midpoint.Y);
                }

                if (ScoreWork.GetWinner(out _matchWinner))
                    // ReSharper disable once TailRecursiveCall
                    SetState(MatchState.Over);
                else
                    BannerWork.SetMessage(WinMessages[(int) _winner], WinColors[(int) _winner], 1.0f);
                break;
            case MatchState.NotInitialized:
            case MatchState.Over:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void Update(Span<GameInput> inputs)
    {
        if (_state == MatchState.NotInitialized)
            return;
        switch (_state)
        {
            case MatchState.GetReady:
                if (BannerWork.IsDone())
                {
                    _fade -= 0.05f;
                    if (_fade <= 0.0f)
                    {
                        _fade = 0.0f;
                        SetState(MatchState.InGame);
                    }
                }

                break;
            case MatchState.InGame:
                DecideWinner();

                if (_winner >= 0) SetState(MatchState.Winner);
                break;
            case MatchState.Winner:
                if (BannerWork.IsDone())
                {
                    _fade += 0.05f;
                    if (_fade >= 1.0f) SetState(MatchState.GetReady);
                }

                break;
            case MatchState.NotInitialized:
            case MatchState.Over:
                _fade -= 0.01f;
                if (_fade <= 0.0f)
                {
                    _fade = 0.0f;
                    SetState(MatchState.NotInitialized);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        PlatformWork.Update();
        PlayerWork.UpdateUs(inputs);
        CameraWork.UpdateMe();
        BannerWork.Update();
        TimerWork.UpdateMe();
    }

    private static void DecideWinner()
    {
        switch (PlayerWork.First.Lost)
        {
            case true when PlayerWork.Last.Lost:
                _winner = Winner.Draw;
                break;
            case true:
                _winner = Winner.P2;
                break;
            default:
            {
                if (PlayerWork.Last.Lost) _winner = Winner.P1;
                break;
            }
        }
    }

    public static void Draw()
    {
        if (_state == MatchState.NotInitialized)
            return;

        Graphics.Camera = null;
        Graphics.Draw(Assets.Texture(Assets.TextureIndex.Sky), null, Matrix3x2.CreateScale(2.0f), Color.White);
        Graphics.Camera = CameraWork.Camera;

        PlayerWork.DrawUs();
        PlatformWork.DrawUs();

        if (!(_fade >= 0.0f)) return;

        Graphics.Camera = null;

        TimerWork.DrawMe();
        Graphics.Draw(Texture.White, null,
            Matrix3x2.CreateScale(Graphics.LogicalSize.Width, Graphics.LogicalSize.Height),
            Color.FromArgb((int) (Math.Min(1.0f, _fade) * byte.MaxValue), 0, 0, 0));
        BannerWork.DrawMe();
    }

    private enum MatchState : byte
    {
        NotInitialized,
        GetReady,
        InGame,
        Winner,
        Over
    }

    private enum Winner : sbyte
    {
        None = -1,
        P1,
        P2,
        Draw
    }
}