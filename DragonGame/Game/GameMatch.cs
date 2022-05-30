#region

using System;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.GameWork.Background;
using DuckDuckJump.Game.GameWork.Banner;
using DuckDuckJump.Game.GameWork.Camera;
using DuckDuckJump.Game.GameWork.FinishLine;
using DuckDuckJump.Game.GameWork.Items;
using DuckDuckJump.Game.GameWork.Platforming;
using DuckDuckJump.Game.GameWork.Players;
using DuckDuckJump.Game.GameWork.Rng;
using DuckDuckJump.Game.GameWork.Scoring;
using DuckDuckJump.Game.GameWork.Sound;
using DuckDuckJump.Game.GameWork.Time;
using DuckDuckJump.Game.Input;

#endregion

namespace DuckDuckJump.Game;

internal static class Match
{
    public const int PlayerCount = 2;

    public static GameInfo Info;
    public static float Fade;
    public static MatchWinner RoundWinner, SetWinner;
    public static byte CurrentRound;

    public static ulong UpdateFrameCount;


    public static MatchState State { get; private set; }

    public static bool IsOver { get; private set; }

    public static void Initialize(GameInfo info)
    {
        IsOver = false;
        Info = info;
        CurrentRound = 0;
        UpdateFrameCount = 0;
        Fade = 1.0f;

        ScoreWork.Reset();
        RandomWork.Reset();

        SetState(Info.BeginMessageIndex != BannerWork.MessageIndex.NoBanner
            ? MatchState.BeginningMessage
            : MatchState.GetReady);
    }

    private static void SetState(MatchState state)
    {
        State = state;

        switch (State)
        {
            case MatchState.GetReady:
                OnGetReady();
                break;
            case MatchState.InGame:
                break;
            case MatchState.Winner:
                OnWinner();
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
                BannerWork.SetMessage(Info.BeginMessageIndex);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void OnWinner()
    {
        if (RoundWinner != MatchWinner.Draw && RoundWinner != MatchWinner.None)
        {
            var winner = (int) RoundWinner;
            ScoreWork.IncreaseScore(winner);

            var player = PlayerWork.Get(winner);

            CenterCameraOnPlayer(ref player);
        }

        if (ScoreWork.GetWinner(out SetWinner))
            SetState(MatchState.Over);
        else
            BannerWork.SetMessage(RoundWinner != MatchWinner.Draw
                ? BannerWork.MessageIndex.Wins
                : BannerWork.MessageIndex.Draw);
    }

    private static void OnGetReady()
    {
        RoundWinner = MatchWinner.None;
        CurrentRound++;

        BackgroundWork.Reset();
        BannerWork.Reset();
        CameraWork.Reset();
        PlayerWork.Reset();
        PlatformWork.Reset();
        ItemWork.Reset();
        TimerWork.Reset();
        FinishLineWork.Reset();

        if (Info.ScoreCount > 1)
            BannerWork.SetMessage(BannerWork.MessageIndex.Round);
    }

    private static void CenterCameraOnPlayer(ref Player player)
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
                UpdateGetReady();
                break;
            case MatchState.InGame:
                UpdateInGame();
                break;
            case MatchState.TimeUp:
                UpdateTimeUp();
                break;
            case MatchState.Winner:
                UpdateWinner();
                break;
            case MatchState.NotInitialized:
                break;
            case MatchState.Over:
                UpdateOver();
                break;
            case MatchState.BeginningMessage:
                UpdateBeginningMessage();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        BackgroundWork.Update();
        PlatformWork.Update();
        ItemWork.Update(inputs);
        PlayerWork.UpdateUs(inputs);
        CameraWork.UpdateMe();
        BannerWork.Update();
        TimerWork.UpdateMe();
        FinishLineWork.UpdateMe();
        SoundEffectWork.UpdateMe();

        ++UpdateFrameCount;
    }

    private static void UpdateBeginningMessage()
    {
        if (BannerWork.IsDone()) SetState(MatchState.GetReady);
    }

    private static void UpdateOver()
    {
        if (!BannerWork.IsDone()) return;

        Fade += 0.05f;
        
        if (!(Fade >= 1.0f)) return;

        Fade = 1.0f;
        IsOver = true;
        SetState(MatchState.NotInitialized);
    }

    private static void UpdateWinner()
    {
        if (!BannerWork.IsDone()) return;

        Fade += 0.05f;
        if (Fade >= 1.0f) SetState(MatchState.GetReady);
    }

    private static void UpdateTimeUp()
    {
        if (!BannerWork.IsDone()) return;

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

    private static void UpdateInGame()
    {
        if (TimerWork.Over)
        {
            SetState(MatchState.TimeUp);
        }
        else
        {
            DecideWinner();
            if (RoundWinner >= 0) SetState(MatchState.Winner);
        }
    }

    private static void UpdateGetReady()
    {
        if (!BannerWork.IsDone()) return;
        Fade -= 0.05f;
        if (!(Fade <= 0.0f)) return;

        Fade = 0.0f;
        SetState(MatchState.InGame);
    }

    private static void DecideWinner()
    {
        switch (PlayerWork.First.Lost)
        {
            case true when PlayerWork.Last.Lost:
                RoundWinner = MatchWinner.Draw;
                break;
            case true:
                RoundWinner = MatchWinner.P2;
                break;
            default:
            {
                if (PlayerWork.Last.Lost) RoundWinner = MatchWinner.P1;
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
        ItemWork.DrawMe();
        FinishLineWork.DrawMe();

        Graphics.Camera = null;
        if ((Info.GameFlags & GameInfo.Flags.Exhibition) == 0) DrawGui();
        DrawFade();

        BannerWork.DrawMe();
    }

    private static void DrawFade()
    {
        Graphics.Draw(Texture.White, null,
            Matrix3x2.CreateScale(Graphics.LogicalSize.Width, Graphics.LogicalSize.Height),
            Color.FromArgb((int) (Math.Min(1.0f, Fade) * byte.MaxValue), 0, 0, 0));
    }

    private static void DrawGui()
    {
        TimerWork.DrawMe();
        ScoreWork.DrawMe();
        ItemWork.DrawGui();
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
}