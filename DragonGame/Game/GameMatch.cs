#region

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using Common.Utilities;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game;

internal static partial class Match
{
    public const int PlayerCount = 2;

    private static GameInfo _info;
    private static float _fade;
    private static Winner _winner;
    private static byte _currentRound;

    private static Winner _matchWinner;

    public static MatchState State { get; private set; }
    public static Winner MatchWinner => _matchWinner;

    public static bool IsOver { get; private set; }

    public static void Save(Stream stream)
    {
        _info.Save(stream);

        stream.Write(State);
        stream.Write(_fade);
        stream.Write(_winner);
        stream.Write(_currentRound);
        stream.Write(_matchWinner);
        stream.Write(IsOver);

        CameraWork.SaveMe(stream);
        BannerWork.SaveMe(stream);
        PlatformWork.SaveMe(stream);
        RandomWork.SaveMe(stream);
        FinishLineWork.SaveMe(stream);
        PlayerWork.SaveMe(stream);
        ScoreWork.Save(stream);
        TimerWork.SaveMe(stream);
        SoundEffectWork.SaveMe(stream);
    }

    public static void Load(Stream stream)
    {
        _info.Load(stream);

        State = stream.Read<MatchState>();
        _fade = stream.Read<float>();
        _winner = stream.Read<Winner>();
        _currentRound = stream.Read<byte>();
        _matchWinner = stream.Read<Winner>();
        IsOver = stream.Read<bool>();

        CameraWork.LoadMe(stream);
        BannerWork.LoadMe(stream);
        PlatformWork.LoadMe(stream);
        RandomWork.LoadMe(stream);
        FinishLineWork.LoadMe(stream);
        PlayerWork.LoadMe(stream);
        ScoreWork.Load(stream);
        TimerWork.LoadMe(stream);

        SoundEffectWork.LoadMe(stream);
        SoundEffectWork.UpdateMe();
    }


    public static void Initialize(GameInfo info)
    {
        IsOver = false;
        _info = info;
        _currentRound = 0;
        _matchWinner = 0;

        _fade = 1.0f;

        ScoreWork.Reset();
        RandomWork.Reset();

        SetState(_info.BeginMessageIndex != BannerWork.MessageIndex.NoBanner
            ? MatchState.BeginningMessage
            : MatchState.GetReady);
    }

    private static void SetState(MatchState state)
    {
        State = state;

        switch (State)
        {
            case MatchState.GetReady:
                _winner = Winner.None;
                _currentRound++;
                BackgroundWork.Reset();
                BannerWork.Reset();
                CameraWork.Reset();
                PlayerWork.Reset();
                PlatformWork.Reset();
                TimerWork.Reset();
                FinishLineWork.Reset();

                if (_info.ScoreCount > 1)
                    BannerWork.SetMessage(BannerWork.MessageIndex.Round);
                break;
            case MatchState.InGame:
                break;
            case MatchState.Winner:
                if (_winner != Winner.Draw && _winner != Winner.None)
                {
                    var winner = (int) _winner;
                    ScoreWork.IncreaseScore(winner);

                    ref var player = ref PlayerWork.Get(winner);

                    CenterCameraOnPlayer(ref player);
                }

                if (ScoreWork.GetWinner(out _matchWinner))
                    SetState(MatchState.Over);
                else
                    BannerWork.SetMessage(_winner != Winner.Draw
                        ? BannerWork.MessageIndex.Wins
                        : BannerWork.MessageIndex.Draw);
                break;
            case MatchState.NotInitialized:
                break;
            case MatchState.Over:
                BannerWork.SetMessage(BannerWork.MessageIndex.GameSet);
                break;
            case MatchState.TimeUp:
                BannerWork.SetMessage(BannerWork.MessageIndex.TimeUp);
                break;
            case MatchState.BeginningMessage:
                BannerWork.SetMessage(_info.BeginMessageIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void CenterCameraOnPlayer(ref PlayerWork.Player player)
    {
        CameraWork.Target =
            new Vector2(player.Position.X + player.PushBox.Width / 2.0f - Graphics.Midpoint.X,
                player.Position.Y + player.PushBox.Height / 2.0f - Graphics.Midpoint.Y);
    }

    public static void Update(Span<GameInput> inputs)
    {
        if (State == MatchState.NotInitialized)
            return;

        SoundEffectWork.Reset();

        switch (State)
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
                if (TimerWork.Over)
                {
                    SetState(MatchState.TimeUp);
                }
                else
                {
                    DecideWinner();
                    if (_winner >= 0) SetState(MatchState.Winner);
                }

                break;
            case MatchState.TimeUp:
                if (BannerWork.IsDone())
                {
                    if (PlayerWork.First.LastJumpedPlatform == PlayerWork.Last.LastJumpedPlatform)
                    {
                        PlayerWork.First.Lost = true;
                        PlayerWork.Last.Lost = true;
                    }
                    else if (PlayerWork.First.LastJumpedPlatform > PlayerWork.Last.LastJumpedPlatform)
                    {
                        PlayerWork.Last.Lost = true;
                    }
                    else
                    {
                        PlayerWork.First.Lost = true;
                    }

                    DecideWinner();
                    SetState(MatchState.Winner);
                }

                break;
            case MatchState.Winner:
                if (BannerWork.IsDone())
                {
                    _fade += 0.05f;
                    if (_fade >= 1.0f) SetState(MatchState.GetReady);
                }

                break;
            case MatchState.NotInitialized:
                break;
            case MatchState.Over:

                if (BannerWork.IsDone())
                {
                    _fade += 0.005f;
                    if (_fade >= 1.0f)
                    {
                        _fade = 1.0f;
                        IsOver = true;
                        SetState(MatchState.NotInitialized);
                    }

                    Audio.MusicFade = 1.0f - _fade;
                }


                break;
            case MatchState.BeginningMessage:
                if (BannerWork.IsDone()) SetState(MatchState.GetReady);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        BackgroundWork.Update();
        PlatformWork.Update();
        PlayerWork.UpdateUs(inputs);
        CameraWork.UpdateMe();
        BannerWork.Update();
        TimerWork.UpdateMe();
        FinishLineWork.UpdateMe();
        SoundEffectWork.UpdateMe();
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
        if (State == MatchState.NotInitialized)
            return;

        Graphics.Camera = null;
        BackgroundWork.DrawMe();
        Graphics.Camera = CameraWork.Camera;

        PlayerWork.DrawUs();
        PlatformWork.DrawUs();
        FinishLineWork.DrawMe();

        Graphics.Camera = null;
        if ((_info.GameFlags & GameInfo.Flags.Exhibition) == 0) DrawGui();
        DrawFade();

        BannerWork.DrawMe();
    }

    private static void DrawFade()
    {
        Graphics.Draw(Texture.White, null,
            Matrix3x2.CreateScale(Graphics.LogicalSize.Width, Graphics.LogicalSize.Height),
            Color.FromArgb((int) (Math.Min(1.0f, _fade) * byte.MaxValue), 0, 0, 0));
    }

    private static void DrawGui()
    {
        TimerWork.DrawMe();
        ScoreWork.DrawMe();
    }

    internal enum MatchState : byte
    {
        NotInitialized,
        GetReady,
        InGame,
        TimeUp,
        Winner,
        Over,
        BeginningMessage
    }

    public enum Winner : sbyte
    {
        None = -1,
        P1,
        P2,
        Draw
    }
}