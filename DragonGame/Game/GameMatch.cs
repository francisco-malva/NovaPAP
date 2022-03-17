using System;
using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game.Gameplay;
using DuckDuckJump.Game.Gameplay.Announcer;
using DuckDuckJump.Game.Gameplay.Resources;
using DuckDuckJump.Game.Input;
using DuckDuckJump.Game.Input.InputHandlers;
using SDL2;

namespace DuckDuckJump.Game;

internal class GameMatch
{
    public const int GameBorder = 64;
    public const int Width = 640;
    public const int Height = 480;

    public const byte GetReadyTime = 120;
    public const byte RoundEndTime = 120;

    private readonly Announcer _announcer;

    private readonly Texture? _gameBorder;
    private readonly Texture _outputTexture;

    private readonly GameField _p1Field;
    private readonly GameField _p2Field;

    private readonly Renderer _renderer;

    private GameState _state = GameState.GetReady;
    private byte _stateTimer;

    public Action? OnGameEnded;

    public GameMatch(GameInfo info, GameplayResources resources)
    {
        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        _renderer = GameContext.Instance.Renderer;

        _outputTexture = resources.OutputTexture;

        _gameBorder = resources.GameBorder;
        var random = new Random(info.RandomSeed);

        _p1Field = new GameField(random, this, false, info, resources);
        _p2Field = new GameField(random, this, true, info, resources);

        _p1Field.SetOther(_p2Field);
        _p2Field.SetOther(_p1Field);

        resources.GameplayMusic.Play();

        _announcer = new Announcer(resources);

        ChangeState(GameState.GetReady);
    }

    public Winner Winner { get; private set; }

    protected bool CanPause => _state == GameState.InGame;

    public Winner WinningSide
    {
        get
        {
            if (_p1Field.PlayerClimbingProgress > _p2Field.PlayerClimbingProgress)
                return Winner.P1;
            return _p2Field.PlayerClimbingProgress > _p1Field.PlayerClimbingProgress
                ? Winner.P2
                : Winner.Neither;
        }
    }

    public bool HasMatchEnded => _state == GameState.MatchEnded;

    public bool MatchInCourse => _state == GameState.InGame;

    protected void SetRoundsToWin(byte roundsToWin)
    {
        _p1Field.Scoreboard.RoundsToWin = roundsToWin;
        _p2Field.Scoreboard.RoundsToWin = roundsToWin;
    }

    private void EndRound(Winner winner)
    {
        Winner = winner;
        ChangeState(GameState.PlayerWon);
    }

    private void AnnounceWinner()
    {
        switch (Winner)
        {
            case Winner.Neither:
                _p1Field.WinRound(true);
                _p2Field.WinRound(true);
                _announcer.Say(AnnouncementType.Draw);
                break;
            case Winner.P1:
                _p1Field.WinRound();
                _p2Field.LoseRound();
                _announcer.Say(AnnouncementType.P1Wins);
                break;
            case Winner.P2:
                _p2Field.WinRound();
                _p1Field.LoseRound();
                _announcer.Say(AnnouncementType.P2Wins);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ChangeState(GameState state)
    {
        _stateTimer = 0;
        _state = state;

        switch (_state)
        {
            case GameState.GetReady:
                _announcer.Say(AnnouncementType.GetReady);
                _p1Field.GetReady();
                _p2Field.GetReady();
                break;
            case GameState.InGame:
                _announcer.Say(AnnouncementType.Go);
                _p1Field.BeginRound();
                _p2Field.BeginRound();
                break;
            case GameState.PlayerWon:
                AnnounceWinner();
                break;
            case GameState.MatchEnded:
                OnGameEnded?.Invoke();
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

        _p1Field.Update(p1Input);
        _p2Field.Update(p2Input);
    }

    private void InGameUpdate(GameInput p1Input, GameInput p2Input)
    {
        _p1Field.Update(p1Input);
        _p2Field.Update(p2Input);

        switch (_p1Field.PlayerWonRound)
        {
            case true when _p2Field.PlayerWonRound:
                EndRound(Winner.Neither);
                break;
            case true:
                EndRound(Winner.P1);
                break;
            default:
            {
                if (_p2Field.PlayerWonRound) EndRound(Winner.P2);
                break;
            }
        }
    }

    private void PlayerWonUpdate(GameInput p1Input, GameInput p2Input)
    {
        _p1Field.Update(p1Input);
        _p2Field.Update(p2Input);

        if (_stateTimer != RoundEndTime) return;

        if (_p1Field.PlayerWonGame || _p2Field.PlayerWonGame)
            ChangeState(GameState.MatchEnded);
        else
            ChangeState(GameState.GetReady);
    }

    internal void Update(IInputHandler inputHandler)
    {
        var inputs = inputHandler.GetGameInput();
        switch (_state)
        {
            case GameState.GetReady:
                GetReadyUpdate(inputs.First, inputs.Second);
                break;
            case GameState.InGame:
                InGameUpdate(inputs.First, inputs.Second);
                break;
            case GameState.PlayerWon:
                PlayerWonUpdate(inputs.First, inputs.Second);
                break;
            case GameState.MatchEnded:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ++_stateTimer;
    }

    internal void Draw()
    {
        var p1Dest = new Rectangle(GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);
        var p2Dest = new Rectangle(Width / 2 + GameBorder / 2, GameBorder / 2, GameField.Width, GameField.Height);

        _renderer.DrawColor = Color.Black;
        _renderer.Clear();
        _renderer.Copy(_gameBorder, null, null);

        _p1Field.Draw(_outputTexture);
        _renderer.SetRenderTarget(null);
        _renderer.CopyEx(_outputTexture, null, p1Dest, 0.0, null,
            SDL.SDL_RendererFlip.SDL_FLIP_NONE);

        _p2Field.Draw(_outputTexture);
        _renderer.SetRenderTarget(null);
        _renderer.CopyEx(_outputTexture, null, p2Dest, 0.0, null, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
    }
}