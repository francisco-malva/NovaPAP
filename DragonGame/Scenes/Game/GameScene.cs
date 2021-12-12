using System;
using System.IO;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;
using DuckDuckJump.Scenes.Game.Gameplay;
using DuckDuckJump.Scenes.Game.Gameplay.Announcer;
using DuckDuckJump.Scenes.Game.Input;
using DuckDuckJump.Scenes.Game.Local;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

namespace DuckDuckJump.Scenes.Game;

internal abstract class GameScene : Scene
{
    public const int GameBorder = 32;
    public const int Width = 640;
    public const int Height = 480;

    public const byte GetReadyTime = 120;
    public const byte RoundEndTime = 120;

    public static Replay CurrentReplay;

    private readonly Announcer _announcer;

    private readonly Texture _gameBorder;

    private readonly Music _music;

    protected readonly GameField P1Field;
    protected readonly GameField P2Field;

    protected readonly DeterministicRandom Random;

    private GameState _state = GameState.GetReady;
    private byte _stateTimer;
    private Winner _winner;

    protected GameScene(GameInfo info)
    {
        FrameCount = 0;

        _gameBorder = Engine.Game.Instance.TextureManager["UI/game-border"];
        Random = new DeterministicRandom();
        Random.Setup(info.RandomSeed);

        P1Field = new GameField(Random, this, false, info);
        P2Field = new GameField(Random, this, true, info);

        _music = Engine.Game.Instance.MusicManager["mus-test"];
        _music.Play();

        _announcer = new Announcer();

        ChangeState(GameState.GetReady);
    }

    protected ulong FrameCount { get; private set; }

    protected bool CanPause => _state == GameState.InGame;

    public Winner WinningSide
    {
        get
        {
            if (P1Field.PlatformManager.GetClimbingProgress() > P2Field.PlatformManager.GetClimbingProgress())
                return Winner.P1;
            return P2Field.PlatformManager.GetClimbingProgress() > P1Field.PlatformManager.GetClimbingProgress()
                ? Winner.P2
                : Winner.Both;
        }
    }

    protected void SetRoundsToWin(byte roundsToWin)
    {
        P1Field.Scoreboard.RoundsToWin = roundsToWin;
        P2Field.Scoreboard.RoundsToWin = roundsToWin;
    }

    private void EndRound(Winner winner)
    {
        _winner = winner;
        ChangeState(GameState.PlayerWon);
    }

    protected void SimulateAndDraw(GameInput p1Input, GameInput p2Input)
    {
        SimulateFrame(p1Input, p2Input);
        Draw();
        ++FrameCount;
    }

    protected static void ProcessInput(ref GameInput input,
        SDL.SDL_Scancode leftScanCode,
        SDL.SDL_Scancode rightScanCode, SDL.SDL_Scancode specialScanCode)
    {
        input = GameInput.None;

        input |= Keyboard.KeyHeld(leftScanCode) ? GameInput.Left : GameInput.None;
        input |= Keyboard.KeyHeld(rightScanCode) ? GameInput.Right : GameInput.None;
        input |= Keyboard.KeyHeld(specialScanCode) ? GameInput.Special : GameInput.None;
    }

    private void DecideWinner()
    {
        switch (_winner)
        {
            case Winner.Both:
                P1Field.WinRound(true);
                P2Field.WinRound(true);
                _announcer.Say(AnnouncementType.Draw);
                break;
            case Winner.P1:
                P1Field.WinRound();
                P2Field.LoseRound();
                _announcer.Say(AnnouncementType.P1Wins);
                break;
            case Winner.P2:
                P2Field.WinRound();
                P1Field.LoseRound();
                _announcer.Say(AnnouncementType.P2Wins);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected void ChangeState(GameState state)
    {
        _stateTimer = 0;
        _state = state;

        switch (_state)
        {
            case GameState.GetReady:
                //Bass.ChannelSetAttribute(_musicChannel, ChannelAttribute.Volume, 0.25f);
                _announcer.Say(AnnouncementType.GetReady);
                P1Field.GetReady();
                P2Field.GetReady();
                break;
            case GameState.InGame:
                //Bass.ChannelSetAttribute(_musicChannel, ChannelAttribute.Volume, 0.50f);
                _announcer.Say(AnnouncementType.Go);
                P1Field.BeginRound();
                P2Field.BeginRound();
                break;
            case GameState.PlayerWon:
                //Bass.ChannelSetAttribute(_musicChannel, ChannelAttribute.Volume, 0.25f);
                DecideWinner();
                break;
            case GameState.GameOver:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state));
        }
    }

    private void GetReadyUpdate(GameInput p1Input, GameInput p2Input)
    {
        if (_stateTimer == GetReadyTime)
        {
            ChangeState(GameState.InGame);
            return;
        }

        P1Field.Update(p1Input);
        P2Field.Update(p2Input);
    }

    private void InGameUpdate(GameInput p1Input, GameInput p2Input)
    {
        P1Field.Update(p1Input);
        P2Field.Update(p2Input);

        if (P1Field.PlayerWonRound && P2Field.PlayerWonRound)
            EndRound(Winner.Both);
        else if (P1Field.PlayerWonRound)
            EndRound(Winner.P1);
        else if (P2Field.PlayerWonRound) EndRound(Winner.P2);
    }

    private void PlayerWonUpdate(GameInput p1Input, GameInput p2Input)
    {
        P1Field.Update(p1Input);
        P2Field.Update(p2Input);

        if (_stateTimer == RoundEndTime)
        {
            if (P1Field.PlayerWonGame || P2Field.PlayerWonGame)
                ChangeState(GameState.GameOver);
            else
                ChangeState(GameState.GetReady);
        }
    }

    private void SimulateFrame(GameInput p1Input, GameInput p2Input)
    {
        switch (_state)
        {
            case GameState.GetReady:
                GetReadyUpdate(p1Input, p2Input);
                break;
            case GameState.InGame:
                InGameUpdate(p1Input, p2Input);
                break;
            case GameState.PlayerWon:
                PlayerWonUpdate(p1Input, p2Input);
                break;
            case GameState.GameOver:
                using (var file = File.Create("replay.rpy"))
                {
                    using var writer = new BinaryWriter(file);
                    CurrentReplay.Save(writer);
                }

                Engine.Game.Instance.SceneManager.Set(new MainMenuScene());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ++_stateTimer;
    }

    protected void Draw(Action<Renderer> postRenderCallback = null)
    {
        var renderer = Engine.Game.Instance.Renderer;

        P1Field.Draw();
        P2Field.Draw();

        var p1Dest = new Rectangle(GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);
        var p2Dest = new Rectangle(Width / 2 + GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);

        renderer.SetDrawColor(Color.Black);
        renderer.Clear();
        renderer.Copy(_gameBorder, null, null);
        renderer.Copy(P1Field.OutputTexture, null, p1Dest);
        renderer.Copy(P2Field.OutputTexture, null, p2Dest);

        postRenderCallback?.Invoke(renderer);

        renderer.Present();
    }

    protected override void OnUnload()
    {
        OnGameEnd();
        P1Field.Dispose();
        P2Field.Dispose();
    }

    protected virtual void OnGameEnd()
    {
        Music.Halt();
    }
}